using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Oathdeep.EditorTools
{
    /// <summary>Android player settings and APK build. The build is queued so a remote caller isn't blocked for minutes.</summary>
    public static partial class OathdeepSetup
    {
        const string ApkPath = "Builds/Android/Undersun.apk";
        const string BuildReportPath = "Builds/Android/build-report.txt";
        const string IconPath = ArtDir + "/UI/app_icon.png";

        static void ConfigureAndroid()
        {
            var android = NamedBuildTarget.Android;
            PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetApplicationIdentifier(android, Core.AppLinks.PackageId);
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 3;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.Android.useCustomKeystore = false;   // debug-signed: fine for sideloading the slice
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release;

            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (icon != null)
            {
                var ti = (TextureImporter)AssetImporter.GetAtPath(IconPath);
                ti.textureType = TextureImporterType.Default;
                ti.filterMode = FilterMode.Point;
                ti.mipmapEnabled = false;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.SaveAndReimport();
                PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Application);
            }
        }

        [MenuItem("Oathdeep/Build Android APK")]
        public static string QueueAndroidBuild()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(BuildReportPath));
            File.WriteAllText(BuildReportPath, "queued\n");
            EditorApplication.update -= BuildWhenIdle;
            EditorApplication.update += BuildWhenIdle;
            return "Android build queued; watch " + BuildReportPath;
        }

        /// <summary>Runs on an editor tick once nothing is compiling (a build started mid-compile fails instantly).</summary>
        static void BuildWhenIdle()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            EditorApplication.update -= BuildWhenIdle;
            BuildAndroidNow();
        }

        static void BuildAndroidNow()
        {
            try
            {
                ConfigureAndroid();
                // An unsaved scene makes BuildPlayer show a modal save prompt, which blocks unattended builds.
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = ApkPath,
                    target = BuildTarget.Android,
                    options = BuildOptions.None,
                });
                var s = report.summary;
                var text = $"result: {s.result}\nsize: {s.totalSize / (1024f * 1024f):0.0} MB\ntime: {s.totalTime}\nerrors: {s.totalErrors}\nwarnings: {s.totalWarnings}\noutput: {s.outputPath}\n";
                foreach (var step in report.steps)
                    foreach (var m in step.messages)
                        if (m.type == LogType.Error || m.type == LogType.Exception) text += $"[{step.name}] {m.content}\n";
                File.WriteAllText(BuildReportPath, text);
            }
            catch (System.Exception e)
            {
                File.WriteAllText(BuildReportPath, "result: Exception\n" + e);
            }
        }
    }
}
