using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Oathdeep.Dungeon;
using UnityEngine;

namespace Oathdeep.Tests
{
    public class DungeonLayoutGeneratorTests
    {
        static IEnumerable<int> Seeds => Enumerable.Range(0, 200);

        [Test]
        public void ProducesRequestedRoomCount([ValueSource(nameof(Seeds))] int seed)
        {
            var layout = DungeonLayoutGenerator.Generate(seed, 7);
            Assert.AreEqual(7, layout.Rooms.Count);
        }

        [Test]
        public void HasExactlyOneStartBossAndTreasure([ValueSource(nameof(Seeds))] int seed)
        {
            var layout = DungeonLayoutGenerator.Generate(seed, 7);
            Assert.AreEqual(1, layout.Rooms.Count(r => r.Type == RoomType.Start));
            Assert.AreEqual(1, layout.Rooms.Count(r => r.Type == RoomType.Boss));
            Assert.AreEqual(1, layout.Rooms.Count(r => r.Type == RoomType.Treasure));
            Assert.AreEqual(RoomType.Start, layout.Rooms[0].Type);
        }

        [Test]
        public void BossRoomIsLeafAndFurthestFromStart([ValueSource(nameof(Seeds))] int seed)
        {
            var layout = DungeonLayoutGenerator.Generate(seed, 7);
            var distances = Bfs(layout);
            var boss = layout.Rooms.First(r => r.Type == RoomType.Boss);

            Assert.AreEqual(1, boss.Connections.Count, "Boss room must be a dead end");
            Assert.AreEqual(distances.Values.Max(), distances[boss.GridPosition]);
        }

        [Test]
        public void AllRoomsReachableAndCellsUnique([ValueSource(nameof(Seeds))] int seed)
        {
            var layout = DungeonLayoutGenerator.Generate(seed, 7);
            Assert.AreEqual(layout.Rooms.Count, layout.Rooms.Select(r => r.GridPosition).Distinct().Count());
            Assert.AreEqual(layout.Rooms.Count, Bfs(layout).Count);
        }

        [Test]
        public void ConnectionsAreSymmetricAndAdjacent([ValueSource(nameof(Seeds))] int seed)
        {
            var layout = DungeonLayoutGenerator.Generate(seed, 7);
            foreach (var room in layout.Rooms)
            {
                foreach (var other in room.Connections)
                {
                    var delta = other - room.GridPosition;
                    Assert.AreEqual(1, Mathf.Abs(delta.x) + Mathf.Abs(delta.y));
                    Assert.IsTrue(layout.Get(other).Connections.Contains(room.GridPosition));
                }
            }
        }

        [Test]
        public void SameSeed_GivesSameLayout()
        {
            var a = DungeonLayoutGenerator.Generate(1234, 7);
            var b = DungeonLayoutGenerator.Generate(1234, 7);
            CollectionAssert.AreEqual(a.Rooms.Select(r => (r.GridPosition, r.Type)), b.Rooms.Select(r => (r.GridPosition, r.Type)));
        }

        [Test]
        public void LargeFloors_HaveEverySpecialRoomOnce([ValueSource(nameof(Seeds))] int seed)
        {
            var layout = DungeonLayoutGenerator.Generate(seed, 18);
            foreach (var type in new[] { RoomType.Merchant, RoomType.Sunstone, RoomType.Pilgrim, RoomType.Rest })
                Assert.AreEqual(1, layout.Rooms.Count(r => r.Type == type), type.ToString());
            Assert.GreaterOrEqual(layout.Rooms.Count(r => r.Type == RoomType.Event), 1);
            Assert.GreaterOrEqual(layout.Rooms.Count(r => r.Type == RoomType.Elite), 1);
            Assert.GreaterOrEqual(layout.Rooms.Count(r => r.Type == RoomType.Combat), 6, "most rooms still fight");
        }

        [Test]
        public void RoomNextToBoss_IsNeverASpecialRoom_AndBossStaysLeaf([ValueSource(nameof(Seeds))] int seed)
        {
            var layout = DungeonLayoutGenerator.Generate(seed, 18);
            var boss = layout.Rooms.Single(r => r.Type == RoomType.Boss);
            Assert.AreEqual(1, boss.Connections.Count);
            Assert.AreEqual(RoomType.Start, layout.Rooms[0].Type);
        }

        [Test]
        public void SmallFloors_StillGenerate([Values(3, 4, 5, 6)] int count)
        {
            var layout = DungeonLayoutGenerator.Generate(5, count);
            Assert.AreEqual(count, layout.Rooms.Count);
            Assert.AreEqual(1, layout.Rooms.Count(r => r.Type == RoomType.Boss));
        }

        [Test]
        public void TooFewRooms_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => DungeonLayoutGenerator.Generate(1, 2));
        }

        static Dictionary<Vector2Int, int> Bfs(DungeonLayout layout)
        {
            var dist = new Dictionary<Vector2Int, int> { [layout.Rooms[0].GridPosition] = 0 };
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(layout.Rooms[0].GridPosition);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                foreach (var n in layout.Get(cur).Connections)
                {
                    if (dist.ContainsKey(n)) continue;
                    dist[n] = dist[cur] + 1;
                    queue.Enqueue(n);
                }
            }
            return dist;
        }
    }
}
