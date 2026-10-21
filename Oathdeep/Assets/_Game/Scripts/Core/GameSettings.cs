using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>Player-facing settings, persisted in the save file (see <see cref="SaveSystem"/>).</summary>
    public static class GameSettings
    {
        static SettingsData S => SaveSystem.Current.settings;

        public static bool Vibration { get => S.vibration; set { S.vibration = value; SaveSystem.Save(); } }
        public static bool ReduceMotion { get => S.reduceMotion; set { S.reduceMotion = value; SaveSystem.Save(); } }
        public static bool AutoAim { get => S.autoAim; set { S.autoAim = value; SaveSystem.Save(); } }
        public static float ScreenShake { get => S.screenShake; set { S.screenShake = Mathf.Clamp01(value); SaveSystem.Save(); } }
        // Volumes change continuously while a slider is dragged; the settings page saves when it closes.
        public static float MusicVolume { get => S.music; set => S.music = Mathf.Clamp01(value); }
        public static float SfxVolume { get => S.sfx; set => S.sfx = Mathf.Clamp01(value); }
        public static bool DamageNumbers { get => S.damageNumbers; set { S.damageNumbers = value; SaveSystem.Save(); } }
        public static bool FixedJoystick { get => S.fixedJoystick; set { S.fixedJoystick = value; SaveSystem.Save(); } }
        public static bool LeftHanded { get => S.leftHanded; set { S.leftHanded = value; SaveSystem.Save(); } }
    }
}
