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

        /// <summary>True once the player has placed the touch controls themselves.</summary>
        public static bool CustomControls => S.customControls;

        /// <summary>Saved control centres (fractions of the HUD safe area), in the order stick, attack, dodge, skill.</summary>
        public static Vector2[] ControlPositions => new[] { S.stickPos, S.attackPos, S.dodgePos, S.skillPos };

        public static void SaveControlPositions(Vector2 stick, Vector2 attack, Vector2 dodge, Vector2 skill)
        {
            S.stickPos = stick; S.attackPos = attack; S.dodgePos = dodge; S.skillPos = skill;
            S.customControls = true;
            SaveSystem.Save();
        }

        /// <summary>Flips a saved layout left to right, so switching hands keeps the player's arrangement.</summary>
        public static void MirrorControlPositions()
        {
            if (!S.customControls) return;
            Vector2 M(Vector2 p) => new Vector2(1f - p.x, p.y);
            S.stickPos = M(S.stickPos); S.attackPos = M(S.attackPos); S.dodgePos = M(S.dodgePos); S.skillPos = M(S.skillPos);
            SaveSystem.Save();
        }

        /// <summary>Back to the built-in layout (left-handed or not).</summary>
        public static void ResetControlPositions()
        {
            S.customControls = false;
            SaveSystem.Save();
        }
    }
}
