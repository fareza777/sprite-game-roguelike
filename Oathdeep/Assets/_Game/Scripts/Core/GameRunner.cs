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
    /// Owns the app flow (splash → menu → run → menu) and the run loop. A run is a descent through ten floors;
    /// the hero, gear and HUD persist across floors while each floor's dungeon is rebuilt. Floor building lives in
    /// GameRunner.Floors.cs, pause/inventory/encounter screens in GameRunner.Screens.cs.
    /// </summary>
    public sealed partial class GameRunner : MonoBehaviour
    {
        enum Phase { Boot, Menu, Playing, Paused, Ended }

        [Header("Content")]
        [SerializeField] HeroData[] heroes;
        [SerializeField] EnemyData[] codex;
        [SerializeField] FloorConfig[] floors;
        [SerializeField] PropSet props;
        [SerializeField] Pickup pickupPrefab;
        [SerializeField] Sprite goldSprite;
        [SerializeField] Sprite potionSprite;
        [SerializeField] UISkin skin;
        [SerializeField] ItemIcons itemIcons;
        [SerializeField] AudioLibrary audioLibrary;
        [SerializeField] StoryAssets story;
        [Header("Scene")]
        [SerializeField] Camera worldCamera;
        [SerializeField] Light2D globalLight;
        [SerializeField] AmbientMotes ambient;
        [SerializeField] Enemies.BossFx bossFx;

        Phase phase;
        FrontEnd frontEnd;
        PlayerController player;
        HudView hud;
        RectTransform hudRoot;
        NarrationBanner narration;
        DamageNumbers numbers;
        RunStats stats;
        string lastSummary = "";

        int FinalFloor => floors != null ? floors.Length : 1;

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
            }, story, heroes, codex);
            if (worldCamera != null && worldCamera.GetComponent<CameraRig>() == null) worldCamera.gameObject.AddComponent<CameraRig>();
            if (FindFirstObjectByType<HitStop>() == null) new GameObject("HitStop").AddComponent<HitStop>();
            LootDropper.Configure(pickupPrefab, goldSprite, potionSprite);
            EncounterHost.ShowDialog = ShowEncounterDialog;
            EncounterHost.OpenShop = OpenShop;
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
            QuestTracker.BindRunEvents();
            ExitPortal.ClearListeners();
            TargetRegistry.Clear();
            Pool.Clear();
            LootDropper.Configure(pickupPrefab, goldSprite, potionSprite);

            stats = new RunStats();
            SaveSystem.Current.runs++;

            var hero = HeroesPage.Selected(heroes);
            player = Instantiate(hero.PlayerPrefab).GetComponent<PlayerController>();
            if (player.GetComponent<PlayerInputReader>() == null) player.gameObject.AddComponent<PlayerInputReader>();
            player.Configure(hero);

            hudRoot = UIFactory.Stretch("HUD", frontEnd.SafeArea);
            hudRoot.SetAsFirstSibling();   // below menus/pages so they draw on top
            hud = hudRoot.gameObject.AddComponent<HudView>();
            hud.Build(skin, hudRoot, player, Pause, OpenInventory, hero.SkillIcon);
            GameEvents.RoomEntered += cell => hud.Minimap.Enter(cell);

            numbers = new GameObject("DamageNumbers").AddComponent<DamageNumbers>();
            numbers.Init(skin);

            var inventory = player.GetComponent<PlayerInventory>();
            inventory.BeginRun(hero); // after HUD subscribed so the bars initialise
            var (metaStats, metaSpecials, startGold) = QuestTracker.MetaBonus(SaveSystem.Current);
            inventory.AddRunBonus(metaStats, metaSpecials);
            var health = player.GetComponent<Health>();
            health.Heal(health.Max);
            if (startGold > 0) inventory.AddGold(startGold);
            CameraRig.Follow(player.transform);

            stats.Begin();
            Monetization.OnGameplay();   // no banner during play: it would sit on the touch controls
            reviveUsed = false;
            GameEvents.PlayerDied += () => StartCoroutine(Fallen());
            GameEvents.BossDefeated += () => GameEvents.RaiseToast("The way down is open.");
            GameEvents.ItemFound += _ => AudioManager.Play(Sfx.UiConfirm);
            ExitPortal.Reached += OnPortalReached;

            narration = NarrationBanner.Create(hudRoot, skin);
            GameEvents.BossEncountered += _ => narration.ShowBoss(CurrentStory);
            phase = Phase.Playing;
            EnterFloor(0);
        }

        void TeardownRun()
        {
            TeardownFloor();
            if (player != null) Destroy(player.gameObject);
            if (hud != null) Destroy(hud.gameObject);
            if (numbers != null) Destroy(numbers.gameObject);
            player = null; hud = null; numbers = null; hudRoot = null;
        }

        bool reviveUsed;

        /// <summary>The hero fell: once a run, offer a second chance for a rewarded ad, otherwise end the run.</summary>
        IEnumerator Fallen()
        {
            yield return new WaitForSecondsRealtime(1.2f);
            if (AdPolicy.ReviveOffered(reviveUsed, Monetization.RewardedReady))
            {
                bool decided = false, revived = false;
                var d = new Dungeon.EncounterDialog
                {
                    Speaker = "Second chance",
                    Text = "Not yet. Watch a short ad to rise again with half your health?",
                };
                d.Choices.Add(new Dungeon.EncounterChoice("RISE AGAIN (WATCH AD)", () => Monetization.ShowRewarded(ok => { revived = ok; decided = true; })));
                d.Choices.Add(new Dungeon.EncounterChoice("REST", () => decided = true));
                Dungeon.EncounterHost.Dialog(d);
                while (!decided) yield return null;
                if (revived)
                {
                    reviveUsed = true;
                    Time.timeScale = 1f;
                    player.Revive(0.5f);
                    GameEvents.RaiseToast("You rise again!");
                    yield break;
                }
            }
            yield return EndAfter(0.4f, false);
        }

        IEnumerator EndAfter(float delay, bool victory)
        {
            yield return new WaitForSecondsRealtime(delay);
            phase = Phase.Ended;
            stats.End();
            int reached = CurrentFloor.FloorNumber;
            var save = SaveSystem.Current;
            save.totalKills += stats.Kills;
            save.bestFloor = Mathf.Max(save.bestFloor, reached);
            if (victory) save.victories++; else save.deaths++;
            SaveSystem.Save();
            lastSummary =
                $"Floor reached   B{reached}  {CurrentFloor.Biome?.DisplayName}\n" +
                $"Foes slain      {stats.Kills}\n" +
                $"Rooms cleared   {stats.RoomsCleared}\n" +
                $"Gold            {stats.Gold}\n" +
                $"Highest hit     {stats.HighestHit}\n" +
                $"Time            {RunStats.FormatTime(stats.Duration)}";
            if (victory && story != null) { PlayEnding(); yield break; }
            AudioManager.PlayStinger(victory);
            frontEnd.ShowEnd(victory, lastSummary);
            Time.timeScale = victory ? 1f : 0.25f;
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

        void ToMenu()
        {
            StopAllCoroutines();
            TeardownRun();
            Time.timeScale = 1f;
            phase = Phase.Menu;
            frontEnd.ShowMenu();
            Monetization.OnMenu();
        }

        void OnApplicationPause(bool paused)
        {
            if (!paused || Monetization.ShowingFullScreenAd) return;   // an ad, not the player leaving
            Pause();
            SaveSystem.Save();
        }
    }
}
