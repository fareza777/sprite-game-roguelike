using System.Collections.Generic;
using Oathdeep.Combat;
using Oathdeep.Core;
using Oathdeep.Dungeon;
using Oathdeep.Enemies;
using Oathdeep.Loot;
using Oathdeep.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Oathdeep.EditorTools
{
    /// <summary>Stage 3: every runtime prefab, built in memory and saved. Leaves first so references resolve.</summary>
    public static partial class OathdeepSetup
    {
        const string UnlitMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
        const string FlashMaterialPath = ArtDir + "/Materials/SpriteFlash.mat";
        const string SlipperyPath = ArtDir + "/Materials/Frictionless.physicsMaterial2D";
        const int OrderFloorFx = -70, OrderShadow = -50, OrderTelegraph = -40, OrderWall = 1, OrderFx = 5;

        static Material unlit, flashMaterial;
        static PhysicsMaterial2D slippery;

        static void BuildPrefabs(List<string> log)
        {
            unlit = AssetDatabase.LoadAssetAtPath<Material>(UnlitMaterialPath);
            flashMaterial = BuildFlashMaterial();
            slippery = BuildFrictionless();
            int before = count;
            BuildVfx();
            BuildProjectilesAndPickup();
            BuildPlayers();
            BuildEnemies();
            BuildBestiaryVfx();
            BuildBestiary();
            BuildProps();
            BuildEncounters();
            log.Add($"prefabs: {count - before} saved");
        }

        static int count;

        static GameObject Prefab(string name)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{name}.prefab");
            if (go == null) throw new System.InvalidOperationException($"Prefab {name} has not been built");
            return go;
        }

        static GameObject Save(GameObject go, string name)
        {
            string path = $"{PrefabDir}/{name}.prefab";
            EnsureFolder(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'));
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            count++;
            return prefab;
        }

        static Material BuildFlashMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(FlashMaterialPath);
            if (mat != null) return mat;
            EnsureFolder(ArtDir + "/Materials");
            mat = new Material(Shader.Find("Oathdeep/SpriteFlash"));
            AssetDatabase.CreateAsset(mat, FlashMaterialPath);
            return mat;
        }

        /// <summary>Zero friction so characters slide along walls instead of sticking when moving diagonally.</summary>
        static PhysicsMaterial2D BuildFrictionless()
        {
            var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(SlipperyPath);
            if (mat != null) return mat;
            EnsureFolder(ArtDir + "/Materials");
            mat = new PhysicsMaterial2D("Frictionless") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(mat, SlipperyPath);
            return mat;
        }

        // ---------------- building blocks ----------------

        static SpriteRenderer AddSprite(GameObject parent, string name, Sprite sprite, int order, Vector3 local, bool unlitFx = false)
        {
            var go = parent.name == name ? parent : new GameObject(name);
            if (go != parent) go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = local;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            sr.spriteSortPoint = SpriteSortPoint.Pivot;   // y-sort by feet, not by sprite centre
            if (unlitFx && unlit != null) sr.sharedMaterial = unlit;
            return sr;
        }

        static SpriteVfx Flipbook(SpriteRenderer sr, Sprite[] frames, float fps, bool loop, float lifetime = 0f, bool randomStart = false)
        {
            var vfx = sr.gameObject.AddComponent<SpriteVfx>();
            vfx.EditorInit(frames, fps, loop, lifetime, randomStart);
            return vfx;
        }

        static Light2D PointLight(GameObject parent, Vector3 local, Color color, float radius, float intensity, bool flicker)
        {
            var go = new GameObject("Light");
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = local;
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.pointLightOuterRadius = radius;
            light.pointLightInnerRadius = radius * 0.15f;
            light.falloffIntensity = 0.65f;
            if (flicker) Set(go.AddComponent<FlickerLight>(), ("baseIntensity", intensity));
            return light;
        }

        static Rigidbody2D Body(GameObject go, RigidbodyType2D type)
        {
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = type;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            if (type == RigidbodyType2D.Dynamic) rb.sharedMaterial = slippery;
            return rb;
        }

        static void Shadow(GameObject parent, float width)
        {
            var sr = AddSprite(parent, "Shadow", Spr(VfxPng, "shadow"), OrderShadow, Vector3.zero);
            sr.color = new Color(1f, 1f, 1f, 0.55f);
            sr.transform.localScale = new Vector3(width, width * 0.9f, 1f);
        }

        /// <summary>Animated character model from an imported .aseprite, with hit flash and feet-pivot sorting.</summary>
        static (Animator animator, SpriteRenderer renderer, HitFlash flash) Model(GameObject root, string asePath, float scale = 1f)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(asePath);
            var model = (GameObject)PrefabUtility.InstantiatePrefab(source, root.transform);
            model.name = "Model";
            model.transform.localScale = Vector3.one * scale;
            var sr = model.GetComponent<SpriteRenderer>();
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            sr.sortingOrder = 0;
            var animator = model.GetComponent<Animator>();
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            var flash = model.AddComponent<HitFlash>();
            Set(flash, ("flashMaterial", flashMaterial));
            return (animator, sr, flash);
        }

        static AnimDriver Driver(GameObject root, Animator animator, SpriteRenderer renderer)
        {
            var driver = root.AddComponent<AnimDriver>();
            Set(driver, ("animator", animator), ("spriteRenderer", renderer));
            return driver;
        }

        // ---------------- VFX ----------------

        static void BuildVfx()
        {
            Vfx("Slash", Seq(VfxPng, "slash", 3), 22f, OrderFx);
            Vfx("Bash", Seq(VfxPng, "bash", 3), 18f, OrderFx);
            Vfx("Dust", Seq(PropsPng, "dust", 3), 14f, OrderShadow + 1);
            Vfx("Spark", Seq(PropsPng, "spark", 3), 24f, OrderFx + 1);
            Vfx("Shards", Seq(PropsPng, "shards", 3), 14f, OrderFx);
            Vfx("Poof", Seq(PropsPng, "poof", 4), 14f, OrderFx);
            Vfx("Toll", Seq(HeroFxPng, "ring", 4), 10f, OrderFloorFx + 1);
            Vfx("Shadow", Seq(HeroFxPng, "shadow", 2), 10f, OrderFx);
            var wisp = new GameObject("Wisp");
            Flipbook(AddSprite(wisp, "Wisp", Spr(HeroFxPng, "wisp0"), OrderFx, Vector3.zero, true), Seq(HeroFxPng, "wisp", 2), 8f, true);
            PointLight(wisp, Vector3.zero, new Color(0.55f, 0.85f, 0.9f), 1.6f, 0.6f, false);
            Save(wisp, "VFX/Wisp");
            var marker = new GameObject("SpawnMarker");
            Flipbook(AddSprite(marker, "SpawnMarker", Spr(PropsPng, "marker0"), OrderFloorFx, Vector3.zero, true),
                     Seq(PropsPng, "marker", 4), 10f, true, 0.75f);
            Save(marker, "VFX/SpawnMarker");
        }

        static void Vfx(string name, Sprite[] frames, float fps, int order)
        {
            var go = new GameObject(name);
            Flipbook(AddSprite(go, name, frames[0], order, Vector3.zero, true), frames, fps, false);
            Save(go, "VFX/" + name);
        }

        // ---------------- projectiles + pickup ----------------

        static void BuildProjectilesAndPickup()
        {
            Projectile("Arrow", Spr(PropsPng, "arrow"), false);
            Projectile("BoneShard", Spr(PropsPng, "boneShard"), true);
            Projectile("Bolt", Spr(HeroFxPng, "bolt"), false);
            Projectile("Flare", Spr(HeroFxPng, "flare"), true);
            Projectile("SoulOrb", Spr(HeroFxPng, "orb0"), true);

            var go = new GameObject("Pickup");
            var item = AddSprite(go, "Sprite", Spr(PropsPng, "coin0"), 2, new Vector3(0f, 0.08f, 0f));
            var glow = AddSprite(go, "Glow", Spr(VfxPng, "glow"), 1, Vector3.zero, true);
            glow.enabled = false;
            var trigger = go.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.35f;
            Body(go, RigidbodyType2D.Kinematic);
            Set(go.AddComponent<Pickup>(), ("sprite", item), ("glow", glow));
            Save(go, "Pickup");
        }

        static void Projectile(string name, Sprite sprite, bool magic)
        {
            var go = new GameObject(name);
            AddSprite(go, "Sprite", sprite, 3, Vector3.zero, magic);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.14f;
            var rb = Body(go, RigidbodyType2D.Dynamic);
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Set(go.AddComponent<Projectile>(), ("impactVfx", Prefab("VFX/Spark")));
            Save(go, "Projectiles/" + name);
        }

        // ---------------- player ----------------

        static void BuildPlayers()
        {
            var heroAse = new Dictionary<HeroId, string>
            {
                { HeroId.Aldric, KnightAse }, { HeroId.Wren, WrenAse }, { HeroId.Morrow, MorrowAse },
                { HeroId.Brakka, BrakkaAse }, { HeroId.Sable, SableAse },
            };
            foreach (var pair in heroAse)
            {
                var prefab = BuildPlayer(pair.Key, pair.Value);
                Set(Heroes[pair.Key], ("playerPrefab", prefab));
                EditorUtility.SetDirty(Heroes[pair.Key]);
            }
        }

        static GameObject BuildPlayer(HeroId id, string ase)
        {
            var go = new GameObject("Player_" + id);
            var rb = Body(go, RigidbodyType2D.Dynamic);
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            col.offset = new Vector2(0f, 0.25f);
            Shadow(go, 1.1f);
            var (animator, renderer, flash) = Model(go, ase);
            PointLight(go, new Vector3(0f, 0.8f, 0f), new Color(1f, 0.9f, 0.75f), 3.6f, 0.55f, false);

            Set(go.AddComponent<Health>(), ("team", Team.Player), ("invulnerableAfterHit", 0.45f));
            go.AddComponent<CombatStats>();
            var input = go.AddComponent<PlayerInputReader>();
            var driver = Driver(go, animator, renderer);
            go.AddComponent<PlayerInventory>();
            Set(go.AddComponent<PlayerController>(), ("heroId", id), ("input", input), ("anim", driver), ("flash", flash),
                ("slashVfx", Prefab("VFX/Slash")), ("dodgeDustVfx", Prefab("VFX/Dust")), ("bashVfx", Prefab("VFX/Bash")),
                ("boltPrefab", Prefab("Projectiles/Bolt").GetComponent<Projectile>()),
                ("flarePrefab", Prefab("Projectiles/Flare").GetComponent<Projectile>()),
                ("orbPrefab", Prefab("Projectiles/SoulOrb").GetComponent<Projectile>()),
                ("wispPrefab", Prefab("VFX/Wisp")), ("tollVfx", Prefab("VFX/Toll")), ("shadowVfx", Prefab("VFX/Shadow")));
            return Save(go, "Heroes/Player_" + id);
        }

        // ---------------- enemies ----------------

        struct EnemySpec
        {
            public string Name, Ase, Data;
            public float BodyRadius, ShadowWidth, ModelScale, KnockbackResistance;
            public Vector2 Hurtbox;
            public Oathdeep.Data.BestiaryEntry Entry;   // set for sprite-sheet creatures
        }

        static void BuildEnemies()
        {
            var skeleton = Enemy<SkeletonAI>(new EnemySpec { Name = "Skeleton", Ase = SkeletonAse, Data = "skeleton", BodyRadius = 0.28f, ShadowWidth = 0.9f, ModelScale = 1f, Hurtbox = new Vector2(0.7f, 1.2f) }, null);
            Enemy<ArcherAI>(new EnemySpec { Name = "Archer", Ase = ArcherAse, Data = "archer", BodyRadius = 0.28f, ShadowWidth = 0.9f, ModelScale = 1f, Hurtbox = new Vector2(0.7f, 1.2f) },
                            ai => Set(ai, ("arrowPrefab", Prefab("Projectiles/Arrow").GetComponent<Projectile>())));
            var slimeling = Enemy<SlimeAI>(new EnemySpec { Name = "Slimeling", Ase = SlimeAse, Data = "slimeling", BodyRadius = 0.2f, ShadowWidth = 0.7f, ModelScale = 0.6f, Hurtbox = new Vector2(0.6f, 0.5f) },
                            ai => Set(ai, ("splitPrefab", null), ("splitCount", 0)));
            Enemy<SlimeAI>(new EnemySpec { Name = "Slime", Ase = SlimeAse, Data = "slime", BodyRadius = 0.34f, ShadowWidth = 1.2f, ModelScale = 1f, Hurtbox = new Vector2(1f, 0.8f) },
                           ai => Set(ai, ("splitPrefab", slimeling.GetComponent<SlimeAI>()), ("splitCount", 2)));
            Enemy<BoneWardenAI>(new EnemySpec { Name = "BoneWarden", Ase = WardenAse, Data = "bone_warden", BodyRadius = 0.6f, ShadowWidth = 2.6f, ModelScale = 1f, KnockbackResistance = 0.85f, Hurtbox = new Vector2(1.6f, 2.6f) },
                                ai =>
                                {
                                    var shard = Prefab("Projectiles/BoneShard").GetComponent<Projectile>();
                                    Set(ai, ("shardPrefab", shard), ("minionPrefab", skeleton.GetComponent<EnemyBase>()));
                                    AddBossRules(ai.gameObject, "bone_warden", shard);
                                });
        }

        static GameObject Enemy<T>(EnemySpec spec, System.Action<T> extra) where T : EnemyBase
        {
            var go = new GameObject(spec.Name);
            var rb = Body(go, RigidbodyType2D.Dynamic);
            rb.mass = spec.BodyRadius > 0.5f ? 8f : 1f;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = spec.BodyRadius;
            col.offset = new Vector2(0f, spec.BodyRadius * 0.7f);

            var hurt = new GameObject("Hurtbox");
            hurt.transform.SetParent(go.transform, false);
            var box = hurt.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = spec.Hurtbox;
            box.offset = new Vector2(0f, spec.Hurtbox.y * 0.5f);

            Shadow(go, spec.ShadowWidth);
            var aura = AddSprite(go, "EliteAura", Spr(PropsPng, "aura0"), OrderShadow + 1, Vector3.zero, true);
            aura.transform.localScale = Vector3.one * (spec.ShadowWidth * 1.4f);
            Flipbook(aura, new[] { Spr(PropsPng, "aura0"), Spr(PropsPng, "aura1") }, 6f, true);
            aura.gameObject.SetActive(false);

            var tele = AddSprite(go, "Telegraph", Spr(VfxPng, "telegraph"), OrderTelegraph, Vector3.zero, true);
            var telegraph = tele.gameObject.AddComponent<Telegraph>();

            var (animator, renderer, flash) = spec.Entry != null ? SheetModel(go, spec.Entry) : Model(go, spec.Ase, spec.ModelScale);
            go.AddComponent<Health>();
            go.AddComponent<CombatStats>();
            var driver = Driver(go, animator, renderer);
            var ai = go.AddComponent<T>();
            Set(ai, ("data", Enemies[spec.Data]), ("anim", driver), ("flash", flash), ("telegraph", telegraph),
                ("deathVfx", Prefab("VFX/Poof")), ("knockbackResistance", spec.KnockbackResistance), ("eliteAura", aura.gameObject));
            extra?.Invoke(ai);
            return Save(go, "Enemies/" + spec.Name);
        }

        // ---------------- props ----------------

        static readonly Color Firelight = new Color(1f, 0.62f, 0.32f);

        static void BuildProps()
        {
            var torch = new GameObject("Torch");
            Flipbook(AddSprite(torch, "Flame", Spr(PropsPng, "torch0"), OrderWall, Vector3.zero), Seq(PropsPng, "torch", 4), 8f, true, 0f, true);
            PointLight(torch, new Vector3(0f, -0.4f, 0f), Firelight, 5f, 1.1f, true);
            Save(torch, "Props/Torch");

            foreach (var deco in new[] { "Banner", "Chains", "SkullNiche" })
            {
                var go = new GameObject(deco);
                AddSprite(go, "Sprite", Spr(PropsPng, char.ToLower(deco[0]) + deco.Substring(1)), OrderWall, Vector3.zero);
                Save(go, "Props/" + deco);
            }

            BreakableProp("Pot", "pot", 1, new Vector2Int(0, 3), 0.08f);
            BreakableProp("Barrel", "barrel", 2, new Vector2Int(1, 4), 0.06f);
            BreakableProp("Crate", "crate", 2, new Vector2Int(0, 5), 0.05f);

            var statue = FloorProp("Statue", Spr(PropsPng, "statue"));
            Solid(statue, new Vector2(0.85f, 0.5f));
            Save(statue, "Props/Statue");

            var brazier = FloorProp("Brazier", Spr(PropsPng, "brazier0"));
            Flipbook(brazier.GetComponentInChildren<SpriteRenderer>(), Seq(PropsPng, "brazier", 4), 9f, true, 0f, true);
            Solid(brazier, new Vector2(0.6f, 0.35f));
            PointLight(brazier, new Vector3(0f, 0.2f, 0f), Firelight, 5.5f, 1.25f, true);
            Save(brazier, "Props/Brazier");

            var candles = FloorProp("Candles", Spr(PropsPng, "candles0"));
            Flipbook(candles.GetComponentInChildren<SpriteRenderer>(), Seq(PropsPng, "candles", 3), 7f, true, 0f, true);
            PointLight(candles, new Vector3(0f, -0.1f, 0f), new Color(1f, 0.78f, 0.45f), 2.6f, 0.8f, true);
            Save(candles, "Props/Candles");

            ChestProp("Chest", "chest", "chestOpen", new Vector2(0.8f, 0.4f), 0.75f, new Vector2Int(8, 16), 0.5f);
            ChestProp("BigChest", "bigChest", "bigChestOpen", new Vector2(1.6f, 0.5f), 1.1f, new Vector2Int(25, 40), 1f);

            var spikes = new GameObject("SpikeTrap");
            var spikeSr = AddSprite(spikes, "Sprite", Spr(PropsPng, "spikeHidden"), OrderFloorFx - 10, Vector3.zero);
            spikes.AddComponent<CombatStats>();
            Set(spikes.AddComponent<SpikeTrap>(), ("spriteRenderer", spikeSr), ("hiddenSprite", Spr(PropsPng, "spikeHidden")),
                ("warnSprite", Spr(PropsPng, "spikeWarn")), ("upSprite", Spr(PropsPng, "spikeUp")));
            Save(spikes, "Props/SpikeTrap");

            var door = new GameObject("Door");
            var gate = AddSprite(door, "Gate", Spr(PropsPng, "portcullis"), OrderWall, Vector3.zero);
            gate.drawMode = SpriteDrawMode.Tiled;
            gate.tileMode = SpriteTileMode.Continuous;
            var blocker = door.AddComponent<BoxCollider2D>();
            Set(door.AddComponent<Door>(), ("spriteRenderer", gate), ("blocker", blocker),
                ("closedSprite", Spr(PropsPng, "portcullis")), ("openSprite", null));
            Save(door, "Props/Door");

            var portal = new GameObject("ExitPortal");
            Flipbook(AddSprite(portal, "Sprite", Spr(PropsPng, "portal0"), OrderFloorFx, Vector3.zero, true), Seq(PropsPng, "portal", 4), 8f, true);
            var zone = portal.AddComponent<CircleCollider2D>();
            zone.isTrigger = true;
            zone.radius = 0.8f;
            portal.AddComponent<ExitPortal>();
            PointLight(portal, Vector3.zero, new Color(0.45f, 0.75f, 0.85f), 4.5f, 1.2f, true);
            Save(portal, "Props/ExitPortal");
        }

        /// <summary>Root sits at the cell centre; the sprite's feet pivot sits on the cell's bottom edge.</summary>
        static GameObject FloorProp(string name, Sprite sprite)
        {
            var go = new GameObject(name);
            AddSprite(go, "Sprite", sprite, 0, new Vector3(0f, -0.45f, 0f));
            return go;
        }

        static void Solid(GameObject go, Vector2 size)
        {
            var box = go.AddComponent<BoxCollider2D>();
            box.size = size;
            box.offset = new Vector2(0f, -0.45f + size.y * 0.5f);
        }

        static void BreakableProp(string name, string sprite, int hp, Vector2Int gold, float potion)
        {
            var go = FloorProp(name, Spr(PropsPng, sprite));
            Solid(go, new Vector2(0.7f, 0.45f));
            go.AddComponent<Health>();
            go.AddComponent<CombatStats>();
            Set(go.AddComponent<Breakable>(), ("hitPoints", hp), ("goldRange", gold), ("potionChance", potion),
                ("shatterVfx", Prefab("VFX/Shards")), ("spriteRenderer", go.GetComponentInChildren<SpriteRenderer>()));
            Save(go, "Props/" + name);
        }

        static void ChestProp(string name, string closed, string open, Vector2 solid, float reach, Vector2Int gold, float potion)
        {
            var go = FloorProp(name, Spr(PropsPng, closed));
            Solid(go, solid);
            var trigger = go.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = reach;
            Set(go.AddComponent<Chest>(), ("spriteRenderer", go.GetComponentInChildren<SpriteRenderer>()),
                ("closedSprite", Spr(PropsPng, closed)), ("openSprite", Spr(PropsPng, open)),
                ("openVfx", Prefab("VFX/Spark")), ("goldRange", gold), ("potionChance", potion));
            Save(go, "Props/" + name);
        }
    }
}
