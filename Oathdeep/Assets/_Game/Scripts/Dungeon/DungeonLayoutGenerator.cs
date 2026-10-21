using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oathdeep.Dungeon
{
    /// <summary>
    /// Seeded room-graph generator. Grows a spanning tree on a grid (biased toward long branches),
    /// then places the boss on the furthest dead end and treasure on another dead end.
    /// </summary>
    public static class DungeonLayoutGenerator
    {
        public const int MinRooms = 3;
        const float GrowFromNewestChance = 0.6f;
        const int MinBossDistance = 3;
        const int MaxAttempts = 64;

        static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        public static DungeonLayout Generate(int seed, int roomCount)
        {
            if (roomCount < MinRooms) throw new ArgumentOutOfRangeException(nameof(roomCount), $"Need at least {MinRooms} rooms.");

            var rng = new System.Random(seed);
            int wantedDistance = Mathf.Min(MinBossDistance, roomCount - 1);
            List<RoomNode> rooms = null;

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                rooms = GrowTree(rng, roomCount);
                AssignDistances(rooms);
                if (rooms.Max(r => r.DistanceFromStart) >= wantedDistance) break;
            }

            AssignRoomTypes(rng, rooms);
            return new DungeonLayout(rooms);
        }

        static List<RoomNode> GrowTree(System.Random rng, int roomCount)
        {
            var rooms = new List<RoomNode> { new RoomNode(Vector2Int.zero, RoomType.Start) };
            var occupied = new HashSet<Vector2Int> { Vector2Int.zero };

            while (rooms.Count < roomCount)
            {
                var from = rng.NextDouble() < GrowFromNewestChance ? rooms[rooms.Count - 1] : rooms[rng.Next(rooms.Count)];
                var freeDirs = Directions.Where(d => !occupied.Contains(from.GridPosition + d)).ToList();
                if (freeDirs.Count == 0) continue;

                var cell = from.GridPosition + freeDirs[rng.Next(freeDirs.Count)];
                var room = new RoomNode(cell, RoomType.Combat);
                room.Connect(from.GridPosition);
                from.Connect(cell);
                rooms.Add(room);
                occupied.Add(cell);
            }
            return rooms;
        }

        static void AssignDistances(List<RoomNode> rooms)
        {
            var byCell = rooms.ToDictionary(r => r.GridPosition);
            var queue = new Queue<RoomNode>();
            var seen = new HashSet<Vector2Int> { rooms[0].GridPosition };
            rooms[0].DistanceFromStart = 0;
            queue.Enqueue(rooms[0]);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                foreach (var cell in cur.Connections)
                {
                    if (!seen.Add(cell)) continue;
                    byCell[cell].DistanceFromStart = cur.DistanceFromStart + 1;
                    queue.Enqueue(byCell[cell]);
                }
            }
        }

        static void AssignRoomTypes(System.Random rng, List<RoomNode> rooms)
        {
            // In a tree the furthest node from the root is always a leaf.
            int maxDistance = rooms.Max(r => r.DistanceFromStart);
            var bossCandidates = rooms.Where(r => r.DistanceFromStart == maxDistance).ToList();
            var boss = bossCandidates[rng.Next(bossCandidates.Count)];
            boss.Type = RoomType.Boss;

            var leaves = rooms.Where(r => r.Type == RoomType.Combat && r.Connections.Count == 1).ToList();
            var pool = leaves.Count > 0 ? leaves : rooms.Where(r => r.Type == RoomType.Combat).ToList();
            pool[rng.Next(pool.Count)].Type = RoomType.Treasure;

            AssignEncounters(rng, rooms, boss);
        }

        /// <summary>
        /// Sprinkles the peaceful rooms and elites over the remaining combat rooms. The room guarding the boss stays
        /// a fight; a campfire sits close to the boss; at least half the floor keeps fighting.
        /// </summary>
        static void AssignEncounters(System.Random rng, List<RoomNode> rooms, RoomNode boss)
        {
            var byCell = rooms.ToDictionary(r => r.GridPosition);
            var guard = byCell[boss.Connections[0]];
            List<RoomNode> Free() => rooms.Where(r => r.Type == RoomType.Combat && r != guard).ToList();
            int budget = Mathf.Max(0, (rooms.Count - 4) / 2);   // encounters + elites never exceed half the floor

            bool Take(RoomType type, Func<RoomNode, double> preference)
            {
                if (budget <= 0) return false;
                var free = Free();
                if (free.Count == 0) return false;
                var best = free.OrderByDescending(r => preference(r) + rng.NextDouble() * 0.5).First();
                best.Type = type;
                budget--;
                return true;
            }

            int max = rooms.Max(r => r.DistanceFromStart);
            Take(RoomType.Rest, r => -Math.Abs(r.DistanceFromStart - (max - 1)));       // rest before the boss
            Take(RoomType.Merchant, r => -Math.Abs(r.DistanceFromStart - max / 2.0));   // shop mid-floor
            Take(RoomType.Sunstone, r => r.Connections.Count == 1 ? 1 : 0);              // sunstones hide in dead ends
            Take(RoomType.Pilgrim, r => r.DistanceFromStart <= 2 ? 1 : 0);              // pilgrims near the entrance
            Take(RoomType.Elite, r => r.DistanceFromStart);
            Take(RoomType.Event, r => r.Connections.Count == 1 ? 1 : 0);
            if (rooms.Count >= 14) Take(RoomType.Elite, r => r.DistanceFromStart);
            if (rooms.Count >= 16) Take(RoomType.Event, r => 0);
        }
    }
}
