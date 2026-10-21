using System.Collections.Generic;
using UnityEngine;

namespace Oathdeep.Dungeon
{
    public readonly struct DoorSpan
    {
        public readonly Vector2Int Direction;
        public readonly RectInt Cells;

        public DoorSpan(Vector2Int direction, RectInt cells)
        {
            Direction = direction;
            Cells = cells;
        }
    }

    public enum RoomShape { Chamber, Hall, GreatHall, Gallery, Nave, Vault, Arena }

    public sealed class RoomGeometry
    {
        public RoomNode Node { get; }
        public RoomShape Shape { get; }
        public RectInt Interior { get; }
        public Vector2Int Centre => new Vector2Int(Interior.x + Interior.width / 2, Interior.y + Interior.height / 2);
        public List<DoorSpan> Doors { get; } = new List<DoorSpan>();
        public List<Vector2Int> SpawnCells { get; } = new List<Vector2Int>();
        public List<Vector2Int> Pillars { get; } = new List<Vector2Int>();

        public RoomGeometry(RoomNode node, RoomShape shape, RectInt interior)
        {
            Node = node;
            Shape = shape;
            Interior = interior;
        }
    }

    /// <summary>
    /// Turns a room graph into a tile grid: rooms of varied archetypes centred on a coarse grid,
    /// 3-wide corridors along the centre lines, door spans, procedural pillar layouts and
    /// walls 2 cells thick around every open cell.
    /// </summary>
    public sealed class DungeonGeometry
    {
        public const int StrideX = 40;
        public const int StrideY = 38;
        const int CorridorHalf = 1;
        const int WallThickness = 2;
        const int LaneHalf = 2;      // keep a 5-wide cross through each room centre clear of pillars
        const int PillarMargin = 2;  // keep pillars this far from interior walls

        static readonly Dictionary<RoomShape, Vector2Int> Sizes = new Dictionary<RoomShape, Vector2Int>
        {
            { RoomShape.Chamber, new Vector2Int(15, 15) },
            { RoomShape.Hall, new Vector2Int(21, 17) },
            { RoomShape.GreatHall, new Vector2Int(27, 23) },
            { RoomShape.Gallery, new Vector2Int(31, 13) },
            { RoomShape.Nave, new Vector2Int(15, 27) },
            { RoomShape.Vault, new Vector2Int(13, 13) },
            { RoomShape.Arena, new Vector2Int(33, 29) },
        };

        static readonly RoomShape[] CombatShapes = { RoomShape.Chamber, RoomShape.Hall, RoomShape.Hall, RoomShape.GreatHall, RoomShape.Gallery, RoomShape.Nave };

        readonly HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        readonly HashSet<Vector2Int> walls = new HashSet<Vector2Int>();

        public List<RoomGeometry> Rooms { get; } = new List<RoomGeometry>();
        public IEnumerable<Vector2Int> FloorCells => floor;
        public IEnumerable<Vector2Int> WallCells => walls;

        public bool IsFloor(Vector2Int c) => floor.Contains(c);
        public bool IsWall(Vector2Int c) => walls.Contains(c);

        public static Vector2Int CellCentre(Vector2Int grid) => new Vector2Int(grid.x * StrideX, grid.y * StrideY);

        public static DungeonGeometry Build(DungeonLayout layout, int seed)
        {
            var geo = new DungeonGeometry();
            var rng = new System.Random(seed ^ 0x5f3759df);

            foreach (var node in layout.Rooms)
            {
                var shape = PickShape(node.Type, rng);
                var size = Sizes[shape];
                var c = CellCentre(node.GridPosition);
                var room = new RoomGeometry(node, shape, new RectInt(c.x - size.x / 2, c.y - size.y / 2, size.x, size.y));
                geo.Rooms.Add(room);
                geo.Carve(room.Interior);
            }

            var byCell = new Dictionary<Vector2Int, RoomGeometry>();
            foreach (var r in geo.Rooms) byCell[r.Node.GridPosition] = r;
            foreach (var room in geo.Rooms) geo.CarveCorridorsAndDoors(room, byCell);

            foreach (var room in geo.Rooms) geo.PlacePillars(room, rng);

            geo.BuildWalls();
            foreach (var room in geo.Rooms) geo.CollectSpawnCells(room);
            return geo;
        }

        static RoomShape PickShape(RoomType type, System.Random rng) => type switch
        {
            RoomType.Start => RoomShape.Chamber,
            RoomType.Treasure => RoomShape.Vault,
            RoomType.Boss => RoomShape.Arena,
            RoomType.Elite => rng.NextDouble() < 0.5 ? RoomShape.GreatHall : RoomShape.Gallery,
            RoomType.Merchant or RoomType.Pilgrim or RoomType.Rest => RoomShape.Chamber,
            RoomType.Sunstone or RoomType.Event => RoomShape.Hall,
            _ => CombatShapes[rng.Next(CombatShapes.Length)],
        };

        void Carve(RectInt r)
        {
            for (int x = r.xMin; x < r.xMax; x++)
                for (int y = r.yMin; y < r.yMax; y++)
                    floor.Add(new Vector2Int(x, y));
        }

        void CarveCorridorsAndDoors(RoomGeometry room, Dictionary<Vector2Int, RoomGeometry> byCell)
        {
            var r = room.Interior;
            var c = CellCentre(room.Node.GridPosition);

            foreach (var otherCell in room.Node.Connections)
            {
                var dir = otherCell - room.Node.GridPosition;
                var other = byCell[otherCell].Interior;
                int span = CorridorHalf * 2 + 1;

                if (dir == Vector2Int.right)
                {
                    Carve(new RectInt(r.xMax, c.y - CorridorHalf, other.xMin - r.xMax, span));
                    room.Doors.Add(new DoorSpan(dir, new RectInt(r.xMax, c.y - CorridorHalf, 1, span)));
                }
                else if (dir == Vector2Int.left)
                {
                    room.Doors.Add(new DoorSpan(dir, new RectInt(r.xMin - 1, c.y - CorridorHalf, 1, span)));
                }
                else if (dir == Vector2Int.up)
                {
                    Carve(new RectInt(c.x - CorridorHalf, r.yMax, span, other.yMin - r.yMax));
                    room.Doors.Add(new DoorSpan(dir, new RectInt(c.x - CorridorHalf, r.yMax, span, 1)));
                }
                else if (dir == Vector2Int.down)
                {
                    room.Doors.Add(new DoorSpan(dir, new RectInt(c.x - CorridorHalf, r.yMin - 1, span, 1)));
                }
            }
        }

        /// <summary>2×2 pillars on a lattice, never in the centre cross lanes or against the walls.</summary>
        void PlacePillars(RoomGeometry room, System.Random rng)
        {
            var type = room.Node.Type;
            if (type == RoomType.Start || type == RoomType.Treasure || RoomTypes.IsEncounter(type)) return;

            var r = room.Interior;
            var centre = room.Centre;
            int pattern = type == RoomType.Boss ? 1 : rng.Next(5); // 0 none, 1 corners, 2 lattice, 3 colonnade, 4 scattered
            if (pattern == 0) return;

            var candidates = new List<Vector2Int>();
            int stepX = pattern == 2 ? 4 : pattern == 3 ? 5 : 6;
            int stepY = pattern == 2 ? 4 : pattern == 3 ? r.height : 6;
            for (int x = r.xMin + PillarMargin; x + 1 < r.xMax - PillarMargin; x += stepX)
                for (int y = r.yMin + PillarMargin; y + 1 < r.yMax - PillarMargin; y += Mathf.Max(3, stepY))
                    candidates.Add(new Vector2Int(x, y));

            if (pattern == 3)
            {
                // Two rows of columns flanking the long axis.
                candidates.Clear();
                for (int x = r.xMin + PillarMargin; x + 1 < r.xMax - PillarMargin; x += stepX)
                {
                    candidates.Add(new Vector2Int(x, r.yMin + PillarMargin));
                    candidates.Add(new Vector2Int(x, r.yMax - PillarMargin - 2));
                }
            }
            if (pattern == 1)
            {
                candidates.Clear();
                candidates.Add(new Vector2Int(r.xMin + PillarMargin + 1, r.yMin + PillarMargin + 1));
                candidates.Add(new Vector2Int(r.xMax - PillarMargin - 3, r.yMin + PillarMargin + 1));
                candidates.Add(new Vector2Int(r.xMin + PillarMargin + 1, r.yMax - PillarMargin - 3));
                candidates.Add(new Vector2Int(r.xMax - PillarMargin - 3, r.yMax - PillarMargin - 3));
            }

            foreach (var p in candidates)
            {
                if (pattern == 4 && rng.NextDouble() < 0.55) continue;
                bool inLaneX = p.x + 1 >= centre.x - LaneHalf && p.x <= centre.x + LaneHalf;
                bool inLaneY = p.y + 1 >= centre.y - LaneHalf && p.y <= centre.y + LaneHalf;
                if (inLaneX || inLaneY) continue;
                if (p.x < r.xMin + PillarMargin || p.x + 1 >= r.xMax - PillarMargin) continue;
                if (p.y < r.yMin + PillarMargin || p.y + 1 >= r.yMax - PillarMargin) continue;

                room.Pillars.Add(p);
                for (int dx = 0; dx < 2; dx++)
                    for (int dy = 0; dy < 2; dy++)
                        floor.Remove(new Vector2Int(p.x + dx, p.y + dy));
            }
        }

        void BuildWalls()
        {
            foreach (var c in floor)
                for (int dx = -WallThickness; dx <= WallThickness; dx++)
                    for (int dy = -WallThickness; dy <= WallThickness; dy++)
                    {
                        var n = new Vector2Int(c.x + dx, c.y + dy);
                        if (!floor.Contains(n)) walls.Add(n);
                    }
        }

        void CollectSpawnCells(RoomGeometry room)
        {
            var r = room.Interior;
            for (int x = r.xMin + 1; x < r.xMax - 1; x++)
                for (int y = r.yMin + 1; y < r.yMax - 1; y++)
                {
                    bool clear = true;
                    for (int dx = -1; dx <= 1 && clear; dx++)
                        for (int dy = -1; dy <= 1 && clear; dy++)
                            if (!floor.Contains(new Vector2Int(x + dx, y + dy))) clear = false;
                    if (clear) room.SpawnCells.Add(new Vector2Int(x, y));
                }
        }
    }
}
