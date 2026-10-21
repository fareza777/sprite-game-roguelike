using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Oathdeep.EditorTools
{
    /// <summary>
    /// One-click project assembly: import settings for the art, ScriptableObject content, prefabs and the main scene.
    /// Idempotent — rerun after art changes. Split across partial files by stage.
    /// </summary>
    public static partial class OathdeepSetup
    {
        const string Root = "Assets/_Game";
        const string DataDir = Root + "/Data";
        const string PrefabDir = Root + "/Prefabs";
        const int WorldPpu = 16;

        [MenuItem("Oathdeep/Build Everything")]
        public static string BuildEverything()
        {
            var log = new List<string>();
            ConfigureArt(log);
            BuildContent(log);
            BuildAudio(log);
            BuildPrefabs(log);
            FinishContent(log);
            BuildScene(log);
            AssetDatabase.SaveAssets();
            string report = string.Join("\n", log);
            Debug.Log("[Oathdeep] Build Everything\n" + report);
            return report;
        }

        const string SetupReportPath = "Logs/setup-report.txt";

        /// <summary>Runs Build Everything on an idle editor tick (long imports outlive remote-call timeouts).</summary>
        public static string QueueBuildEverything()
        {
            Directory.CreateDirectory("Logs");
            File.WriteAllText(SetupReportPath, "queued" + System.Environment.NewLine);
            EditorApplication.update -= BuildWhenEditorIdle;
            EditorApplication.update += BuildWhenEditorIdle;
            return "Build Everything queued; watch " + SetupReportPath;
        }

        static void BuildWhenEditorIdle()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= BuildWhenEditorIdle;
            try { File.WriteAllText(SetupReportPath, "done" + System.Environment.NewLine + BuildEverything()); }
            catch (System.Exception e) { File.WriteAllText(SetupReportPath, "failed" + System.Environment.NewLine + e); }
        }

        // ---------------- asset helpers ----------------

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static readonly Dictionary<string, Dictionary<string, Sprite>> SpriteCache = new Dictionary<string, Dictionary<string, Sprite>>();

        /// <summary>Named sprite from a sliced sheet; throws with a clear message if the slice is missing.</summary>
        static Sprite Spr(string sheet, string name)
        {
            if (!SpriteCache.TryGetValue(sheet, out var map))
            {
                map = AssetDatabase.LoadAllAssetsAtPath(sheet).OfType<Sprite>().ToDictionary(s => s.name, s => s);
                SpriteCache[sheet] = map;
            }
            if (!map.TryGetValue(name, out var sprite))
                throw new System.InvalidOperationException($"Sprite '{name}' not found in {sheet}");
            return sprite;
        }

        static Sprite[] Seq(string sheet, string prefix, int count) =>
            Enumerable.Range(0, count).Select(i => Spr(sheet, prefix + i)).ToArray();

        // ---------------- serialized field helpers ----------------

        static SerializedProperty Prop(SerializedObject so, string field)
        {
            var p = so.FindProperty(field);
            if (p == null) throw new System.InvalidOperationException($"{so.targetObject.GetType().Name} has no serialized field '{field}'");
            return p;
        }

        /// <summary>Assigns private [SerializeField]s by name. Supports refs, numbers, bools, strings, colours, vectors and ref arrays.</summary>
        static void Set(Object target, params (string field, object value)[] values)
        {
            var so = new SerializedObject(target);
            foreach (var (field, value) in values)
            {
                var p = Prop(so, field);
                switch (value)
                {
                    case null: p.objectReferenceValue = null; break;
                    case Object o: p.objectReferenceValue = o; break;
                    case int i when p.propertyType == SerializedPropertyType.Float: p.floatValue = i; break;
                    case int i: p.intValue = i; break;
                    case float f: p.floatValue = f; break;
                    case bool b: p.boolValue = b; break;
                    case string s: p.stringValue = s; break;
                    case Color c: p.colorValue = c; break;
                    case Vector2 v: p.vector2Value = v; break;
                    case Vector2Int v: p.vector2IntValue = v; break;
                    case System.Enum e: p.enumValueIndex = System.Convert.ToInt32(e); break;
                    case Object[] arr:
                        p.arraySize = arr.Length;
                        for (int k = 0; k < arr.Length; k++) p.GetArrayElementAtIndex(k).objectReferenceValue = arr[k];
                        break;
                    default: throw new System.ArgumentException($"Unsupported value for {field}: {value.GetType()}");
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
