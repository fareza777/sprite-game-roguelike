using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Oathdeep.EditorTools
{
    /// <summary>AdMob and the Android libraries it needs. The SDK's editor types are internal, so they are reached by name.</summary>
    public static partial class OathdeepSetup
    {
        // Google's sample AdMob app; replace with the real app ID from the AdMob console before release.
        const string AdMobTestAppId = "ca-app-pub-3940256099942544~3347511713";

        static void ConfigureAds()
        {
            var settingsType = Type.GetType("GoogleMobileAds.Editor.GoogleMobileAdsSettings, GoogleMobileAds.Editor");
            var load = settingsType?.GetMethod("LoadInstance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (load?.Invoke(null, null) is ScriptableObject settings)
            {
                var so = new SerializedObject(settings);
                so.FindProperty("adMobAndroidAppId").stringValue = AdMobTestAppId;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(settings);
            }
            else Debug.LogWarning("[Oathdeep] Google Mobile Ads settings not found; is com.google.ads.mobile installed?");

            // The dependency resolver writes the ads and billing libraries into these Gradle templates.
            var player = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0];
            var ps = new SerializedObject(player);
            ps.FindProperty("useCustomMainGradleTemplate").boolValue = true;
            ps.FindProperty("useCustomGradlePropertiesTemplate").boolValue = true;
            ps.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        /// <summary>Runs the External Dependency Manager so the APK carries the Play services AARs.</summary>
        public static string ResolveAndroidDependencies()
        {
            ConfigureAds();
            var resolver = Type.GetType("GooglePlayServices.PlayServicesResolver, Google.JarResolver");
            var resolve = resolver?.GetMethod("ResolveSync", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(bool) }, null);
            if (resolve == null) return "EDM4U resolver not found";
            object ok = resolve.Invoke(null, new object[] { true });
            return "Android dependencies resolved: " + ok;
        }
    }
}
