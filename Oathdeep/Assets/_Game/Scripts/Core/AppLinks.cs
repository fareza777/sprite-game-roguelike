using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>Store page, rating and system share sheet. Android uses intents; elsewhere falls back to the browser.</summary>
    public static class AppLinks
    {
        public const string PackageId = "com.fareza777.undersun";
        public const string StoreUrl = "https://play.google.com/store/apps/details?id=" + PackageId;
        const string MarketUrl = "market://details?id=" + PackageId;

        public static void OpenStorePage()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try { Application.OpenURL(MarketUrl); return; }
            catch (System.Exception e) { Debug.LogWarning("[AppLinks] market intent failed: " + e.Message); }
#endif
            Application.OpenURL(StoreUrl);
        }

        public static void Share(string message)
        {
            string text = message + "\n" + StoreUrl;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var intentClass = new AndroidJavaClass("android.content.Intent");
                using var intent = new AndroidJavaObject("android.content.Intent");
                intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
                intent.Call<AndroidJavaObject>("setType", "text/plain");
                intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), text);
                using var chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, "Share UNDERSUN");
                // Works for both Activity and GameActivity entry points (Unity 6).
                UnityEngine.Android.AndroidApplication.currentActivity.Call("startActivity", chooser);
                return;
            }
            catch (System.Exception e) { Debug.LogWarning("[AppLinks] share intent failed: " + e.Message); }
#endif
            GUIUtility.systemCopyBuffer = text;
            Debug.Log("[AppLinks] share text copied to clipboard: " + text);
        }
    }
}
