using System.Collections.Generic;
using Oathdeep.Combat;
using Oathdeep.Enemies;
using UnityEditor;
using UnityEngine;

namespace Oathdeep.EditorTools
{
    /// <summary>
    /// Boss fights: the arena art the mechanics lay down (boss_fx and the sandstorm veil) and, on every Chorister
    /// prefab, the shared <see cref="BossCore"/> plus that boss's signature mechanics.
    /// </summary>
    public static partial class OathdeepSetup
    {
        const string BossFxPng = ArtDir + "/VFX/boss_fx.png";
        const string SandFogPng = ArtDir + "/VFX/sand_fog.png";
        const string BossFxAsset = DataDir + "/BossFx.asset";

        static readonly Vector2 BeamPivot = new Vector2(0f, 0.5f);

        /// <summary>Slices the boss art and fills the BossFx library the mechanics read from.</summary>
        static void ConfigureBossArt()
        {
            Slice(BossFxPng, new List<Cell>
            {
                new Cell("bone_totem", 0, 0, 32, 32, Feet), new Cell("root_core", 32, 0, 32, 32, Feet),
                new Cell("spore_pod", 64, 0, 32, 32, Feet), new Cell("dusk_anchor", 96, 0, 32, 32, Feet),
                new Cell("steam", 128, 0, 16, 16, Centre), new Cell("water", 144, 0, 16, 16, Centre),
                new Cell("ice", 160, 0, 16, 16, Centre), new Cell("lava", 176, 0, 16, 16, Centre),
                new Cell("clock_hand", 192, 0, 16, 16, BeamPivot), new Cell("nest", 208, 0, 32, 32, Feet),
                new Cell("spore_cloud", 0, 32, 72, 72, Centre), new Cell("quicksand", 72, 32, 80, 80, Centre),
                new Cell("time_bubble", 152, 32, 80, 80, Centre), new Cell("root_snare", 0, 112, 40, 40, Centre),
            }, WorldPpu, SpriteMeshType.FullRect);
            Slice(SandFogPng, new List<Cell> { new Cell("sand_fog", 0, 0, 640, 640, Centre) }, WorldPpu, SpriteMeshType.FullRect);

            var fx = LoadOrCreate<BossFx>(BossFxAsset);
            fx.boneTotem = Spr(BossFxPng, "bone_totem");
            fx.rootCore = Spr(BossFxPng, "root_core");
            fx.sporePod = Spr(BossFxPng, "spore_pod");
            fx.duskAnchor = Spr(BossFxPng, "dusk_anchor");
            fx.nest = Spr(BossFxPng, "nest");
            fx.sporeCloud = Spr(BossFxPng, "spore_cloud");
            fx.water = Spr(BossFxPng, "water");
            fx.ice = Spr(BossFxPng, "ice");
            fx.quicksand = Spr(BossFxPng, "quicksand");
            fx.lava = Spr(BossFxPng, "lava");
            fx.rootSnare = Spr(BossFxPng, "root_snare");
            fx.timeBubble = Spr(BossFxPng, "time_bubble");
            fx.steam = Spr(BossFxPng, "steam");
            fx.clockHand = Spr(BossFxPng, "clock_hand");
            fx.sandFog = Spr(SandFogPng, "sand_fog");
            EditorUtility.SetDirty(fx);
        }

        /// <summary>Phases, break gauge and the signature mechanics of one Chorister.</summary>
        static void AddBossRules(GameObject boss, string id, Projectile shard)
        {
            var core = boss.AddComponent<BossCore>();
            core.EditorInit(AssetDatabase.LoadAssetAtPath<BossFx>(BossFxAsset), shard);
            switch (id)
            {
                case "bone_warden":
                    Totems(boss, TotemShield.Style.Bone, 2, 3);
                    break;
                case "mother_morel":
                    Mechanic<SporeGarden>(boss, 1, 3);
                    break;
                case "tidecaller":
                    Mechanic<Tides>(boss, 1, 3);
                    break;
                case "furnace_saint":
                    Mechanic<Overheat>(boss, 1, 3);
                    break;
                case "pale_matriarch":
                    Mechanic<FrostFloor>(boss, 2, 3);
                    Mechanic<MirrorDecoys>(boss, 3, 3);
                    break;
                case "verdant_hierophant":
                    Mechanic<RootSnares>(boss, 1, 3);
                    Totems(boss, TotemShield.Style.Root, 2, 3);
                    break;
                case "horologist":
                    Mechanic<ClockHands>(boss, 1, 3);
                    break;
                case "dune_pharaoh":
                    Mechanic<Sandstorm>(boss, 1, 3).EditorInit(2, true);
                    break;
                case "mirrored_choir":
                    Mechanic<MirrorDecoys>(boss, 1, 3);
                    break;
                case "the_cantor":
                    // Every verse at once: anchors first, then the clock and the tide, then sand and reflections.
                    Totems(boss, TotemShield.Style.Dusk, 1, 1);
                    Mechanic<ClockHands>(boss, 2, 3);
                    Mechanic<Tides>(boss, 2, 2);
                    Mechanic<Sandstorm>(boss, 3, 3).EditorInit(3, false);
                    Mechanic<MirrorDecoys>(boss, 3, 3);
                    break;
            }
        }

        static T Mechanic<T>(GameObject boss, int from, int until) where T : BossMechanic
        {
            var m = boss.AddComponent<T>();
            m.EditorPhases(from, until);
            return m;
        }

        static void Totems(GameObject boss, TotemShield.Style style, int from, int until) =>
            Mechanic<TotemShield>(boss, from, until).EditorInit(style, 3, 0.06f);
    }
}
