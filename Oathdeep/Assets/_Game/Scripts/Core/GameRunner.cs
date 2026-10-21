using System.Collections;
using Oathdeep.Audio;
using Oathdeep.Combat;
using Oathdeep.Data;
using Oathdeep.Dungeon;
using Oathdeep.Loot;
using Oathdeep.Player;
using Oathdeep.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Oathdeep.Core
{
    /// <summary>
    /// Owns the run loop: title → build floor → play → game over / victory → restart.
    /// Everything per-run (dungeon, player, HUD) is rebuilt so no state leaks between runs.
    /// </summary>
    public sealed class GameRunner : MonoBehaviour
    {
        enum Phase { Title, Playing, Paused, Ended }

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
        DungeonBuilder dungeon;
        PlayerController player;
        RectTransform canvasRoot;
        RectTransform safeArea;
        HudView hud;
        DamageNumbers numbers;
        ScreenView titleScreen, pauseScreen, overScreen, victoryScreen;
        RunStats stats;

        void Awake()
        {
            Application.targetFrameRate = 60;
            BuildCanvas();
            BuildScreens();
            if (worldCamera != null && worldCamera.GetComponent<CameraRig>() == null) worldCamera.gameObject.AddComponent<CameraRig>();
            if (FindFirstObjectByType<HitStop>() == null) new GameObject("HitStop").AddComponent<HitStop>();
            LootDropper.Configure(pickupPrefab, goldSprite, potionSprite);
            AudioManager.Create(audioLibrary);
        }

        void Start()
        {
            phase = Phase.Title;
            titleScreen.Show();
            AudioManager.PlayMusic(MusicTrack.Title);
        }

        // ---------------- Run lifecycle ----------------

        public void StartRun()
        {
            StopAllCoroutines();
            Time.timeScale = 1f;
            TeardownRun();
            GameEvents.ClearAll();
            AudioManager.BindRunEvents();
            AudioManager.PlayMusic(MusicTrack.Crypt);
            ExitPortal.ClearListeners();
            TargetRegistry.Clear();
            Pool.Clear();
            LootDropper.Configure(pickupPrefab, goldSprite, potionSprite);

            int seed = System.Environment.TickCount;
            stats = new RunStats();

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

            var hudRoot = UIFactory.Stretch("HUD", safeArea);
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
            HideScreens();
            GameEvents.RaiseToast(floor.Biome != null ? floor.Biome.DisplayName : "The Deep");
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
            string summary =
                $"Floor reached   B{floor.FloorNumber}\n" +
                $"Foes slain      {stats.Kills}\n" +
                $"Rooms cleared   {stats.RoomsCleared}\n" +
                $"Gold            {stats.Gold}\n" +
                $"Highest hit     {stats.HighestHit}\n" +
                $"Time            {RunStats.FormatTime(stats.Duration)}";
            AudioManager.PlayStinger(victory);
            var screen = victory ? victoryScreen : overScreen;
            screen.SetBody(summary);
            screen.Show();
            Time.timeScale = victory ? 1f : 0.25f;
        }

        // ---------------- Pause ----------------

        public void Pause()
        {
            if (phase != Phase.Playing) return;
            phase = Phase.Paused;
            Time.timeScale = 0f;
            pauseScreen.Show();
        }

        public void Resume()
        {
            if (phase != Phase.Paused) return;
            phase = Phase.Playing;
            Time.timeScale = 1f;
            pauseScreen.Hide();
        }

        void ToTitle()
        {
            TeardownRun();
            Time.timeScale = 1f;
            phase = Phase.Title;
            HideScreens();
            titleScreen.Show();
            AudioManager.PlayMusic(MusicTrack.Title);
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) Pause();
        }

        // ---------------- UI scaffolding ----------------

        void BuildCanvas()
        {
            var canvasGo = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(360, 640);
            scaler.matchWidthOrHeight = 0.5f;
            canvasRoot = (RectTransform)canvasGo.transform;
            safeArea = UIFactory.Stretch("SafeArea", canvasRoot);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        void BuildScreens()
        {
            titleScreen = ScreenView.Create("Title", safeArea, skin, "OATHDEEP",
                "Sworn to descend. Sworn to return.\n\nMove: left thumb   Strike: right thumb",
                new (string, System.Action)[] { ("BEGIN", StartRun) }, showLogo: true);
            pauseScreen = ScreenView.Create("Pause", safeArea, skin, "PAUSED", SoundLabel(),
                new (string, System.Action)[] { ("RESUME", Resume), ("SOUND", CycleSound), ("RESTART", StartRun), ("TITLE", ToTitle) });
            overScreen = ScreenView.Create("GameOver", safeArea, skin, "FALLEN", "",
                new (string, System.Action)[] { ("RISE AGAIN", StartRun), ("TITLE", ToTitle) });
            victoryScreen = ScreenView.Create("Victory", safeArea, skin, "FLOOR CLEARED", "",
                new (string, System.Action)[] { ("DESCEND AGAIN", StartRun), ("TITLE", ToTitle) });
        }

        /// <summary>Sound button cycles: all on -> music off -> all off.</summary>
        void CycleSound()
        {
            bool music = GameSettings.MusicVolume > 0f, sfx = GameSettings.SfxVolume > 0f;
            if (music && sfx) GameSettings.MusicVolume = 0f;
            else if (sfx) GameSettings.SfxVolume = 0f;
            else { GameSettings.MusicVolume = 0.8f; GameSettings.SfxVolume = 1f; }
            AudioManager.RefreshVolumes();
            pauseScreen.SetBody(SoundLabel());
        }

        static string SoundLabel() =>
            $"Music  {(GameSettings.MusicVolume > 0f ? "ON" : "OFF")}     Effects  {(GameSettings.SfxVolume > 0f ? "ON" : "OFF")}";

        void HideScreens()
        {
            titleScreen.Hide(); pauseScreen.Hide(); overScreen.Hide(); victoryScreen.Hide();
        }
    }
}
