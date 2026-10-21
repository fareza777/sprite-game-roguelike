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
            public Action OpenCodex, OpenQuests;   // null hides the menu button
        }

        public const string Version = "UNDERSUN  v0.2.0";

        UISkin skin;
        Hooks hooks;
        SplashView splash;
        MainMenuView menu;
        PageView settingsPage, aboutPage;
        HeroesPage heroesPage;
        ScreenView pauseScreen, fallenScreen, victoryScreen;

        public RectTransform CanvasRoot { get; private set; }
        public RectTransform SafeArea { get; private set; }
        public ConfirmDialog Confirm { get; private set; }

        public CinematicView Cinematic { get; private set; }

        public void Build(UISkin uiSkin, Hooks runHooks, StoryAssets story, IReadOnlyList<HeroData> heroes)
        {
            skin = uiSkin;
            hooks = runHooks;
            BuildCanvas();
            menu = MainMenuView.Create(SafeArea, skin, new MainMenuView.Actions
            {
                Descend = hooks.StartRun,
                Heroes = heroes != null && heroes.Count > 0 ? () => heroesPage.Show() : (Action)null,
                Codex = hooks.OpenCodex,
                Quests = hooks.OpenQuests,
                Settings = () => OpenSettings(),
                About = () => aboutPage.Show(),
                Share = () => AppLinks.Share("I'm descending into UNDERSUN. The sun was buried - come help me raise it."),
                Rate = AppLinks.OpenStorePage,
            }, Version);
            aboutPage = PageView.Create("About", SafeArea, skin, "ABOUT", null);
            FrontEndPages.BuildAbout(aboutPage, Version);
            if (heroes != null && heroes.Count > 0) heroesPage = HeroesPage.Create(SafeArea, skin, heroes);
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
        public void HidePause() => pauseScreen.Hide();

        public void ShowEnd(bool victory, string summary)
        {
            var screen = victory ? victoryScreen : fallenScreen;
            screen.SetBody(summary);
            screen.Show();
        }

        public void HideRunScreens()
        {
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
