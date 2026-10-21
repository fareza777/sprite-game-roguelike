using System.Collections.Generic;
using Oathdeep.Core;
using Oathdeep.Loot;
using Oathdeep.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Oathdeep.EditorTools
{
    /// <summary>Stage 5: render/project settings and the single game scene.</summary>
    public static partial class OathdeepSetup
    {
        const string ScenePath = Root + "/Scenes/Main.unity";
        const string Renderer2DPath = "Assets/Settings/Renderer2D.asset";
        // 216x384 reference at 16 PPU: ~13.5 tiles across in portrait, integer scale 5 on 1080x1920.
        const int RefWidth = 216, RefHeight = 384;

        static void BuildScene(List<string> log)
        {
            ConfigureProject();

            EnsureFolder(Root + "/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = RefHeight / (2f * WorldPpu);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(0x09, 0x0a, 0x14, 0xff);
            camGo.AddComponent<UniversalAdditionalCameraData>();
            camGo.AddComponent<AudioListener>();
            var ppc = camGo.AddComponent<PixelPerfectCamera>();
            ppc.assetsPPU = WorldPpu;
            ppc.refResolutionX = RefWidth;
            ppc.refResolutionY = RefHeight;
            ppc.gridSnapping = PixelPerfectCamera.GridSnapping.PixelSnapping;
            ppc.cropFrame = PixelPerfectCamera.CropFrame.None;
            camGo.AddComponent<CameraRig>();

            var lightGo = new GameObject("Global Light 2D");
            var global = lightGo.AddComponent<Light2D>();
            global.lightType = Light2D.LightType.Global;
            global.intensity = 0.35f;

            var runner = new GameObject("GameRunner").AddComponent<GameRunner>();
            Set(runner,
                ("hero", knightHero), ("floor", floorOne), ("props", cryptProps),
                ("playerPrefab", Prefab("Player").GetComponent<PlayerController>()),
                ("pickupPrefab", Prefab("Pickup").GetComponent<Pickup>()),
                ("goldSprite", Spr(PropsPng, "coin0")), ("potionSprite", Spr(PropsPng, "potion")),
                ("skin", uiSkin), ("audioLibrary", audioLibrary), ("worldCamera", cam), ("globalLight", global));

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            log.Add("scene: " + ScenePath);
        }

        static void ConfigureProject()
        {
            Physics2D.gravity = Vector2.zero;
            PlayerSettings.runInBackground = true;   // keeps editor play mode ticking when remote-driven
            PlayerSettings.companyName = "Oathdeep";
            PlayerSettings.productName = "OATHDEEP";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.oathdeep.game");

            // Characters and props sort by their feet: lower on screen draws in front.
            var renderer = AssetDatabase.LoadAssetAtPath<Renderer2DData>(Renderer2DPath);
            if (renderer == null) throw new System.IO.FileNotFoundException("2D renderer data missing", Renderer2DPath);
            var so = new SerializedObject(renderer);
            Prop(so, "m_TransparencySortMode").enumValueIndex = (int)TransparencySortMode.CustomAxis;
            Prop(so, "m_TransparencySortAxis").vector3Value = new Vector3(0f, 1f, 0f);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(renderer);
        }
    }
}
