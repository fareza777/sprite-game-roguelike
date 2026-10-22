using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Oathdeep.Dungeon;
using UnityEngine;

namespace Oathdeep.Tests
{
    public class DungeonGeometryTests
    {
        static IEnumerable<int> Seeds => Enumerable.Range(0, 60);
        const int Rooms = 14;

        static DungeonGeometry Build(int seed) => DungeonGeometry.Build(DungeonLayoutGenerator.Generate(seed, Rooms), seed);

        static RoomGeometry BossRoom(DungeonGeometry geo) => geo.Rooms.First(r => r.Node.Type == RoomType.Boss);

        [Test]
        public void BossArenas_StayWalkable_OnEveryFloor([Values(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)] int floor, [Values(3, 7, 11)] int seed)
        {
            var geo = DungeonGeometry.Build(DungeonLayoutGenerator.Generate(seed, Rooms), seed, floor);
            var boss = BossRoom(geo);
            Assert.IsTrue(geo.IsFloor(boss.Centre));
            Assert.IsTrue(geo.IsFloor(boss.Centre + Vector2Int.up * 3), "the boss rises 3 cells above the centre");
            Assert.IsTrue(FloodFill(geo, geo.Rooms[0].Centre).Contains(boss.Centre));
        }

        [Test]
        public void BossArenas_AreLaidOutDifferentlyAcrossTheDescent()
        {
            var layouts = Enumerable.Range(1, 10)
                .Select(f => string.Join(";", BossRoom(DungeonGeometry.Build(DungeonLayoutGenerator.Generate(7, Rooms), 7, f)).Pillars
                    .Select(p => p - BossRoom(DungeonGeometry.Build(DungeonLayoutGenerator.Generate(7, Rooms), 7, f)).Interior.min)))
                .Distinct().Count();
            Assert.GreaterOrEqual(layouts, 6);
        }

        [Test]
        public void EveryRoomCentreIsFloor([ValueSource(nameof(Seeds))] int seed)
        {
            var geo = Build(seed);
            foreach (var room in geo.Rooms)
                Assert.IsTrue(geo.IsFloor(room.Centre), $"room centre {room.Centre} should be floor");
        }

        [Test]
        public void AllRoomsReachableByWalking([ValueSource(nameof(Seeds))] int seed)
        {
            var geo = Build(seed);
            var reached = FloodFill(geo, geo.Rooms[0].Centre);
            foreach (var room in geo.Rooms)
                Assert.IsTrue(reached.Contains(room.Centre), "every room must be reachable");
        }

        [Test]
        public void DoorsMatchConnections([ValueSource(nameof(Seeds))] int seed)
        {
            var layout = DungeonLayoutGenerator.Generate(seed, Rooms);
            var geo = DungeonGeometry.Build(layout, seed);
            for (int i = 0; i < layout.Rooms.Count; i++)
                Assert.AreEqual(layout.Rooms[i].Connections.Count, geo.Rooms[i].Doors.Count);
        }

        [Test]
        public void FloorCellsAreSurroundedByWalls([ValueSource(nameof(Seeds))] int seed)
        {
            var geo = Build(seed);
            foreach (var cell in geo.FloorCells)
                foreach (var d in Dirs)
                {
                    var n = cell + d;
                    Assert.IsTrue(geo.IsFloor(n) || geo.IsWall(n), $"floor {cell} leaks into void at {n}");
                }
        }

        [Test]
        public void NorthWallsAreTwoTilesThick([ValueSource(nameof(Seeds))] int seed)
        {
            var geo = Build(seed);
            foreach (var cell in geo.FloorCells)
            {
                var above = cell + Vector2Int.up;
                if (geo.IsWall(above)) Assert.IsTrue(geo.IsWall(above + Vector2Int.up), "wall face needs a cap above it");
            }
        }

        [Test]
        public void RoomsNeverOverlap([ValueSource(nameof(Seeds))] int seed)
        {
            var geo = Build(seed);
            for (int i = 0; i < geo.Rooms.Count; i++)
                for (int j = i + 1; j < geo.Rooms.Count; j++)
                {
                    var a = geo.Rooms[i].Interior; var b = geo.Rooms[j].Interior;
                    var grownA = new RectInt(a.x - 3, a.y - 3, a.width + 6, a.height + 6);
                    Assert.IsFalse(grownA.Overlaps(b), $"rooms {i} and {j} are too close");
                }
        }

        [Test]
        public void FloorIsLargeAndVaried([ValueSource(nameof(Seeds))] int seed)
        {
            var geo = Build(seed);
            Assert.Greater(geo.FloorCells.Count(), 2500, "a floor should feel massive");
            Assert.Greater(geo.Rooms.Select(r => r.Interior.size).Distinct().Count(), 2, "rooms should vary in size");
        }

        [Test]
        public void BossRoomIsTheLargestArena([ValueSource(nameof(Seeds))] int seed)
        {
            var geo = Build(seed);
            var boss = geo.Rooms.First(r => r.Node.Type == RoomType.Boss);
            int bossArea = boss.Interior.width * boss.Interior.height;
            Assert.IsTrue(geo.Rooms.All(r => r.Interior.width * r.Interior.height <= bossArea));
        }

        [Test]
        public void CombatRoomsHaveEnoughSpawnCells([ValueSource(nameof(Seeds))] int seed)
        {
            var geo = Build(seed);
            foreach (var r in geo.Rooms.Where(r => r.Node.Type == RoomType.Combat))
                Assert.GreaterOrEqual(r.SpawnCells.Count, 30);
        }

        static readonly Vector2Int[] Dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        static HashSet<Vector2Int> FloodFill(DungeonGeometry geo, Vector2Int start)
        {
            var seen = new HashSet<Vector2Int> { start };
            var q = new Queue<Vector2Int>();
            q.Enqueue(start);
            while (q.Count > 0)
            {
                var c = q.Dequeue();
                foreach (var d in Dirs)
                {
                    var n = c + d;
                    if (geo.IsFloor(n) && seen.Add(n)) q.Enqueue(n);
                }
            }
            return seen;
        }
    }
}
