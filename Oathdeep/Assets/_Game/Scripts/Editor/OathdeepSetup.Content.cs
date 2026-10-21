using System.Collections.Generic;
using System.Linq;
using Oathdeep.Data;
using Oathdeep.Dungeon;
using Oathdeep.Enemies;
using Oathdeep.Loot;
using Oathdeep.Player;
using Oathdeep.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.Tilemaps;

namespace Oathdeep.EditorTools
{
    /// <summary>Stage 2: tiles, biome, items, enemy/hero data, font and UI skin. Stage 4 wires prop set + floor.</summary>
    public static partial class OathdeepSetup
    {
        const string TileDir = DataDir + "/Tiles";
        const string FontTtf = Root + "/Fonts/OathdeepPixel.ttf";
        const string FontAssetPath = Root + "/Fonts/OathdeepPixel.asset";

        static readonly Dictionary<string, ItemData> Items = new Dictionary<string, ItemData>();
        static readonly Dictionary<string, EnemyData> Enemies = new Dictionary<string, EnemyData>();
        static BiomeTiles cryptBiome;
        static readonly Dictionary<HeroId, HeroData> Heroes = new Dictionary<HeroId, HeroData>();
        static UISkin uiSkin;

        static void BuildContent(List<string> log)
        {
            cryptBiome = BuildBiome();
            BuildItems();
            BuildEnemyData();
            BuildHeroes();
            uiSkin = BuildSkin(BuildFont());
            log.Add($"content: biome, {Items.Count} items, {Enemies.Count} enemies, {Heroes.Count} heroes, font, UI skin");
        }

        // ---------------- heroes ----------------

        /// <summary>The five Unsung. Prefabs are attached later, in the prefab stage.</summary>
        static void BuildHeroes()
        {
            Hero(HeroId.Aldric, "Hero_Knight", KnightAse, "Aldric", "The Oathknight",
                 "He swore to guard a sun he has never seen. Sword, shield, and a stubbornness older than the dusk.",
                 new StatBlock(110, 12, 5, 0.08f, 1.75f, 4.2f), 5f, 0.45f, "Shield Bash",
                 "Charge behind your shield, slamming and stunning everything in your path. Untouchable while charging.", Spr(UiPng, "iconSkill"));
            Hero(HeroId.Wren, "Hero_Wren", WrenAse, "Wren", "The Lamplighter",
                 "The last lamplighter of Hollowmere kept every wick burning through forty winters of dusk. When the oil ran out " +
                 "she took her crossbow and her lantern and went down to fetch the real thing.",
                 new StatBlock(80, 9, 2, 0.12f, 1.75f, 4.8f), 6f, 0.4f, "Flare Volley",
                 "Hop back and loose five burning bolts in a wide fan. Bolts reach far - keep your distance.", Spr(HeroFxPng, "iconFlare"));
            Hero(HeroId.Morrow, "Hero_Morrow", MorrowAse, "Morrow", "The Gravesinger",
                 "Gravesingers hum the dead to sleep. Morrow hums them awake. The Choir struck his name from every hymn for it, " +
                 "so he sings his own.",
                 new StatBlock(85, 11, 2, 0.08f, 1.75f, 4.0f), 9f, 0.5f, "Requiem",
                 "Sing up three wisps of the dead. They circle you for 7 seconds, chilling and biting anything they touch. " +
                 "Your soul orbs seek their own targets.", Spr(HeroFxPng, "iconRequiem"));
            Hero(HeroId.Brakka, "Hero_Brakka", BrakkaAse, "Brakka", "The Bellbreaker",
                 "A bell-founder from the mountain halls who heard the Choir's great bell ring the Sun down. He has come to break it, " +
                 "and brought a smaller bell to do the breaking.",
                 new StatBlock(150, 15, 8, 0.05f, 1.6f, 3.5f), 7f, 0.6f, "Toll",
                 "Ring the bell-hammer into the floor. The shockwave hits and stuns every foe around you.", Spr(HeroFxPng, "iconToll"));
            Hero(HeroId.Sable, "Hero_Sable", SableAse, "Sable", "The Duskblade",
                 "Born in the dusk, raised by it, paid by it. Sable cut throats for the Choir's wardens, and knows exactly " +
                 "where their hymns are thin.",
                 new StatBlock(90, 10, 3, 0.18f, 2.0f, 5.0f), 5f, 0.35f, "Shadowstep",
                 "Melt into shadow and reappear behind the nearest foe with a devastating strike.", Spr(HeroFxPng, "iconShadowstep"));
        }

        static void Hero(HeroId id, string asset, string ase, string name, string title, string lore, StatBlock stats,
                         float skillCooldown, float dodgeCooldown, string skill, string skillText, Sprite skillIcon)
        {
            var hero = LoadOrCreate<HeroData>($"{DataDir}/{asset}.asset");
            var idle = AssetDatabase.LoadAllAssetsAtPath(ase).OfType<Sprite>()
                .Where(s => s.name.StartsWith("Frame_")).OrderBy(s => int.Parse(s.name.Substring(6))).Take(4).ToArray();
            if (idle.Length == 0) throw new System.InvalidOperationException($"No frames imported from {ase}");
            Set(hero, ("id", id), ("displayName", name), ("title", title), ("description", lore),
                ("skillCooldown", skillCooldown), ("dodgeCooldown", dodgeCooldown), ("skillName", skill),
                ("skillDescription", skillText), ("skillIcon", skillIcon), ("idleFrames", (Object[])idle));
            var so = new SerializedObject(hero);
            var block = Prop(so, "baseStats");
            block.FindPropertyRelative("maxHealth").intValue = stats.MaxHealth;
            block.FindPropertyRelative("attack").intValue = stats.Attack;
            block.FindPropertyRelative("defense").intValue = stats.Defense;
            block.FindPropertyRelative("critChance").floatValue = stats.CritChance;
            block.FindPropertyRelative("critMultiplier").floatValue = stats.CritMultiplier;
            block.FindPropertyRelative("moveSpeed").floatValue = stats.MoveSpeed;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hero);
            Heroes[id] = hero;
        }

        // ---------------- tiles + biome ----------------

        static Tile MakeTile(string sprite, Tile.ColliderType collider)
        {
            var tile = LoadOrCreate<Tile>($"{TileDir}/{sprite}.asset");
            tile.sprite = Spr(TilesPng, sprite);
            tile.colliderType = collider;
            tile.color = Color.white;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        static TileBase[] Tiles(Tile.ColliderType collider, params string[] names) =>
            names.Select(n => (TileBase)MakeTile(n, collider)).ToArray();

        static BiomeTiles BuildBiome()
        {
            var none = Tile.ColliderType.None;
            var grid = Tile.ColliderType.Grid;
            var biome = LoadOrCreate<BiomeTiles>(DataDir + "/Biome_SunkenCrypt.asset");
            biome.EditorInit(
                Tiles(none, "floor0", "floor1", "floor2", "floor3"),
                Tiles(none, "floor4", "floor5", "floor6", "floor7"),
                Tiles(grid, "face0", "face1", "face2", "face3"),
                Tiles(grid, Enumerable.Range(0, 16).Select(m => "cap" + m).ToArray()),
                MakeTile("shadowTop", none), MakeTile("shadowLeft", none), MakeTile("shadowCorner", none));
            Set(biome, ("displayName", "Sunken Crypt"), ("rareFloorChance", 0.06f),
                ("ambientLight", new Color(0.42f, 0.46f, 0.66f)), ("ambientIntensity", 0.42f));
            EditorUtility.SetDirty(biome);
            return biome;
        }

        // ---------------- items ----------------

        static void BuildItems()
        {
            Item("rusted_blade", "Rusted Blade", "Notched, but it remembers its oath.", "iconSword", Rarity.Common, EquipSlot.Weapon, new StatBlock(0, 4, 0, 0f, 0f, 0f));
            Item("oathsworn_greatsword", "Oathsworn Greatsword", "Heavy enough to split a crypt door.", "iconGreatsword", Rarity.Rare, EquipSlot.Weapon, new StatBlock(0, 9, 0, 0.05f, 0.25f, -0.2f));
            Item("crypt_plate", "Crypt Plate", "Cold iron, still warm from its last owner.", "iconArmor", Rarity.Uncommon, EquipSlot.Armor, new StatBlock(25, 0, 4, 0f, 0f, -0.1f));
            Item("pilgrim_tabard", "Pilgrim's Tabard", "Light cloth for long descents.", "iconTabard", Rarity.Common, EquipSlot.Armor, new StatBlock(15, 0, 2, 0f, 0f, 0.2f));
            Item("helm_of_vigil", "Helm of the Vigil", "Its wearer never sleeps below ground.", "iconHelm", Rarity.Uncommon, EquipSlot.Armor, new StatBlock(20, 0, 3, 0f, 0f, 0f));
            Item("ember_ring", "Ember Ring", "A spark that never went out.", "iconRing", Rarity.Rare, EquipSlot.Accessory, new StatBlock(0, 3, 0, 0.08f, 0.3f, 0f));
            Item("warden_amulet", "Warden's Amulet", "Taken from the Bone Warden's throat.", "iconAmulet", Rarity.Epic, EquipSlot.Accessory, new StatBlock(30, 4, 2, 0.05f, 0f, 0f));
        }

        static void Item(string id, string name, string desc, string icon, Rarity rarity, EquipSlot slot, StatBlock bonus)
        {
            var item = LoadOrCreate<ItemData>($"{DataDir}/Items/Item_{id}.asset");
            item.EditorInit(id, name, desc, Spr(PropsPng, icon), rarity, slot, bonus);
            EditorUtility.SetDirty(item);
            Items[id] = item;
        }

        static ItemData[] ItemsOf(params string[] ids) => ids.Select(i => Items[i]).ToArray();

        // ---------------- enemy data ----------------

        static void BuildEnemyData()
        {
            Enemy("skeleton", "Skeleton", new StatBlock(34, 7, 2, 0.03f, 1.5f, 2.4f), 8f, 1.3f, new Vector2Int(1, 4), 0.06f, 0.06f,
                  ItemsOf("rusted_blade", "pilgrim_tabard", "helm_of_vigil"), false);
            Enemy("archer", "Skeleton Archer", new StatBlock(24, 6, 0, 0.05f, 1.5f, 2.3f), 10f, 6f, new Vector2Int(2, 5), 0.06f, 0.05f,
                  ItemsOf("ember_ring", "pilgrim_tabard"), false);
            Enemy("slime", "Bog Slime", new StatBlock(30, 6, 0, 0f, 1.5f, 2f), 7f, 1.6f, new Vector2Int(1, 3), 0.03f, 0.08f,
                  ItemsOf("crypt_plate"), false);
            Enemy("slimeling", "Slimeling", new StatBlock(10, 4, 0, 0f, 1.5f, 2.6f), 7f, 1.2f, new Vector2Int(0, 1), 0f, 0.02f,
                  new ItemData[0], false);
            Enemy("bone_warden", "The Bone Warden", new StatBlock(520, 15, 8, 0.05f, 1.6f, 2.1f), 22f, 2.4f, new Vector2Int(40, 60), 1f, 1f,
                  ItemsOf("oathsworn_greatsword", "warden_amulet"), true);
        }

        static void Enemy(string id, string name, StatBlock stats, float aggro, float range, Vector2Int gold, float itemChance,
                          float potionChance, ItemData[] loot, bool boss)
        {
            var data = LoadOrCreate<EnemyData>($"{DataDir}/Enemies/Enemy_{id}.asset");
            data.EditorInit(name, stats, aggro, range, gold, itemChance, potionChance, loot, boss);
            EditorUtility.SetDirty(data);
            Enemies[id] = data;
        }

        // ---------------- font + skin ----------------

        static TMP_FontAsset BuildFont()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (existing != null) return existing;
            var font = AssetDatabase.LoadAssetAtPath<Font>(FontTtf);
            if (font == null) throw new System.IO.FileNotFoundException("Pixel font missing", FontTtf);
            // Raster at 16pt = exactly 2 texels per font pixel; point filtering keeps edges hard at any integer scale.
            var asset = TMP_FontAsset.CreateFontAsset(font, 16, 2, GlyphRenderMode.RASTER, 256, 256, AtlasPopulationMode.Dynamic, false);
            asset.name = "OathdeepPixel";
            AssetDatabase.CreateAsset(asset, FontAssetPath);
            var atlas = asset.atlasTexture;
            atlas.name = "OathdeepPixel Atlas";
            atlas.filterMode = FilterMode.Point;
            AssetDatabase.AddObjectToAsset(atlas, asset);
            asset.material.name = "OathdeepPixel Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            asset.TryAddCharacters(string.Concat(Enumerable.Range(32, 95).Select(c => (char)c)), out _);
            asset.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        static UISkin BuildSkin(TMP_FontAsset font)
        {
            var skin = LoadOrCreate<UISkin>(DataDir + "/UISkin.asset");
            skin.font = font;
            skin.panel = Spr(UiPng, "panel");
            skin.button = Spr(UiPng, "button");
            skin.buttonPressed = Spr(UiPng, "buttonPressed");
            skin.barFrame = Spr(UiPng, "barFrame");
            skin.barFillHealth = Spr(UiPng, "barFillHealth");
            skin.barFillBoss = Spr(UiPng, "barFillBoss");
            skin.iconGold = Spr(UiPng, "iconGold");
            skin.iconPotion = Spr(UiPng, "iconPotion");
            skin.iconAttack = Spr(UiPng, "iconAttack");
            skin.iconDodge = Spr(UiPng, "iconDodge");
            skin.iconSkill = Spr(UiPng, "iconSkill");
            skin.iconPause = Spr(UiPng, "iconPause");
            skin.actionRing = Spr(UiPng, "actionRing");
            skin.joystickBase = Spr(UiPng, "joystickBase");
            skin.joystickKnob = Spr(UiPng, "joystickKnob");
            skin.logo = AssetDatabase.LoadAssetAtPath<Sprite>(LogoPng);
            skin.menuBackground = AssetDatabase.LoadAssetAtPath<Sprite>(MenuBgPng);
            skin.iconSettings = Spr(MenuIconsPng, "gear");
            skin.iconInfo = Spr(MenuIconsPng, "info");
            skin.iconShare = Spr(MenuIconsPng, "share");
            skin.iconRate = Spr(MenuIconsPng, "star");
            skin.iconCodex = Spr(MenuIconsPng, "codex");
            skin.iconQuest = Spr(MenuIconsPng, "quest");
            skin.iconHeroes = Spr(MenuIconsPng, "heroes");
            skin.iconBack = Spr(MenuIconsPng, "back");
            skin.iconClose = Spr(MenuIconsPng, "close");
            skin.iconBag = Spr(MenuIconsPng, "bag");
            skin.white = AssetDatabase.LoadAssetAtPath<Sprite>(WhitePng);
            EditorUtility.SetDirty(skin);
            return skin;
        }

        // ---------------- stage 4: needs prefabs ----------------

        static PropSet cryptProps;
        static FloorConfig floorOne;

        static void FinishContent(List<string> log)
        {
            cryptProps = LoadOrCreate<PropSet>(DataDir + "/Props_Crypt.asset");
            cryptProps.torch = Prefab("Props/Torch");
            cryptProps.wallDecor = new[] { Prefab("Props/Banner"), Prefab("Props/Chains"), Prefab("Props/SkullNiche") };
            cryptProps.decals = Tiles(Tile.ColliderType.None, "decalBones", "decalSkull", "decalCrack", "decalBlood", "decalMoss",
                                      "decalPuddle", "decalRubble", "decalScratch");
            cryptProps.cornerDecals = Tiles(Tile.ColliderType.None, "cobwebTL", "cobwebTR");
            cryptProps.breakables = new[] { Prefab("Props/Pot"), Prefab("Props/Barrel"), Prefab("Props/Crate") };
            cryptProps.statue = Prefab("Props/Statue");
            cryptProps.brazier = Prefab("Props/Brazier");
            cryptProps.candles = Prefab("Props/Candles");
            cryptProps.chest = Prefab("Props/Chest").GetComponent<Chest>();
            cryptProps.bigChest = Prefab("Props/BigChest").GetComponent<Chest>();
            cryptProps.spikeTrap = Prefab("Props/SpikeTrap").GetComponent<SpikeTrap>();
            cryptProps.door = Prefab("Props/Door").GetComponent<Door>();
            cryptProps.exitPortal = Prefab("Props/ExitPortal");
            cryptProps.spawnMarker = Prefab("VFX/SpawnMarker");
            EditorUtility.SetDirty(cryptProps);

            floorOne = LoadOrCreate<FloorConfig>(DataDir + "/Floor_01.asset");
            floorOne.EditorInit(cryptBiome, new[]
            {
                new WeightedEnemy { Prefab = Prefab("Enemies/Skeleton").GetComponent<EnemyBase>(), Weight = 5f },
                new WeightedEnemy { Prefab = Prefab("Enemies/Archer").GetComponent<EnemyBase>(), Weight = 3f },
                new WeightedEnemy { Prefab = Prefab("Enemies/Slime").GetComponent<EnemyBase>(), Weight = 3f },
            }, Prefab("Enemies/BoneWarden").GetComponent<EnemyBase>(), ItemsOf("crypt_plate", "ember_ring", "helm_of_vigil", "pilgrim_tabard"));
            EditorUtility.SetDirty(floorOne);
            log.Add("content: prop set + floor 1 wired");
        }
    }
}
