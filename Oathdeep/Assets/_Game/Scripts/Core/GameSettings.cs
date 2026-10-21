using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>Player-facing settings persisted in PlayerPrefs. Full settings screen comes after the slice.</summary>
    public static class GameSettings
    {
        const string VibrationKey = "settings.vibration";
        const string ReduceMotionKey = "settings.reduceMotion";
        const string AutoAimKey = "settings.autoAim";
        const string ShakeKey = "settings.shake";

        public static bool Vibration
        {
            get => PlayerPrefs.GetInt(VibrationKey, 1) == 1;
            set => PlayerPrefs.SetInt(VibrationKey, value ? 1 : 0);
        }

        public static bool ReduceMotion
        {
            get => PlayerPrefs.GetInt(ReduceMotionKey, 0) == 1;
            set => PlayerPrefs.SetInt(ReduceMotionKey, value ? 1 : 0);
        }

        public static bool AutoAim
        {
            get => PlayerPrefs.GetInt(AutoAimKey, 1) == 1;
            set => PlayerPrefs.SetInt(AutoAimKey, value ? 1 : 0);
        }

        public static float ScreenShake
        {
            get => PlayerPrefs.GetFloat(ShakeKey, 1f);
            set => PlayerPrefs.SetFloat(ShakeKey, Mathf.Clamp01(value));
        }

        const string MusicKey = "settings.musicVolume";
        const string SfxKey = "settings.sfxVolume";

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicKey, 0.8f);
            set => PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value));
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(SfxKey, 1f);
            set => PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value));
        }
    }
}
