using System;
using Oathdeep.Audio;
using Oathdeep.Core;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace Oathdeep.UI
{
    /// <summary>Fills the Settings and About pages.</summary>
    public static class FrontEndPages
    {
        public sealed class SettingsHooks
        {
            public Action ControlsChanged;   // rebuild the HUD so layout options apply immediately
            public Action CustomizeControls; // opens the drag-to-place control editor
            public Action ReplayIntro;
            public Action ProgressReset;
            public ConfirmDialog Confirm;
        }

        public static void BuildSettings(PageView page, SettingsHooks hooks)
        {
            var skin = page.Skin;
            page.Heading("SOUND");
            UIWidgets.SliderRow(page.Row("Music", 36), skin, "Music", GameSettings.MusicVolume,
                v => { GameSettings.MusicVolume = v; AudioManager.RefreshVolumes(); });
            UIWidgets.SliderRow(page.Row("Effects", 36), skin, "Effects", GameSettings.SfxVolume,
                v => { GameSettings.SfxVolume = v; AudioManager.Play(Sfx.UiClick); });

            page.Heading("CONTROLS");
            UIWidgets.ToggleRow(page.Row("Joystick", 36), skin, "Joystick", () => GameSettings.FixedJoystick,
                v => { GameSettings.FixedJoystick = v; hooks.ControlsChanged?.Invoke(); }, "FIXED", "FLOATING");
            UIWidgets.ToggleRow(page.Row("LeftHanded", 36), skin, "Left-handed", () => GameSettings.LeftHanded,
                v => { GameSettings.LeftHanded = v; GameSettings.MirrorControlPositions(); hooks.ControlsChanged?.Invoke(); });
            if (hooks.CustomizeControls != null)
                UIFactory.Button(page.Row("Layout", 40), skin, "CUSTOMIZE LAYOUT", 14, hooks.CustomizeControls);
            UIWidgets.ToggleRow(page.Row("AutoAim", 36), skin, "Auto-aim", () => GameSettings.AutoAim, v => GameSettings.AutoAim = v);
            UIWidgets.ToggleRow(page.Row("Vibration", 36), skin, "Vibration", () => GameSettings.Vibration, v => GameSettings.Vibration = v);

            page.Heading("DISPLAY");
            UIWidgets.ToggleRow(page.Row("Shake", 36), skin, "Screen shake", () => GameSettings.ScreenShake > 0f,
                v => GameSettings.ScreenShake = v ? 1f : 0f);
            UIWidgets.ToggleRow(page.Row("Numbers", 36), skin, "Damage numbers", () => GameSettings.DamageNumbers, v => GameSettings.DamageNumbers = v);
            UIWidgets.ToggleRow(page.Row("Motion", 36), skin, "Reduce motion", () => GameSettings.ReduceMotion, v => GameSettings.ReduceMotion = v);

            page.Heading("ADS");
            if (Monetization.AdsRemoved)
                page.Paragraph("Ads removed. Thank you for supporting UNDERSUN!", 11, skin.accentColor, TextAlignmentOptions.Center);
            else
            {
                Button removeAds = null;
                removeAds = UIFactory.Button(page.Row("RemoveAds", 40), skin, $"REMOVE ADS - {Monetization.RemoveAdsPrice}", 14, () =>
                {
                    removeAds.interactable = false;
                    Monetization.BuyRemoveAds(ok =>
                    {
                        removeAds.interactable = !ok;
                        removeAds.GetComponentInChildren<TextMeshProUGUI>().text = ok ? "ADS REMOVED - THANK YOU!" : $"REMOVE ADS - {Monetization.RemoveAdsPrice}";
                    });
                });
                // The store reports the local price (and any restored purchase) a moment after launch.
                System.Action refresh = null;
                refresh = () =>
                {
                    if (removeAds == null) { Monetization.Changed -= refresh; return; }
                    removeAds.interactable = !Monetization.AdsRemoved;
                    removeAds.GetComponentInChildren<TextMeshProUGUI>().text =
                        Monetization.AdsRemoved ? "ADS REMOVED - THANK YOU!" : $"REMOVE ADS - {Monetization.RemoveAdsPrice}";
                };
                Monetization.Changed += refresh;
            }
            Button restore = null;
            restore = UIFactory.Button(page.Row("Restore", 36), skin, "RESTORE PURCHASES", 12, () =>
            {
                restore.interactable = false;
                Monetization.RestorePurchases(ok =>
                {
                    restore.interactable = true;
                    restore.GetComponentInChildren<TextMeshProUGUI>().text = Monetization.AdsRemoved ? "PURCHASES RESTORED" : "NOTHING TO RESTORE";
                });
            });

            page.Heading("STORY & DATA");
            if (hooks.ReplayIntro != null)
                UIFactory.Button(page.Row("Replay", 40), skin, "REPLAY PROLOGUE", 14, hooks.ReplayIntro);
            UIFactory.Button(page.Row("Reset", 40), skin, "RESET PROGRESS", 14, () =>
                hooks.Confirm.Ask("Erase codex, quests and records?\nSettings are kept.", () =>
                {
                    SaveSystem.ResetProgress();
                    hooks.ProgressReset?.Invoke();
                }));
        }

        public static void BuildAbout(PageView page, string version)
        {
            var skin = page.Skin;
            var logoRow = page.Row("Logo", 110);
            var logo = UIFactory.Rect("Img", logoRow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(216, 108));
            UIFactory.Image(logo, skin.logo);
            page.Paragraph(version, 10, skin.mutedColor, TextAlignmentOptions.Center);

            page.Heading("THE STORY");
            page.Paragraph("Nine hundred years ago the Choir of Dawn sang the Sun down into the earth to hide her " +
                           "from a Hunger in the sky. Ten Choristers stayed below to hold the ten verses of that hymn. " +
                           "The world above has lived in dusk ever since.\n\nNow the Sun is singing back. She calls for the " +
                           "Unsung - those no hymn ever named - to descend, silence each verse, and raise her.");

            page.Heading("CREDITS");
            page.Paragraph("Created by fareza777\n\nDesign, code, pixel art, music and sound were made for UNDERSUN " +
                           "with Unity and Aseprite. Pixel font based on a public-domain (CC0) 5x7 font.");

            page.Heading("PRIVACY");
            page.Paragraph("UNDERSUN plays fully offline. It collects no personal data and has no ads or tracking. " +
                           "Your progress is stored only on this device.");
        }
    }
}
