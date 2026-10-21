using System.Collections.Generic;
using UnityEngine;

namespace Oathdeep.Dungeon
{
    /// <summary>Combat rooms lock until cleared; the special rooms (merchant to event) are peaceful encounters.</summary>
    public enum RoomType { Start, Combat, Elite, Treasure, Merchant, Shrine, Pilgrim, Rest, Event, Boss }

    public static class RoomTypes
    {
        public static bool IsFight(RoomType t) => t == RoomType.Combat || t == RoomType.Elite || t == RoomType.Boss;
        public static bool IsEncounter(RoomType t) => t >= RoomType.Merchant && t <= RoomType.Event;
    }

    public sealed class RoomNode
    {
        public Vector2Int GridPosition { get; }
        public RoomType Type { get; internal set; }
        public IReadOnlyList<Vector2Int> Connections => connections;
        public int DistanceFromStart { get; internal set; }

        readonly List<Vector2Int> connections = new List<Vector2Int>();

        public RoomNode(Vector2Int gridPosition, RoomType type)
        {
            GridPosition = gridPosition;
            Type = type;
        }

        internal void Connect(Vector2Int other)
        {
            if (!connections.Contains(other)) connections.Add(other);
        }

        public bool HasDoor(Vector2Int direction) => connections.Contains(GridPosition + direction);
    }

    public sealed class DungeonLayout
    {
        public IReadOnlyList<RoomNode> Rooms => rooms;
        readonly List<RoomNode> rooms;
        readonly Dictionary<Vector2Int, RoomNode> byCell;

        public DungeonLayout(List<RoomNode> rooms)
        {
            this.rooms = rooms;
            byCell = new Dictionary<Vector2Int, RoomNode>();
            foreach (var r in rooms) byCell[r.GridPosition] = r;
        }

        public RoomNode Get(Vector2Int cell) => byCell[cell];
        public bool TryGet(Vector2Int cell, out RoomNode room) => byCell.TryGetValue(cell, out room);
    }
}
