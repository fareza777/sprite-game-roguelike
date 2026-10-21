using System.Collections.Generic;
using System.Linq;
using Oathdeep.Combat;
using Oathdeep.Core;
using Oathdeep.Data;
using Oathdeep.Dungeon;
using Oathdeep.Enemies;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Oathdeep.EditorTools
{
    /// <summary>
    /// The forty creatures: enemy data from <see cref="Bestiary"/>, sprite-sheet models with generated animator
    /// controllers, archetype AIs, biome projectiles and ground blasts, and each floor's enemy pool.
    /// </summary>
    public static partial class OathdeepSetup
    {
        const string EnemyFxPng = ArtDir + "/VFX/enemy_fx.png";
        static string SheetPng(string id) => $"{ArtDir}/Enemies/Sheets/{id}.png";

        /// <summary>Tag order and frame counts of the sheets drawn by ArtSource/gen/bestiary.py.</summary>
        static readonly (string tag, int frames, float ms, bool loop)[] RegularTags =
            { ("idle", 2, 320, true), ("run", 4, 110, true), ("windup", 2, 150, false), ("attack", 3, 90, false), ("hurt", 1, 140, false), ("death", 4, 120, false) };
        static readonly (string tag, int frames, float ms, bool loop)[] BossTags =
            { ("idle", 2, 340, true), ("run", 4, 120, true), ("windup", 2, 160, false), ("attack", 3, 100, false), ("cast", 3, 130, true), ("hurt", 1, 140, false), ("death", 5, 140, false) };

        static readonly HashSet<string> AsepriteCreatures = new HashSet<string> { "skeleton", "archer", "slime", "bone_warden" };

        static bool HasSheet(string id) => !AsepriteCreatures.Contains(id);
        static int SheetSize(BestiaryEntry e) => e.IsBoss ? 64 : 32;

        // ---------------- art ----------------

        static void ConfigureBestiaryArt()
        {
            foreach (var e in Bestiary.All.Where(b => HasSheet(b.Id)))
            {
                int size = SheetSize(e);
                int count = (e.IsBoss ? BossTags : RegularTags).Sum(t => t.frames);
                var pivot = new Vector2(0.5f, 1.5f / size);
                Slice(SheetPng(e.Id), Enumerable.Range(0, count).Select(i => new Cell($"{e.Id}_{i}", i * size, 0, size, size, pivot)).ToList(),
                      WorldPpu, SpriteMeshType.FullRect);
            }
            string[] fx = { "fireball", "iceShard", "thorn", "glassShard", "waterOrb", "sandBlade", "duskOrb", "spore", "curseOrb", "gearShot" };
            var cells = fx.Select((n, i) => G(n, i, 0, Centre)).ToList();
            for (int i = 0; i < 2; i++) cells.Add(new Cell("warn" + i, 32 * i, 16, 32, 32, Centre));
            for (int i = 0; i < 4; i++) cells.Add(new Cell("boom" + i, 64 + 32 * i, 16, 32, 32, Centre));
            Slice(EnemyFxPng, cells, WorldPpu, SpriteMeshType.FullRect);
        }

        static Sprite Portrait(string id) => id switch
        {
            "skeleton" => FirstAseFrame(SkeletonAse),
            "archer" => FirstAseFrame(ArcherAse),
            "slime" => FirstAseFrame(SlimeAse),
            "bone_warden" => FirstAseFrame(WardenAse),
            _ => Spr(SheetPng(id), id + "_0"),
        };

        static Sprite FirstAseFrame(string ase) => AssetDatabase.LoadAllAssetsAtPath(ase).OfType<Sprite>()
            .Where(s => s.name.StartsWith("Frame_")).OrderBy(s => int.Parse(s.name.Substring(6))).First();

        // ---------------- data ----------------

        static void BuildEnemyData()
        {
            foreach (var e in Bestiary.All)
            {
                var a = e.Archetype;
                bool ranged = a == Archetype.Shooter || a == Archetype.Turret || a == Archetype.Caster;
                float aggro = e.IsBoss ? 22f : ranged ? 11f : 9f;
                float range = a switch
                {
                    Archetype.Tank => 1.5f, Archetype.Shooter => 6f, Archetype.Turret => 8f, Archetype.Caster => 6f,
                    Archetype.Hopper => 1.4f, Archetype.Boss => 2.4f, _ => 1.3f,
                };
                var gold = e.IsBoss ? new Vector2Int(40, 60) : a == Archetype.Tank ? new Vector2Int(2, 6) : new Vector2Int(1, 4);
                float item = e.IsBoss ? 1f : a == Archetype.Tank ? 0.1f : 0.06f;
                Enemy(e.Id, e.Name, Portrait(e.Id), e.Stats, aggro, range, gold, item, e.IsBoss ? 1f : 0.06f, e.IsBoss);
            }
            Enemy("slimeling", "Slimeling", Portrait("slime"), new StatBlock(10, 4, 0, 0f, 1.5f, 2.6f), 7f, 1.2f, new Vector2Int(0, 1), 0f, 0.02f, false);
        }

        static void Enemy(string id, string name, Sprite portrait, StatBlock stats, float aggro, float range, Vector2Int gold, float itemChance,
                          float potionChance, bool boss)
        {
            var data = LoadOrCreate<EnemyData>($"{DataDir}/Enemies/Enemy_{id}.asset");
            data.EditorInit(id, name, portrait, stats, aggro, range, gold, itemChance, potionChance, boss);
            EditorUtility.SetDirty(data);
            Enemies[id] = data;
        }

        // ---------------- models ----------------

        /// <summary>Animator controller with one state per tag, keyed on the sheet's frames; idle is the default.</summary>
        static RuntimeAnimatorController SheetController(BestiaryEntry e)
        {
            string dir = $"{ArtDir}/Enemies/Anim/{e.Id}";
            EnsureFolder(dir);
            string path = $"{dir}/{e.Id}.controller";
            AssetDatabase.DeleteAsset(path);
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
            var machine = ctrl.layers[0].stateMachine;
            int index = 0;
            foreach (var (tag, frames, ms, loop) in e.IsBoss ? BossTags : RegularTags)
            {
                var clip = new AnimationClip { name = tag, frameRate = 60f };
                var keys = new ObjectReferenceKeyframe[frames + 1];
                float t = 0f;
                for (int i = 0; i < frames; i++)
                {
                    keys[i] = new ObjectReferenceKeyframe { time = t, value = Spr(SheetPng(e.Id), $"{e.Id}_{index + i}") };
                    t += ms / 1000f;
                }
                // Hold the last frame (the corpse stays down for the death fade).
                keys[frames] = new ObjectReferenceKeyframe { time = tag == "death" ? t + 0.6f : t, value = keys[frames - 1].value };
                AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = loop;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                AssetDatabase.AddObjectToAsset(clip, ctrl);
                var state = machine.AddState(tag);
                state.motion = clip;
                if (tag == "idle") machine.defaultState = state;
                index += frames;
            }
            return ctrl;
        }

        static (Animator, SpriteRenderer, HitFlash) SheetModel(GameObject root, BestiaryEntry e)
        {
            var model = new GameObject("Model");
            model.transform.SetParent(root.transform, false);
            var sr = model.AddComponent<SpriteRenderer>();
            sr.sprite = Spr(SheetPng(e.Id), e.Id + "_0");
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            var animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = SheetController(e);
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            var flash = model.AddComponent<HitFlash>();
            Set(flash, ("flashMaterial", flashMaterial));
            return (animator, sr, flash);
        }

        // ---------------- vfx + projectiles ----------------

        static void BuildBestiaryVfx()
        {
            Vfx("Boom", Seq(EnemyFxPng, "boom", 4), 16f, OrderFx);
            var blast = new GameObject("GroundBlast");
            var warn = AddSprite(blast, "Warning", Spr(EnemyFxPng, "warn0"), OrderFloorFx + 2, Vector3.zero, true);
            Flipbook(warn, Seq(EnemyFxPng, "warn", 2), 8f, true);
            Set(blast.AddComponent<GroundBlast>(), ("warning", warn), ("burstVfx", Prefab("VFX/Boom")));
            Save(blast, "VFX/GroundBlast");

            foreach (var name in new[] { "fireball", "iceShard", "thorn", "glassShard", "waterOrb", "sandBlade", "duskOrb", "spore", "curseOrb", "gearShot" })
                Projectile("E_" + name, Spr(EnemyFxPng, name), true);
        }

        static Projectile Shot(string name) => Prefab("Projectiles/E_" + name).GetComponent<Projectile>();

        static string ShotFor(BestiaryEntry e) => e.Id switch
        {
            "mummified_priest" => "curseOrb",
            "prism_eye" => "glassShard",
            _ => e.Floor switch
            {
                2 => "spore", 3 => "waterOrb", 4 => "fireball", 5 => "iceShard", 6 => "thorn",
                7 => "gearShot", 8 => "sandBlade", 9 => "glassShard", _ => "duskOrb",
            },
        };

        static readonly Dictionary<string, string> BossMinions = new Dictionary<string, string>
        {
            ["mother_morel"] = "sporeling", ["tidecaller"] = "brine_crab", ["furnace_saint"] = "cinder_imp",
            ["pale_matriarch"] = "frost_wisp", ["verdant_hierophant"] = "briar_crawler", ["horologist"] = "tick_bomb",
            ["dune_pharaoh"] = "scarab_swarm", ["mirrored_choir"] = "shard_sprite", ["the_cantor"] = "hollow_acolyte",
        };

        static string PrefabName(string id) => string.Concat(id.Split('_').Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));

        static readonly Dictionary<string, string> LegacyPrefabs = new Dictionary<string, string>
        {
            ["skeleton"] = "Skeleton", ["archer"] = "Archer", ["slime"] = "Slime", ["bone_warden"] = "BoneWarden",
        };

        static EnemyBase EnemyPrefab(string id) =>
            Prefab("Enemies/" + (LegacyPrefabs.TryGetValue(id, out var legacy) ? legacy : PrefabName(id))).GetComponent<EnemyBase>();

        // ---------------- enemy prefabs ----------------

        static void BuildBestiary()
        {
            var blast = Prefab("VFX/GroundBlast").GetComponent<GroundBlast>();
            var boom = Prefab("VFX/Boom");
            foreach (var e in Bestiary.All.Where(b => HasSheet(b.Id) && !b.IsBoss)) BuildCreature(e, blast, boom);
            foreach (var e in Bestiary.All.Where(b => HasSheet(b.Id) && b.IsBoss)) BuildCreature(e, blast, boom);
        }

        static void BuildCreature(BestiaryEntry e, GroundBlast blast, GameObject boom)
        {
            bool big = e.Archetype == Archetype.Tank;
            var spec = new EnemySpec
            {
                Name = PrefabName(e.Id), Data = e.Id,
                BodyRadius = e.IsBoss ? 0.7f : big ? 0.38f : 0.28f,
                ShadowWidth = e.IsBoss ? 2.8f : big ? 1.2f : 0.9f,
                ModelScale = 1f,
                KnockbackResistance = e.IsBoss ? 0.85f : big ? 0.5f : 0f,
                Hurtbox = e.IsBoss ? new Vector2(1.8f, 2.8f) : big ? new Vector2(1f, 1.4f) : new Vector2(0.7f, 1.1f),
                Entry = e,
            };
            var shot = Shot(ShotFor(e));
            switch (e.Archetype)
            {
                case Archetype.Melee:
                    Enemy<SkeletonAI>(spec, ai => Set(ai, ("windup", e.Id == "briar_crawler" ? 0.35f : 0.5f),
                        ("lungeSpeed", e.Id == "hunger_maw" ? 10f : 8f)));
                    break;
                case Archetype.Tank:
                    Enemy<SkeletonAI>(spec, ai => Set(ai, ("windup", 0.75f), ("lungeSpeed", 5f), ("recover", 0.9f),
                        ("slash", new AttackSpec(1.4f, 9f, 0.2f, 0.07f, 0.25f))));
                    break;
                case Archetype.Shooter:
                    Enemy<ArcherAI>(spec, ai => Set(ai, ("arrowPrefab", shot), ("shotSfx", Oathdeep.Audio.Sfx.EnemyShot), ("arrowSpeed", e.Id == "shard_sprite" ? 11f : 8f),
                        ("shots", e.Id == "thorn_dryad" || e.Id == "mummified_priest" ? 3 : 1),
                        ("spread", e.Id == "thorn_dryad" ? 24f : e.Id == "mummified_priest" ? 36f : 0f)));
                    break;
                case Archetype.Turret:
                    Enemy<ArcherAI>(spec, ai => Set(ai, ("arrowPrefab", shot), ("shotSfx", Oathdeep.Audio.Sfx.EnemyShot), ("stationary", true), ("preferredMin", 0f), ("preferredMax", 99f),
                        ("drawTime", 0.9f), ("arrowSpeed", 8.5f), ("shots", e.Id == "prism_eye" ? 5 : 3), ("spread", e.Id == "prism_eye" ? 50f : 20f)));
                    break;
                case Archetype.Charger:
                    Enemy<ChargerAI>(spec, ai => Set(ai, ("dashSpeed", e.Id == "brine_crab" ? 9f : 11.5f)));
                    break;
                case Archetype.Hopper:
                    Enemy<SlimeAI>(spec, ai => Set(ai, ("splitPrefab", null), ("splitCount", 0), ("hopSpeed", e.Id == "mire_toad" ? 7.5f : 6.5f),
                        ("restTime", e.Id == "scarab_swarm" ? 0.35f : 0.7f)));
                    break;
                case Archetype.Caster:
                    Enemy<CasterAI>(spec, ai => Set(ai, ("blastPrefab", blast), ("blasts", e.Id == "root_horror" ? 3 : e.Id == "hollow_acolyte" ? 2 : 1),
                        ("blastRadius", e.Id == "root_horror" ? 1.1f : 1.3f)));
                    break;
                case Archetype.Bomber:
                    Enemy<BomberAI>(spec, ai => Set(ai, ("burstVfx", boom)));
                    break;
                case Archetype.Boss:
                    Enemy<PatternBossAI>(spec, ai => Set(ai, ("patternId", e.Id), ("shot", shot), ("blast", blast),
                        ("minion", EnemyPrefab(BossMinions[e.Id]))));
                    break;
            }
        }

        /// <summary>The three regulars of a floor, weighted so melee bodies fill the room and specialists season it.</summary>
        static WeightedEnemy[] PoolFor(int floor) => Bestiary.OnFloor(floor).Where(e => !e.IsBoss).Select(e => new WeightedEnemy
        {
            Prefab = EnemyPrefab(e.Id),
            Weight = e.Archetype switch
            {
                Archetype.Melee => 5f, Archetype.Tank => 2f, Archetype.Turret => 2f, Archetype.Caster => 2f, Archetype.Bomber => 2.5f, _ => 3f,
            },
        }).ToArray();
    }
}
