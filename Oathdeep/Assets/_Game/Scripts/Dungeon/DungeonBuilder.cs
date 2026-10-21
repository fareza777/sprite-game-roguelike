using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Oathdeep.Dungeon
{
    /// <summary>
    /// Instantiates a generated floor: paints floor / decal / wall tilemaps, builds wall collision,
    /// spawns doors and room controllers, then hands the rooms to the decorator.
    /// </summary>
    public sealed class DungeonBuilder : MonoBehaviour
    {
        [SerializeField] PropSet props;
        [SerializeField] string sortingLayer = "Default";

        Grid grid;
        Tilemap floorMap;
        Tilemap shadowMap;
        Tilemap decalMap;
        Tilemap wallMap;

        public DungeonGeometry Geometry { get; private set; }
        public List<RoomController> Rooms { get; } = new List<RoomController>();
        public Vector3 StartPosition { get; private set; }

        public void SetProps(PropSet set) => props = set;

        public void Build(FloorConfig config, int seed, Transform player, System.Action<Vector3> onExit)
        {
            Clear();
            var layout = DungeonLayoutGenerator.Generate(seed, config.RoomCount);
            Geometry = DungeonGeometry.Build(layout, seed);
            var rng = new System.Random(seed * 7919 + 13);

            CreateTilemaps();
            Paint(config.Biome, rng);

            var decorator = new DungeonDecorator(props, decalMap, transform, rng);
            foreach (var room in Geometry.Rooms)
            {
                var doors = SpawnDoors(room);
                var go = new GameObject($"Room {room.Node.GridPosition} {room.Node.Type}");
                go.transform.SetParent(transform, false);
                var controller = go.AddComponent<RoomController>();
                controller.Init(room, Geometry.IsFloor, config, rng, player, doors, props != null ? props.spawnMarker : null, onExit);
                Rooms.Add(controller);
                decorator.Decorate(room, Geometry, config, controller);
            }

            var start = Geometry.Rooms[0];
            StartPosition = new Vector3(start.Centre.x + 0.5f, start.Centre.y + 0.5f, 0f);
        }

        public void Clear()
        {
            Rooms.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
        }

        void CreateTilemaps()
        {
            var gridGo = new GameObject("Grid");
            gridGo.transform.SetParent(transform, false);
            grid = gridGo.AddComponent<Grid>();

            floorMap = CreateMap("Floor", -100, false);
            shadowMap = CreateMap("Shadows", -95, false);
            decalMap = CreateMap("Decals", -90, false);
            wallMap = CreateMap("Walls", 0, true);

            var body = wallMap.gameObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            var tileCollider = wallMap.gameObject.AddComponent<TilemapCollider2D>();
            tileCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            wallMap.gameObject.AddComponent<CompositeCollider2D>();
        }

        Tilemap CreateMap(string name, int order, bool ySorted)
        {
            var go = new GameObject(name);
            go.transform.SetParent(grid.transform, false);
            var map = go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = order;
            // Walls sort per tile by Y so characters walk behind pillars and wall faces correctly.
            renderer.mode = ySorted ? TilemapRenderer.Mode.Individual : TilemapRenderer.Mode.Chunk;
            return map;
        }

        void Paint(BiomeTiles biome, System.Random rng)
        {
            var floorCells = new List<Vector3Int>();
            var floorTiles = new List<TileBase>();
            foreach (var c in Geometry.FloorCells)
            {
                floorCells.Add(new Vector3Int(c.x, c.y, 0));
                floorTiles.Add(biome.Floor(rng));
            }
            floorMap.SetTiles(floorCells.ToArray(), floorTiles.ToArray());

            // Contact shadows sell wall height: under every wall face and right of every west wall.
            foreach (var c in Geometry.FloorCells)
            {
                var shadow = biome.Shadow(Geometry.IsWall(c + Vector2Int.up), Geometry.IsWall(c + Vector2Int.left));
                if (shadow != null) shadowMap.SetTile(new Vector3Int(c.x, c.y, 0), shadow);
            }

            var wallCells = new List<Vector3Int>();
            var wallTiles = new List<TileBase>();
            foreach (var c in Geometry.WallCells)
            {
                var kind = WallAutotile.Classify(c, Geometry.IsFloor, out int mask);
                var tile = kind == WallKind.Face ? biome.WallFace(rng) : biome.WallCap(mask);
                if (tile == null) continue;
                wallCells.Add(new Vector3Int(c.x, c.y, 0));
                wallTiles.Add(tile);
            }
            wallMap.SetTiles(wallCells.ToArray(), wallTiles.ToArray());
        }

        List<Door> SpawnDoors(RoomGeometry room)
        {
            var list = new List<Door>();
            if (props == null || props.door == null) return list;
            foreach (var span in room.Doors)
            {
                var r = span.Cells;
                var centre = new Vector3(r.x + r.width * 0.5f, r.y + r.height * 0.5f, 0f);
                var door = Instantiate(props.door, centre, Quaternion.identity, transform);
                bool vertical = span.Direction.x != 0;
                door.Setup(null, null, new Vector2(r.width, r.height), vertical);
                list.Add(door);
            }
            return list;
        }
    }
}
