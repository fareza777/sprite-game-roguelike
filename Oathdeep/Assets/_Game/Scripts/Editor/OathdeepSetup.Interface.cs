using System;
using System.Linq;
using Oathdeep.Core;
using Oathdeep.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oathdeep.EditorTools
{
    public static partial class OathdeepSetup
    {
        const string InterfacePng = ArtDir + "/UI/interface_icons.png";
        const string JoystickBasePng = ArtDir + "/UI/joystick_base.png";
        const string JoystickKnobPng = ArtDir + "/UI/joystick_knob.png";
        const string ActionRingPng = ArtDir + "/UI/action_ring.png";
        const int InterfaceCellSize = 64;
        static readonly string[] InterfaceIconNames =
        {
            "iconGold", "iconPotion", "iconAttack", "iconDodge", "iconSkill", "iconPause",
            "gear", "info", "share", "star", "codex", "quest",
            "heroes", "back", "close", "bag", "iconFlare", "iconRequiem",
            "iconToll", "iconShadowstep", "joystickBase", "joystickKnob", "actionRing", "iconHealth",
        };

        static void ConfigureInterfaceArt()
        {
            Cell IconCell(string name, int index, int columns) => new Cell(name,
                index % columns * InterfaceCellSize, index / columns * InterfaceCellSize,
                InterfaceCellSize, InterfaceCellSize, Centre);
            // 64 pixels per world unit keeps pickups the same size as the previous 16px art at 16 PPU.
            Slice(ItemIconsPng, ItemIconNames.Select((name, i) => IconCell(name, i, 8)).ToList(), InterfaceCellSize, SpriteMeshType.FullRect);
            Slice(InterfacePng, InterfaceIconNames.Select((name, i) => IconCell(name, i, 6)).ToList(), InterfaceCellSize, SpriteMeshType.FullRect);
            foreach (var path in new[] { JoystickBasePng, JoystickKnobPng, ActionRingPng }) SingleSprite(path, InterfaceCellSize);
            SpriteCache.Clear();
        }

        /// <summary>Refresh the generated sprites and their references without regenerating game content.</summary>
        [MenuItem("Oathdeep/Refresh Item and HUD Art")]
        public static void RefreshInterfaceArt()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Leave Play mode before refreshing art.");
            AssetDatabase.Refresh();
            ConfigureInterfaceArt();
            BuildItemIcons();
            BuildSkin(BuildFont());
            string[] heroAssets = { "Hero_Knight", "Hero_Wren", "Hero_Morrow", "Hero_Brakka", "Hero_Sable" };
            string[] skills = { "iconSkill", "iconFlare", "iconRequiem", "iconToll", "iconShadowstep" };
            for (int i = 0; i < heroAssets.Length; i++)
            {
                var hero = AssetDatabase.LoadAssetAtPath<HeroData>($"{DataDir}/{heroAssets[i]}.asset");
                if (hero == null) throw new InvalidOperationException("Hero asset missing: " + heroAssets[i]);
                Set(hero, ("skillIcon", Spr(InterfacePng, skills[i])));
                EditorUtility.SetDirty(hero);
            }

            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            foreach (var runner in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<GameRunner>(true)))
                Set(runner, ("goldSprite", Spr(InterfacePng, "iconGold")), ("potionSprite", Spr(InterfacePng, "iconPotion")));
            EditorSceneManager.SaveScene(scene);
            if (opened) EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.SaveAssets();
            Debug.Log("[Oathdeep] Refreshed 36 item sprites, 24 interface sprites, five hero skills and pickup art.");
        }
    }
}
