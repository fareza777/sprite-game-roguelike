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

        static readonly Dictionary<string, EnemyData> Enemies = new Dictionary<string, EnemyData>();
        static BiomeTiles cryptBiome;
        static readonly Dictionary<HeroId, HeroData> Heroes = new Dictionary<HeroId, HeroData>();
        static UISkin uiSkin;

        static void BuildContent(List<string> log)
        {
            BuildBiomes();
            cryptBiome = Biomes["crypt"];
            BuildEnemyData();
            BuildHeroes();
            itemIcons = BuildItemIcons();
            uiSkin = BuildSkin(BuildFont());
            log.Add($"content: biome, {Enemies.Count} enemies, {Heroes.Count} heroes, {ItemCatalog.Count} item identities, font, UI skin");
        }

        // ---------------- items ----------------

        static ItemIcons itemIcons;

        /// <summary>Icon lookup for the procedural item catalog. Fails loudly if the catalog names an icon we never drew.</summary>
        static ItemIcons BuildItemIcons()
        {
            var missing = ItemCatalog.IconKeys.Where(k => !ItemIconNames.Contains(k)).ToList();
            if (missing.Count > 0) throw new System.InvalidOperationException("Item icons not drawn: " + string.Join(", ", missing));
            var asset = LoadOrCreate<ItemIcons>(DataDir + "/ItemIcons.asset");
            asset.EditorInit(ItemIconNames.Select(n => new ItemIcons.Entry { Key = n, Sprite = Spr(ItemIconsPng, n) }).ToArray(),
                             Spr(ItemIconsPng, "sword"));
            EditorUtility.SetDirty(asset);
            return asset;
        }

        // ---------------- heroes ----------------

        /// <summary>The five Unsung. Prefabs are attached later, in the prefab stage.</summary>
        static void BuildHeroes()
        {
            Hero(HeroId.Aldric, "Hero_Knight", KnightAse, "Aldric", "The Oathknight",
                 "He swore to guard a sun he has never seen. Sword, shield, and a stubbornness older than the dusk.",
                 new StatBlock(110, 12, 5, 0.08f, 1.75f, 4.2f), 5f, 0.45f, "Shield Bash",
                 "Charge behind your shield, slamming and stunning everything in your path. Untouchable while charging.", Spr(InterfacePng, "iconSkill"));
            Hero(HeroId.Wren, "Hero_Wren", WrenAse, "Wren", "The Lamplighter",
                 "The last lamplighter of Hollowmere kept every wick burning through forty winters of dusk. When the oil ran out " +
                 "she took her crossbow and her lantern and went down to fetch the real thing.",
                 new StatBlock(80, 9, 2, 0.12f, 1.75f, 4.8f), 6f, 0.4f, "Flare Volley",
                 "Hop back and loose five burning bolts in a wide fan. Bolts reach far - keep your distance.", Spr(InterfacePng, "iconFlare"));
            Hero(HeroId.Morrow, "Hero_Morrow", MorrowAse, "Morrow", "The Gravesinger",
                 "Gravesingers hum the dead to sleep. Morrow hums them awake. The Choir struck his name from every hymn for it, " +
                 "so he sings his own.",
                 new StatBlock(85, 11, 2, 0.08f, 1.75f, 4.0f), 9f, 0.5f, "Requiem",
                 "Sing up three wisps of the dead. They circle you for 7 seconds, chilling and biting anything they touch. " +
                 "Your soul orbs seek their own targets.", Spr(InterfacePng, "iconRequiem"));
            Hero(HeroId.Brakka, "Hero_Brakka", BrakkaAse, "Brakka", "The Bellbreaker",
                 "A bell-founder from the mountain halls who heard the Choir's great bell ring the Sun down. He has come to break it, " +
                 "and brought a smaller bell to do the breaking.",
                 new StatBlock(150, 15, 8, 0.05f, 1.6f, 3.5f), 7f, 0.6f, "Toll",
                 "Ring the bell-hammer into the floor. The shockwave hits and stuns every foe around you.", Spr(InterfacePng, "iconToll"));
            Hero(HeroId.Sable, "Hero_Sable", SableAse, "Sable", "The Duskblade",
                 "Born in the dusk, raised by it, paid by it. Sable cut throats for the Choir's wardens, and knows exactly " +
                 "where their hymns are thin.",
                 new StatBlock(90, 10, 3, 0.18f, 2.0f, 5.0f), 5f, 0.35f, "Shadowstep",
                 "Melt into shadow and reappear behind the nearest foe with a devastating strike.", Spr(InterfacePng, "iconShadowstep"));
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

        // ---------------- enemy data ----------------

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
            skin.iconGold = Spr(InterfacePng, "iconGold");
            skin.iconPotion = Spr(InterfacePng, "iconPotion");
            skin.iconAttack = Spr(InterfacePng, "iconAttack");
            skin.iconDodge = Spr(InterfacePng, "iconDodge");
            skin.iconSkill = Spr(InterfacePng, "iconSkill");
            skin.iconPause = Spr(InterfacePng, "iconPause");
            skin.actionRing = AssetDatabase.LoadAssetAtPath<Sprite>(ActionRingPng);
            skin.joystickBase = AssetDatabase.LoadAssetAtPath<Sprite>(JoystickBasePng);
            skin.joystickKnob = AssetDatabase.LoadAssetAtPath<Sprite>(JoystickKnobPng);
            skin.logo = AssetDatabase.LoadAssetAtPath<Sprite>(LogoPng);
            skin.menuBackground = AssetDatabase.LoadAssetAtPath<Sprite>(MenuBgPng);
            skin.iconSettings = Spr(InterfacePng, "gear");
            skin.iconInfo = Spr(InterfacePng, "info");
            skin.iconShare = Spr(InterfacePng, "share");
            skin.iconRate = Spr(InterfacePng, "star");
            skin.iconCodex = Spr(InterfacePng, "codex");
            skin.iconQuest = Spr(InterfacePng, "quest");
            skin.iconHeroes = Spr(InterfacePng, "heroes");
            skin.iconBack = Spr(InterfacePng, "back");
            skin.iconClose = Spr(InterfacePng, "close");
            skin.iconBag = Spr(InterfacePng, "bag");
            skin.white = AssetDatabase.LoadAssetAtPath<Sprite>(WhitePng);
            EditorUtility.SetDirty(skin);
            return skin;
        }

        // ---------------- stage 4: needs prefabs ----------------

        static PropSet cryptProps;

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
            WireEncounters(cryptProps);
            EditorUtility.SetDirty(cryptProps);

            BuildFloors();
            log.Add($"content: prop set + {Floors.Count} floors wired");
        }
    }
}
