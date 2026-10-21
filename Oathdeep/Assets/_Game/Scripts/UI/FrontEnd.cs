using System;
using System.Collections.Generic;
using Oathdeep.Audio;
using Oathdeep.Core;
using Oathdeep.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Oathdeep.UI
{
    /// <summary>
    /// Owns the overlay canvas and every out-of-run screen: splash, main menu, settings, about, confirm,
    /// plus the pause / fallen / victory screens. The run itself is driven by GameRunner through Hooks.
    /// </summary>
    public sealed class FrontEnd : MonoBehaviour
    {
        public sealed class Hooks
        {
            public Action StartRun;
            public Action ResumeRun;
            public Action AbandonRun;
            public Action ControlsChanged;
            public Action ReplayIntro;        // null until the prologue exists
            public Action OpenInventory;
        }

        public const string Version = "UNDERSUN  v1.0.0";

        UISkin skin;
        Hooks hooks;
        SplashView splash;
        MainMenuView menu;
        PageView settingsPage, aboutPage;
        HeroesPage heroesPage;
        ItemCompendiumPage itemsPage;
        CodexPage codexPage;
        QuestPage questPage;
        InventoryPage inventoryPage;
        Action inventoryClosed;
        EncounterDialogView dialog;
        ShopPage shopPage;
        Action shopClosed;
        ScreenView pauseScreen, fallenScreen, victoryScreen;

        public RectTransform CanvasRoot { get; private set; }
        public RectTransform SafeArea { get; private set; }
        public ConfirmDialog Confirm { get; private set; }

        public CinematicView Cinematic { get; private set; }

        public void Build(UISkin uiSkin, Hooks runHooks, StoryAssets story, IReadOnlyList<HeroData> heroes, IReadOnlyList<EnemyData> codex)
        {
            skin = uiSkin;
            hooks = runHooks;
            BuildCanvas();
            menu = MainMenuView.Create(SafeArea, skin, new MainMenuView.Actions
            {
                Descend = hooks.StartRun,
                Heroes = heroes != null && heroes.Count > 0 ? () => heroesPage.Show() : (Action)null,
                Items = () => itemsPage.Show(),
                Codex = codex != null && codex.Count > 0 ? () => codexPage.Show() : (Action)null,
                Quests = () => questPage.Show(),
                Settings = () => OpenSettings(),
                About = () => aboutPage.Show(),
                Share = () => AppLinks.Share("I'm descending into UNDERSUN. The sun was buried - come help me raise it."),
                Rate = AppLinks.OpenStorePage,
            }, Version);
            aboutPage = PageView.Create("About", SafeArea, skin, "ABOUT", null);
            FrontEndPages.BuildAbout(aboutPage, Version);
            if (heroes != null && heroes.Count > 0) heroesPage = HeroesPage.Create(SafeArea, skin, heroes);
            itemsPage = ItemCompendiumPage.Create(SafeArea, skin);
            if (codex != null && codex.Count > 0) codexPage = CodexPage.Create(SafeArea, skin, codex);
            questPage = QuestPage.Create(SafeArea, skin);
            inventoryPage = InventoryPage.Create(SafeArea, skin, () => inventoryClosed?.Invoke());
            shopPage = ShopPage.Create(SafeArea, skin, () => { var c = shopClosed; shopClosed = null; c?.Invoke(); });
            dialog = EncounterDialogView.Create(SafeArea, skin);
            Confirm = ConfirmDialog.Create(SafeArea, skin);
            BuildRunScreens();
            Cinematic = CinematicView.Create(SafeArea, skin, story);
            splash = SplashView.Create(SafeArea, skin, "a game by fareza777");
        }

        // ---------------- flow ----------------

        public void PlaySplash(Action then) => splash.Play(then);

        public void ShowMenu()
        {
            HideRunScreens();
            menu.Show();
            AudioManager.PlayMusic(MusicTrack.Title);
        }

        public void HideMenu() => menu.Hide();

        public void ShowPause() => pauseScreen.Show();

        CanvasGroup fader;

        /// <summary>Full-screen black fade used between floors. Runs on unscaled time.</summary>
        public System.Collections.IEnumerator FadeOut(float seconds) => Fade(0f, 1f, seconds);
        public System.Collections.IEnumerator FadeIn(float seconds) => Fade(1f, 0f, seconds);

        System.Collections.IEnumerator Fade(float from, float to, float seconds)
        {
            if (fader == null)
            {
                var rt = UIFactory.Stretch("Fader", CanvasRoot);
                var img = UIFactory.Image(rt, skin.white, Color.black, false, true);
                img.preserveAspect = false;
                fader = rt.gameObject.AddComponent<CanvasGroup>();
            }
            fader.transform.SetAsLastSibling();
            fader.gameObject.SetActive(true);
            fader.blocksRaycasts = true;
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                fader.alpha = Mathf.Lerp(from, to, t / seconds);
                yield return null;
            }
            fader.alpha = to;
            fader.blocksRaycasts = to > 0.5f;
            if (to <= 0f) fader.gameObject.SetActive(false);
        }

        public void ShowDialog(Dungeon.EncounterDialog d, Action onClosed) => dialog.Show(d, onClosed);

        public void ShowShop(string title, Player.PlayerInventory inventory, System.Collections.Generic.List<Dungeon.ShopOffer> stock, Action onClosed)
        {
            shopClosed = onClosed;
            shopPage.Show(title, inventory, stock);
        }

        /// <summary>Opens the equipment page for this run; <paramref name="onClosed"/> runs when the player backs out.</summary>
        public void ShowInventory(Player.PlayerInventory inventory, Action onClosed)
        {
            inventoryClosed = onClosed;
            inventoryPage.Show(inventory);
        }
        public void HidePause() => pauseScreen.Hide();

        public void ShowEnd(bool victory, string summary)
        {
            var screen = victory ? victoryScreen : fallenScreen;
            screen.SetBody(summary);
            screen.Show();
        }

        public void HideRunScreens()
        {
            inventoryClosed = null;
            inventoryPage.Hide();
            shopClosed = null;
            shopPage.Hide();
            dialog.Close();
            pauseScreen.Hide();
            fallenScreen.Hide();
            victoryScreen.Hide();
        }

        void OpenSettings()
        {
            if (settingsPage != null) Destroy(settingsPage.gameObject);   // rebuild so values are current
            settingsPage = PageView.Create("Settings", SafeArea, skin, "SETTINGS", SaveSystem.Save);
            FrontEndPages.BuildSettings(settingsPage, new FrontEndPages.SettingsHooks
            {
                ControlsChanged = hooks.ControlsChanged,
                ReplayIntro = hooks.ReplayIntro == null ? null : () => { settingsPage.Hide(); hooks.ReplayIntro(); },
                Confirm = Confirm,
            });
            settingsPage.Show();
        }

        // ---------------- construction ----------------

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
            CanvasRoot = (RectTransform)canvasGo.transform;
            SafeArea = UIFactory.Stretch("SafeArea", CanvasRoot);
            SafeArea.gameObject.AddComponent<SafeAreaFitter>();

            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        void BuildRunScreens()
        {
            pauseScreen = ScreenView.Create("Pause", SafeArea, skin, "PAUSED", "",
                new (string, Action)[]
                {
                    ("RESUME", hooks.ResumeRun),
                    ("INVENTORY", () => hooks.OpenInventory?.Invoke()),
                    ("SETTINGS", () => OpenSettings()),
                    ("ABANDON RUN", () => Confirm.Ask("Abandon this descent?\nYour run will end.", hooks.AbandonRun)),
                });
            fallenScreen = ScreenView.Create("Fallen", SafeArea, skin, "FALLEN", "",
                new (string, Action)[] { ("DESCEND AGAIN", hooks.StartRun), ("MAIN MENU", hooks.AbandonRun) });
            victoryScreen = ScreenView.Create("Victory", SafeArea, skin, "VICTORY", "",
                new (string, Action)[] { ("DESCEND AGAIN", hooks.StartRun), ("MAIN MENU", hooks.AbandonRun) });
        }
    }
}
