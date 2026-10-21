using System.Collections;
using Oathdeep.Audio;
using Oathdeep.Combat;
using Oathdeep.Data;
using Oathdeep.Dungeon;
using Oathdeep.Loot;
using Oathdeep.Player;
using Oathdeep.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Oathdeep.Core
{
    /// <summary>
    /// Owns the app flow (splash → menu → run → menu) and the run loop. Everything per-run (dungeon, player, HUD)
    /// is rebuilt so no state leaks between runs. Out-of-run screens live in <see cref="FrontEnd"/>.
    /// </summary>
    public sealed class GameRunner : MonoBehaviour
    {
        enum Phase { Boot, Menu, Playing, Paused, Ended }

        [Header("Content")]
        [SerializeField] HeroData hero;
        [SerializeField] FloorConfig floor;
        [SerializeField] PropSet props;
        [SerializeField] PlayerController playerPrefab;
        [SerializeField] Pickup pickupPrefab;
        [SerializeField] Sprite goldSprite;
        [SerializeField] Sprite potionSprite;
        [SerializeField] UISkin skin;
        [SerializeField] AudioLibrary audioLibrary;
        [Header("Scene")]
        [SerializeField] Camera worldCamera;
        [SerializeField] Light2D globalLight;

        Phase phase;
        FrontEnd frontEnd;
        DungeonBuilder dungeon;
        PlayerController player;
        HudView hud;
        DamageNumbers numbers;
        RunStats stats;

        void Awake()
        {
            Application.targetFrameRate = 60;
            AudioManager.Create(audioLibrary);
            frontEnd = new GameObject("FrontEnd").AddComponent<FrontEnd>();
            frontEnd.Build(skin, new FrontEnd.Hooks
            {
                StartRun = StartRun,
                ResumeRun = Resume,
                AbandonRun = ToMenu,
                ControlsChanged = () => { if (hud != null) hud.RebuildControls(); },
            });
            if (worldCamera != null && worldCamera.GetComponent<CameraRig>() == null) worldCamera.gameObject.AddComponent<CameraRig>();
            if (FindFirstObjectByType<HitStop>() == null) new GameObject("HitStop").AddComponent<HitStop>();
            LootDropper.Configure(pickupPrefab, goldSprite, potionSprite);
        }

        void Start()
        {
            phase = Phase.Boot;
            frontEnd.PlaySplash(ToMenu);
        }

        // ---------------- Run lifecycle ----------------

        public void StartRun()
        {
            StopAllCoroutines();
            Time.timeScale = 1f;
            TeardownRun();
            frontEnd.HideMenu();
            frontEnd.HideRunScreens();
            GameEvents.ClearAll();
            AudioManager.BindRunEvents();
            AudioManager.PlayMusic(MusicTrack.Crypt);
            ExitPortal.ClearListeners();
            TargetRegistry.Clear();
            Pool.Clear();
            LootDropper.Configure(pickupPrefab, goldSprite, potionSprite);

            int seed = System.Environment.TickCount;
            stats = new RunStats();
            SaveSystem.Current.runs++;

            player = Instantiate(playerPrefab);
            var inventory = player.GetComponent<PlayerInventory>();
            if (player.GetComponent<PlayerInputReader>() == null) player.gameObject.AddComponent<PlayerInputReader>();
            player.SetCooldowns(hero.DodgeCooldown, hero.SkillCooldown);

            var dungeonGo = new GameObject("Dungeon");
            dungeon = dungeonGo.AddComponent<DungeonBuilder>();
            dungeon.SetProps(props);
            dungeon.Build(floor, seed, player.transform, OnBossRoomCleared);
            player.transform.position = dungeon.StartPosition;
            if (globalLight != null && floor.Biome != null)
            {
                globalLight.color = floor.Biome.AmbientLight;
                globalLight.intensity = floor.Biome.AmbientIntensity;
            }

            var hudRoot = UIFactory.Stretch("HUD", frontEnd.SafeArea);
            hudRoot.SetAsFirstSibling();   // below menus/pages so they draw on top
            hud = hudRoot.gameObject.AddComponent<HudView>();
            hud.Build(skin, hudRoot, player, Pause);
            hud.SetFloor(floor.FloorNumber, floor.Biome != null ? floor.Biome.DisplayName : "The Deep");
            hud.Minimap.Show(dungeon.Geometry.Rooms);
            GameEvents.RoomEntered += hud.Minimap.Enter;

            numbers = new GameObject("DamageNumbers").AddComponent<DamageNumbers>();
            numbers.Init(skin);

            inventory.BeginRun(hero); // after HUD subscribed so the bars initialise
            CameraRig.Follow(player.transform);

            stats.Begin();
            GameEvents.PlayerDied += () => StartCoroutine(EndAfter(1.6f, false));
            GameEvents.BossDefeated += () => GameEvents.RaiseToast("The way down is open.");
            ExitPortal.Reached += () => StartCoroutine(EndAfter(0.4f, true));

            phase = Phase.Playing;
            GameEvents.RaiseToast(floor.Biome != null ? floor.Biome.DisplayName : "The Deep");
            if (!SaveSystem.Current.onboardingDone) OnboardingCoach.Begin(hudRoot, skin, hud, player);
        }

        void TeardownRun()
        {
            if (dungeon != null) Destroy(dungeon.gameObject);
            if (player != null) Destroy(player.gameObject);
            if (hud != null) Destroy(hud.gameObject);
            if (numbers != null) Destroy(numbers.gameObject);
            dungeon = null; player = null; hud = null; numbers = null;
        }

        void OnBossRoomCleared(Vector3 at)
        {
            if (props != null && props.exitPortal != null) Instantiate(props.exitPortal, at, Quaternion.identity, dungeon.transform);
        }

        IEnumerator EndAfter(float delay, bool victory)
        {
            yield return new WaitForSecondsRealtime(delay);
            phase = Phase.Ended;
            stats.End();
            var save = SaveSystem.Current;
            save.totalKills += stats.Kills;
            save.bestFloor = Mathf.Max(save.bestFloor, floor.FloorNumber);
            if (victory) save.victories++; else save.deaths++;
            SaveSystem.Save();
            string summary =
                $"Floor reached   B{floor.FloorNumber}\n" +
                $"Foes slain      {stats.Kills}\n" +
                $"Rooms cleared   {stats.RoomsCleared}\n" +
                $"Gold            {stats.Gold}\n" +
                $"Highest hit     {stats.HighestHit}\n" +
                $"Time            {RunStats.FormatTime(stats.Duration)}";
            AudioManager.PlayStinger(victory);
            frontEnd.ShowEnd(victory, summary);
            Time.timeScale = victory ? 1f : 0.25f;
        }

        // ---------------- Pause / menu ----------------

        public void Pause()
        {
            if (phase != Phase.Playing) return;
            phase = Phase.Paused;
            Time.timeScale = 0f;
            frontEnd.ShowPause();
        }

        public void Resume()
        {
            if (phase != Phase.Paused) return;
            phase = Phase.Playing;
            Time.timeScale = 1f;
            frontEnd.HidePause();
        }

        void ToMenu()
        {
            StopAllCoroutines();
            TeardownRun();
            Time.timeScale = 1f;
            phase = Phase.Menu;
            frontEnd.ShowMenu();
        }

        void OnApplicationPause(bool paused)
        {
            if (!paused) return;
            Pause();
            SaveSystem.Save();
        }
    }
}
