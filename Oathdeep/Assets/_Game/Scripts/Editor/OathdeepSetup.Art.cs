using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Oathdeep.EditorTools
{
    /// <summary>Stage 1: importer settings and sprite slicing for every art sheet.</summary>
    public static partial class OathdeepSetup
    {
        const string ArtDir = Root + "/Art";
        const string KnightAse = ArtDir + "/Characters/knight.aseprite";
        const string WrenAse = ArtDir + "/Characters/wren.aseprite";
        const string MorrowAse = ArtDir + "/Characters/morrow.aseprite";
        const string BrakkaAse = ArtDir + "/Characters/brakka.aseprite";
        const string SableAse = ArtDir + "/Characters/sable.aseprite";
        const string HeroFxPng = ArtDir + "/VFX/hero_fx.png";
        const string SkeletonAse = ArtDir + "/Enemies/skeleton.aseprite";
        const string ArcherAse = ArtDir + "/Enemies/archer.aseprite";
        const string SlimeAse = ArtDir + "/Enemies/slime.aseprite";
        const string WardenAse = ArtDir + "/Enemies/bone_warden.aseprite";
        const string TilesPng = ArtDir + "/Tiles/crypt_tiles.png";
        const string PropsPng = ArtDir + "/Props/crypt_props.png";
        const string VfxPng = ArtDir + "/VFX/vfx.png";
        const string UiPng = ArtDir + "/UI/ui.png";
        const string WhitePng = ArtDir + "/UI/white.png";
        const string MenuIconsPng = ArtDir + "/UI/menu_icons.png";
        const string LogoPng = ArtDir + "/UI/logo_undersun.png";
        const string MenuBgPng = ArtDir + "/UI/menu_bg.png";
        static readonly string[] MenuIconNames = { "gear", "info", "share", "star", "codex", "quest", "heroes", "back", "close", "bag" };
        const float UiPpu = 100f;   // 1 art pixel = 1 reference-canvas unit

        static readonly Vector2 Centre = new Vector2(0.5f, 0.5f);
        static readonly Vector2 Feet = new Vector2(0.5f, 0f);

        struct Cell
        {
            public string Name; public RectInt Px; public Vector2 Pivot; public Vector4 Border;
            public Cell(string name, int x, int y, int w, int h, Vector2 pivot, Vector4 border = default)
            { Name = name; Px = new RectInt(x, y, w, h); Pivot = pivot; Border = border; }
        }

        static void ConfigureArt(List<string> log)
        {
            // Pivot = feet row measured from each sprite's first frame (canvas space).
            foreach (var hero in new[] { KnightAse, WrenAse, MorrowAse, BrakkaAse, SableAse })
                ConfigureAseprite(hero, new Vector2(0.5f, 6f / 48f));
            ConfigureAseprite(SkeletonAse, new Vector2(0.5f, 1f / 32f));
            ConfigureAseprite(ArcherAse, new Vector2(0.5f, 1f / 32f));
            ConfigureAseprite(SlimeAse, new Vector2(0.5f, 3f / 32f));
            ConfigureAseprite(WardenAse, new Vector2(0.5f, 2f / 64f));

            Slice(TilesPng, TileCells(), WorldPpu, SpriteMeshType.FullRect);
            Slice(PropsPng, PropCells(), WorldPpu, SpriteMeshType.FullRect);
            Slice(VfxPng, VfxCells(), WorldPpu, SpriteMeshType.FullRect);
            Slice(HeroFxPng, HeroFxCells(), WorldPpu, SpriteMeshType.FullRect);
            Slice(UiPng, UiCells(), UiPpu, SpriteMeshType.FullRect);
            WriteWhiteSprite();
            Slice(MenuIconsPng, MenuIconNames.Select((n, i) => G(n, i, 0, Centre)).ToList(), UiPpu, SpriteMeshType.FullRect);
            SingleSprite(LogoPng, UiPpu);
            SingleSprite(MenuBgPng, UiPpu);
            SpriteCache.Clear();
            log.Add("art: 9 aseprite importers, 5 sliced sheets");
        }

        static void ConfigureAseprite(string path, Vector2 pivot)
        {
            var imp = AssetImporter.GetAtPath(path) as AsepriteImporter;
            if (imp == null) throw new FileNotFoundException("Aseprite asset missing or not imported", path);
            imp.importMode = FileImportModes.AnimatedSprite;
            imp.layerImportMode = LayerImportModes.MergeFrame;
            imp.spritePixelsPerUnit = WorldPpu;
            imp.filterMode = FilterMode.Point;
            imp.mipmapEnabled = false;
            imp.pivotSpace = PivotSpaces.Canvas;
            imp.pivotAlignment = SpriteAlignment.Custom;
            imp.customPivotPosition = pivot;
            imp.generateModelPrefab = true;
            imp.generateAnimationClips = true;
            var platform = imp.GetImporterPlatformSettings(BuildTarget.NoTarget);
            platform.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SetImporterPlatformSettings(platform);
            imp.SaveAndReimport();
        }

        static void Slice(string path, IReadOnlyList<Cell> cells, float ppu, SpriteMeshType mesh)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) throw new FileNotFoundException("Texture missing", path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.spritePixelsPerUnit = ppu;
            ti.filterMode = FilterMode.Point;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = mesh;
            settings.spriteExtrude = 0;
            ti.SetTextureSettings(settings);
            ti.SaveAndReimport();
            ti.GetSourceTextureWidthAndHeight(out _, out int height);

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dp = factory.GetSpriteEditorDataProviderFromObject(ti);
            dp.InitSpriteEditorDataProvider();
            var ids = dp.GetSpriteRects().GroupBy(r => r.name).ToDictionary(g => g.Key, g => g.First().spriteID);
            var rects = cells.Select(c => new SpriteRect
            {
                name = c.Name,
                spriteID = ids.TryGetValue(c.Name, out var id) ? id : GUID.Generate(),
                rect = new Rect(c.Px.x, height - c.Px.y - c.Px.height, c.Px.width, c.Px.height),
                alignment = SpriteAlignment.Custom,
                pivot = c.Pivot,
                border = c.Border,
            }).ToArray();
            dp.SetSpriteRects(rects);
            dp.GetDataProvider<ISpriteNameFileIdDataProvider>()
              .SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
            dp.Apply();
            ti.SaveAndReimport();
        }

        static Cell G(string name, int col, int row, Vector2 pivot) => new Cell(name, col * 16, row * 16, 16, 16, pivot);

        static List<Cell> TileCells()
        {
            var c = new List<Cell>();
            for (int i = 0; i < 8; i++) c.Add(G("floor" + i, i, 0, Centre));
            for (int i = 0; i < 4; i++) c.Add(G("face" + i, i, 1, Centre));
            for (int m = 0; m < 16; m++) c.Add(G("cap" + m, m % 8, 2 + m / 8, Centre));
            string[] row4 = { "shadowTop", "shadowLeft", "shadowCorner", "decalBones", "decalSkull", "decalCrack", "decalBlood", "decalMoss" };
            for (int i = 0; i < row4.Length; i++) c.Add(G(row4[i], i, 4, Centre));
            string[] row5 = { "decalPuddle", "decalRubble", "cobwebTL", "cobwebTR", "decalScratch" };
            for (int i = 0; i < row5.Length; i++) c.Add(G(row5[i], i, 5, Centre));
            return c;
        }

        static List<Cell> PropCells()
        {
            var c = new List<Cell>();
            for (int i = 0; i < 4; i++) c.Add(G("torch" + i, i, 0, Centre));
            for (int i = 0; i < 4; i++) c.Add(G("brazier" + i, 4 + i, 0, Feet));
            for (int i = 0; i < 3; i++) c.Add(G("candles" + i, 8 + i, 0, Feet));
            c.Add(G("banner", 11, 0, Centre)); c.Add(G("chains", 12, 0, Centre)); c.Add(G("skullNiche", 13, 0, Centre));
            c.Add(G("pot", 14, 0, Feet)); c.Add(G("barrel", 15, 0, Feet));
            c.Add(G("crate", 0, 1, Feet));
            for (int i = 0; i < 3; i++) c.Add(G("shards" + i, 1 + i, 1, Centre));
            c.Add(G("chest", 4, 1, Feet)); c.Add(G("chestOpen", 5, 1, Feet));
            c.Add(G("spikeHidden", 6, 1, Centre)); c.Add(G("spikeWarn", 7, 1, Centre)); c.Add(G("spikeUp", 8, 1, Centre));
            c.Add(G("portcullis", 9, 1, Centre));
            for (int i = 0; i < 4; i++) c.Add(G("marker" + i, 10 + i, 1, Centre));
            c.Add(G("potion", 14, 1, Centre));
            c.Add(G("coin0", 15, 1, Centre));
            for (int i = 1; i < 4; i++) c.Add(G("coin" + i, i - 1, 2, Centre));
            string[] icons = { "iconSword", "iconGreatsword", "iconArmor", "iconTabard", "iconRing", "iconAmulet", "iconHelm" };
            for (int i = 0; i < icons.Length; i++) c.Add(G(icons[i], 3 + i, 2, Centre));
            for (int i = 0; i < 3; i++) c.Add(G("spark" + i, 10 + i, 2, Centre));
            for (int i = 0; i < 3; i++) c.Add(G("dust" + i, 13 + i, 2, Centre));
            c.Add(new Cell("statue", 0, 48, 16, 32, Feet));
            c.Add(new Cell("bigChest", 16, 48, 32, 16, Feet));
            c.Add(new Cell("bigChestOpen", 48, 48, 32, 16, Feet));
            for (int i = 0; i < 4; i++) c.Add(new Cell("portal" + i, 80 + 32 * i, 48, 32, 32, Centre));
            c.Add(G("arrow", 13, 3, Centre)); c.Add(G("boneShard", 14, 3, Centre));
            c.Add(G("aura0", 15, 3, Centre)); c.Add(G("aura1", 13, 4, Centre));
            for (int i = 0; i < 4; i++) c.Add(G("poof" + i, i, 5, Centre));
            return c;
        }

        static List<Cell> VfxCells()
        {
            var c = new List<Cell>();
            // Arcs are drawn around x=4 (slash) / x=6 (bash) so they extend forward from the spawn point.
            for (int i = 0; i < 3; i++) c.Add(new Cell("slash" + i, 32 * i, 0, 32, 32, new Vector2(4f / 32f, 0.5f)));
            for (int i = 0; i < 3; i++) c.Add(new Cell("bash" + i, 96 + 32 * i, 0, 32, 32, new Vector2(6f / 32f, 0.5f)));
            c.Add(new Cell("telegraph", 0, 32, 16, 16, Centre, new Vector4(5, 5, 5, 5)));
            c.Add(new Cell("shadow", 16, 32, 16, 16, Centre));
            c.Add(new Cell("glow", 32, 32, 16, 16, Centre));
            return c;
        }

        static List<Cell> HeroFxCells()
        {
            var c = new List<Cell>();
            string[] small = { "bolt", "flare", "orb0", "orb1", "wisp0", "wisp1", "shadow0", "shadow1" };
            for (int i = 0; i < small.Length; i++) c.Add(G(small[i], i, 0, Centre));
            for (int i = 0; i < 4; i++) c.Add(new Cell("ring" + i, 32 * i, 16, 32, 32, Centre));
            string[] icons = { "iconFlare", "iconRequiem", "iconToll", "iconShadowstep" };
            for (int i = 0; i < icons.Length; i++) c.Add(G(icons[i], i, 3, Centre));
            return c;
        }

        static List<Cell> UiCells() => new List<Cell>
        {
            new Cell("panel", 0, 0, 24, 24, Centre, new Vector4(4, 4, 4, 4)),
            new Cell("button", 24, 0, 48, 20, Centre, new Vector4(6, 6, 6, 6)),
            new Cell("buttonPressed", 72, 0, 48, 20, Centre, new Vector4(6, 6, 6, 6)),
            new Cell("barFrame", 120, 0, 16, 12, Centre, new Vector4(3, 3, 3, 3)),
            new Cell("barFillHealth", 136, 0, 4, 12, Centre),
            new Cell("barFillBoss", 140, 0, 4, 12, Centre),
            new Cell("iconGold", 0, 24, 16, 16, Centre),
            new Cell("iconPotion", 16, 24, 16, 16, Centre),
            new Cell("iconAttack", 32, 24, 16, 16, Centre),
            new Cell("iconDodge", 48, 24, 16, 16, Centre),
            new Cell("iconSkill", 64, 24, 16, 16, Centre),
            new Cell("iconPause", 80, 24, 16, 16, Centre),
            new Cell("actionRing", 96, 24, 48, 48, Centre),
            new Cell("joystickBase", 144, 24, 56, 56, Centre),
            new Cell("joystickKnob", 200, 24, 26, 26, Centre),
            new Cell("logo", 0, 80, 160, 64, Centre),
        };

        static void SingleSprite(string path, float ppu)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) throw new FileNotFoundException("Texture missing", path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = ppu;
            ti.filterMode = FilterMode.Point;
            ti.mipmapEnabled = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }

        /// <summary>4x4 pure white sprite for fills, dims and minimap blocks (utility, not art).</summary>
        static void WriteWhiteSprite()
        {
            if (!File.Exists(WhitePng))
            {
                var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                tex.SetPixels(Enumerable.Repeat(Color.white, 16).ToArray());
                File.WriteAllBytes(WhitePng, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(WhitePng);
            }
            var ti = (TextureImporter)AssetImporter.GetAtPath(WhitePng);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = UiPpu;
            ti.filterMode = FilterMode.Point;
            ti.spriteBorder = new Vector4(1, 1, 1, 1);
            ti.SaveAndReimport();
        }
    }
}
