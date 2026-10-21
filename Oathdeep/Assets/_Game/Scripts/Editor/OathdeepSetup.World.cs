using System.Collections.Generic;
using System.Linq;
using Oathdeep.Core;
using Oathdeep.Dungeon;
using Oathdeep.Enemies;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Oathdeep.EditorTools
{
    /// <summary>The ten biomes, their floor configs and the peaceful-room encounter prefabs.</summary>
    public static partial class OathdeepSetup
    {
        const string NpcPng = ArtDir + "/Props/npcs.png";

        /// <summary>Biome id (sheet Art/Tiles/&lt;id&gt;_tiles.png), display name, ambient light, intensity.</summary>
        static readonly (string id, string name, Color light, float intensity)[] BiomeDefs =
        {
            ("crypt", "Sunken Crypt", new Color(0.42f, 0.46f, 0.66f), 0.42f),
            ("hollow", "Mycelial Hollow", new Color(0.62f, 0.45f, 0.75f), 0.45f),
            ("cistern", "Drowned Cistern", new Color(0.40f, 0.62f, 0.70f), 0.45f),
            ("forge", "Ember Forge", new Color(0.85f, 0.50f, 0.35f), 0.50f),
            ("frost", "Frostbone Caverns", new Color(0.62f, 0.78f, 0.95f), 0.52f),
            ("thornwild", "Thornwild Ruins", new Color(0.50f, 0.72f, 0.45f), 0.48f),
            ("clockwork", "Clockwork Ossuary", new Color(0.80f, 0.65f, 0.45f), 0.45f),
            ("necropolis", "Sandsunk Necropolis", new Color(0.95f, 0.80f, 0.55f), 0.52f),
            ("glass", "Glass Abyss", new Color(0.55f, 0.55f, 0.95f), 0.40f),
            ("dusk", "Heart of Dusk", new Color(0.80f, 0.35f, 0.40f), 0.38f),
        };

        static readonly string[] NpcNames =
        {
            "tallow0", "tallow1", "pilgrim0", "pilgrim1", "sunstone0", "sunstone1", "fire0", "fire1",
            "fire2", "fire3", "altar", "fragment0", "fragment1", "bubble",
        };

        static readonly Oathdeep.Audio.MusicTrack[] BiomeTracks =
        {
            Oathdeep.Audio.MusicTrack.Crypt, Oathdeep.Audio.MusicTrack.Hollow, Oathdeep.Audio.MusicTrack.Cistern,
            Oathdeep.Audio.MusicTrack.Forge, Oathdeep.Audio.MusicTrack.Frost, Oathdeep.Audio.MusicTrack.Thornwild,
            Oathdeep.Audio.MusicTrack.Clockwork, Oathdeep.Audio.MusicTrack.Necropolis, Oathdeep.Audio.MusicTrack.Glass,
            Oathdeep.Audio.MusicTrack.Dusk,
        };

        static string BiomePng(string id) => $"{ArtDir}/Tiles/{id}_tiles.png";

        static readonly Dictionary<string, BiomeTiles> Biomes = new Dictionary<string, BiomeTiles>();
        static readonly List<FloorConfig> Floors = new List<FloorConfig>();

        // ---------------- art ----------------

        static void ConfigureWorldArt()
        {
            foreach (var b in BiomeDefs) Slice(BiomePng(b.id), TileCells(), WorldPpu, SpriteMeshType.FullRect);
            var feet = new Vector2(0.5f, 2f / 32f);
            Slice(NpcPng, NpcNames.Select((n, i) => new Cell(n, (i % 8) * 32, (i / 8) * 32, 32, 32, n == "bubble" ? Centre : feet)).ToList(),
                  WorldPpu, SpriteMeshType.FullRect);
        }

        // ---------------- biomes ----------------

        static Tile BiomeTile(string biome, string sprite, Tile.ColliderType collider)
        {
            var tile = LoadOrCreate<Tile>($"{TileDir}/{biome}/{sprite}.asset");
            tile.sprite = Spr(BiomePng(biome), sprite);
            tile.colliderType = collider;
            tile.color = Color.white;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        static TileBase[] BiomeTileSet(string biome, Tile.ColliderType collider, params string[] names) =>
            names.Select(n => (TileBase)BiomeTile(biome, n, collider)).ToArray();

        static void BuildBiomes()
        {
            var none = Tile.ColliderType.None;
            var grid = Tile.ColliderType.Grid;
            Biomes.Clear();
            for (int i = 0; i < BiomeDefs.Length; i++)
            {
                var (id, name, light, intensity) = BiomeDefs[i];
                var biome = LoadOrCreate<BiomeTiles>($"{DataDir}/Biomes/Biome_{i + 1:00}_{id}.asset");
                biome.EditorInit(
                    BiomeTileSet(id, none, "floor0", "floor1", "floor2", "floor3"),
                    BiomeTileSet(id, none, "floor4", "floor5", "floor6", "floor7"),
                    BiomeTileSet(id, grid, "face0", "face1", "face2", "face3"),
                    BiomeTileSet(id, grid, Enumerable.Range(0, 16).Select(m => "cap" + m).ToArray()),
                    BiomeTile(id, "shadowTop", none), BiomeTile(id, "shadowLeft", none), BiomeTile(id, "shadowCorner", none),
                    // The biome's own three decals appear twice as often as the shared ones.
                    BiomeTileSet(id, none, "decalBones", "decalSkull", "decalCrack", "decalBlood", "decalMoss", "decalPuddle",
                               "decalRubble", "decalScratch", "decalBiome0", "decalBiome1", "decalBiome2",
                               "decalBiome0", "decalBiome1", "decalBiome2"),
                    BiomeTileSet(id, none, "cobwebTL", "cobwebTR"));
                Set(biome, ("displayName", name), ("rareFloorChance", 0.06f), ("ambientLight", light), ("ambientIntensity", intensity),
                    ("music", BiomeTracks[i]), ("bossMusic", i < 4 ? Oathdeep.Audio.MusicTrack.Boss : i < 9 ? Oathdeep.Audio.MusicTrack.BossDeep
                                                                                                    : Oathdeep.Audio.MusicTrack.BossFinal));
                EditorUtility.SetDirty(biome);
                Biomes[id] = biome;
            }
        }

        // ---------------- encounters ----------------

        static void BuildEncounters()
        {
            var fire = new Color(1f, 0.7f, 0.4f);
            var sun = new Color(1f, 0.85f, 0.5f);
            Encounter("Merchant", Seq(NpcPng, "tallow", 2), 3f, "tallow0", new Vector2(1.4f, 0.5f), new Color(1f, 0.8f, 0.5f), 3.5f);
            Encounter("Pilgrim", Seq(NpcPng, "pilgrim", 2), 2.5f, "pilgrim0", new Vector2(0.8f, 0.4f), Color.clear, 0f);
            Encounter("Sunstone", Seq(NpcPng, "sunstone", 2), 3f, "sunstone0", new Vector2(1.6f, 0.6f), sun, 5f);
            Encounter("Campfire", Seq(NpcPng, "fire", 4), 9f, "fire0", new Vector2(1.2f, 0.5f), fire, 6f);
            Encounter("DiceAltar", new[] { Spr(NpcPng, "altar") }, 0f, "altar", new Vector2(1.3f, 0.6f), new Color(0.9f, 0.4f, 0.4f), 2.5f);
            Encounter("SunShard", Seq(NpcPng, "fragment", 2), 3f, "fragment0", new Vector2(0.6f, 0.3f), sun, 4.5f);
            Encounter("WeepingStatue", new[] { Spr(PropsPng, "statue") }, 0f, null, new Vector2(0.85f, 0.5f), new Color(0.8f, 0.3f, 0.3f), 2.5f);
            Encounter("CursedChest", new[] { Spr(PropsPng, "bigChest") }, 0f, null, new Vector2(1.6f, 0.5f), new Color(0.6f, 0.3f, 0.9f), 2.5f,
                      new Color(0.75f, 0.6f, 0.85f));
        }

        static void Encounter(string name, Sprite[] frames, float fps, string portraitName, Vector2 solid, Color light, float lightRadius,
                              Color? tint = null)
        {
            var go = FloorProp(name, frames[0]);
            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (tint.HasValue) sr.color = tint.Value;
            if (frames.Length > 1) Flipbook(sr, frames, fps, true, 0f, true);
            Solid(go, solid);
            if (lightRadius > 0f) PointLight(go, new Vector3(0f, 0.6f, 0f), light, lightRadius, 1f, true);

            var bubble = AddSprite(go, "Bubble", Spr(NpcPng, "bubble"), OrderFx + 2, new Vector3(0.1f, 2.2f, 0f), true);
            bubble.transform.localScale = Vector3.one * 0.5f;
            bubble.gameObject.SetActive(false);

            var interact = go.AddComponent<Interactable>();
            interact.EditorInit("TALK", 1.9f, bubble.gameObject);
            Set(go.AddComponent<RoomEncounter>(), ("portrait", portraitName != null ? Spr(NpcPng, portraitName) : frames[0]));
            Save(go, "Encounters/" + name);
        }

        static RoomEncounter EncounterPrefab(string name) => Prefab("Encounters/" + name).GetComponent<RoomEncounter>();

        static void WireEncounters(PropSet set)
        {
            set.merchant = EncounterPrefab("Merchant");
            set.pilgrim = EncounterPrefab("Pilgrim");
            set.sunstone = EncounterPrefab("Sunstone");
            set.campfire = EncounterPrefab("Campfire");
            set.diceAltar = EncounterPrefab("DiceAltar");
            set.sunShard = EncounterPrefab("SunShard");
            set.weepingStatue = EncounterPrefab("WeepingStatue");
            set.cursedChest = EncounterPrefab("CursedChest");
        }

        // ---------------- floors ----------------

        /// <summary>Ten floors, one per biome. Deeper floors are wider, fight in bigger waves and hit harder.</summary>
        static void BuildFloors()
        {
            Floors.Clear();
            for (int n = 1; n <= BiomeDefs.Length; n++)
            {
                var floor = LoadOrCreate<FloorConfig>($"{DataDir}/Floors/Floor_{n:00}.asset");
                floor.EditorInit(Biomes[BiomeDefs[n - 1].id], PoolFor(n), EnemyPrefab(Oathdeep.Data.Bestiary.BossOf(n).Id));
                Set(floor, ("floorNumber", n), ("roomCount", 15 + (n + 1) / 2),
                    ("wavesPerRoom", new Vector2Int(2, n < 4 ? 3 : 4)),
                    ("enemiesPerWave", new Vector2Int(4 + n / 3, 7 + n / 3)),
                    ("eliteChance", 0.1f + 0.02f * n), ("statScale", 1f + 0.32f * (n - 1)));
                EditorUtility.SetDirty(floor);
                Floors.Add(floor);
            }
        }
    }
}
