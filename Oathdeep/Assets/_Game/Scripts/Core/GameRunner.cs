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
        [SerializeField] HeroData[] heroes;
        [SerializeField] FloorConfig floor;
        [SerializeField] PropSet props;
        [SerializeField] Pickup pickupPrefab;
        [SerializeField] Sprite goldSprite;
        [SerializeField] Sprite potionSprite;
        [SerializeField] UISkin skin;
        [SerializeField] ItemIcons itemIcons;
        [SerializeField] AudioLibrary audioLibrary;
        [SerializeField] StoryAssets story;
        [SerializeField] int finalFloor = 10;
        [Header("Scene")]
        [SerializeField] Camera worldCamera;
        [SerializeField] Light2D globalLight;

        Phase phase;
        FrontEnd frontEnd;
        DungeonBuilder dungeon;
        PlayerController player;
        HudView hud;
        NarrationBanner narration;
        DamageNumbers numbers;
        RunStats stats;

        void Awake()
        {
            Application.targetFrameRate = 60;
            AudioManager.Create(audioLibrary);
            ItemIcons.Use(itemIcons);
            frontEnd = new GameObject("FrontEnd").AddComponent<FrontEnd>();
            frontEnd.Build(skin, new FrontEnd.Hooks
            {
                StartRun = StartRun,
                ResumeRun = Resume,
                AbandonRun = ToMenu,
                ControlsChanged = () => { if (hud != null) hud.RebuildControls(); },
                OpenInventory = OpenInventory,
                ReplayIntro = () => PlayPrologue(ShowMenuAfterCinematic),
            }, story, heroes);
            if (worldCamera != null && worldCamera.GetComponent<CameraRig>() == null) worldCamera.gameObject.AddComponent<CameraRig>();
            if (FindFirstObjectByType<HitStop>() == null) new GameObject("HitStop").AddComponent<HitStop>();
            LootDropper.Configure(pickupPrefab, goldSprite, potionSprite);
        }

        void Start()
        {
            phase = Phase.Boot;
            frontEnd.PlaySplash(() =>
            {
                if (SaveSystem.Current.introSeen) ToMenu();
                else PlayPrologue(ShowMenuAfterCinematic);
            });
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
            LootDropper.BeginFloor(floor.FloorNumber, seed);
            stats = new RunStats();
            SaveSystem.Current.runs++;

            var hero = HeroesPage.Selected(heroes);
            player = Instantiate(hero.PlayerPrefab).GetComponent<PlayerController>();
            var inventory = player.GetComponent<PlayerInventory>();
            if (player.GetComponent<PlayerInputReader>() == null) player.gameObject.AddComponent<PlayerInputReader>();
            player.Configure(hero);

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
            hud.Build(skin, hudRoot, player, Pause, OpenInventory, hero.SkillIcon);
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
            GameEvents.ItemFound += _ => AudioManager.Play(Sfx.UiConfirm);
            ExitPortal.Reached += () => StartCoroutine(EndAfter(0.4f, true));

            phase = Phase.Playing;
            narration = NarrationBanner.Create(hudRoot, skin);
            var floorStory = story != null ? story.Data.Floor(floor.FloorNumber) : null;
            GameEvents.BossEncountered += _ => narration.ShowBoss(floorStory);
            StartCoroutine(IntroduceFloor(floorStory, hudRoot));
        }

        /// <summary>The Sun names the floor first; first-time players get the tutorial once she has spoken.</summary>
        IEnumerator IntroduceFloor(FloorStory floorStory, RectTransform hudRoot)
        {
            float wait = 0f;
            if (floorStory != null)
            {
                var clip = story.VoiceFor(floorStory.id);
                narration.ShowFloor(floorStory, clip);
                wait = Mathf.Max(5.5f, clip != null ? clip.length + 0.5f : 0f) + 1.3f;
            }
            if (SaveSystem.Current.onboardingDone) yield break;
            yield return new WaitForSecondsRealtime(wait);
            if (hud != null && player != null) OnboardingCoach.Begin(hudRoot, skin, hud, player);
        }

        // ---------------- Cinematics ----------------

        void PlayPrologue(System.Action then)
        {
            phase = Phase.Menu;
            frontEnd.HideMenu();
            frontEnd.Cinematic.Play(story.Data.prologue, MusicTrack.Prologue, () =>
            {
                SaveSystem.Current.introSeen = true;
                SaveSystem.Save();
                then?.Invoke();
            });
        }

        void ShowMenuAfterCinematic() => ToMenu();

        void PlayEnding()
        {
            TeardownRun();
            Time.timeScale = 1f;
            frontEnd.HideRunScreens();
            frontEnd.Cinematic.Play(story.Data.ending, MusicTrack.Ending, () => frontEnd.ShowEnd(true, lastSummary));
        }

        string lastSummary = "";

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
            lastSummary = summary;
            if (victory && floor.FloorNumber >= finalFloor && story != null) { PlayEnding(); yield break; }
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

        /// <summary>From play (HUD bag) the game pauses under the page and resumes on close; from the pause menu it returns there.</summary>
        void OpenInventory()
        {
            if (player == null) return;
            var inventory = player.GetComponent<PlayerInventory>();
            if (phase == Phase.Playing)
            {
                phase = Phase.Paused;
                Time.timeScale = 0f;
                frontEnd.ShowInventory(inventory, Resume);
            }
            else if (phase == Phase.Paused)
                frontEnd.ShowInventory(inventory, null);
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
