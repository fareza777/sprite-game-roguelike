using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>Short vibration pulses on Android; no-op elsewhere or when disabled in settings.</summary>
    public static class Haptics
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        static AndroidJavaObject vibrator;
        static bool initialised;

        static AndroidJavaObject Vibrator()
        {
            if (initialised) return vibrator;
            initialised = true;
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Haptics unavailable: {e.Message}");
            }
            return vibrator;
        }
#endif

        public static void Pulse(int milliseconds)
        {
            if (!GameSettings.Vibration || milliseconds <= 0) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            var v = Vibrator();
            if (v == null) return;
            try
            {
                using var effectClass = new AndroidJavaClass("android.os.VibrationEffect");
                using var effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", (long)milliseconds, -1);
                v.Call("vibrate", effect);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Haptic pulse failed: {e.Message}");
            }
#endif
        }
    }
}
