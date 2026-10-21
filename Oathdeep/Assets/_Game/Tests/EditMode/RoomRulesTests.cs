using System.Collections.Generic;
using NUnit.Framework;
using Oathdeep.Dungeon;
using UnityEngine;

namespace Oathdeep.Tests
{
    public class RoomRulesTests
    {
        static readonly RectInt Interior = new RectInt(0, 0, 10, 8);

        // Interior floor except a 2x2 pillar at (4,4)
        static bool Floor(Vector2Int c) => Interior.Contains(c) && !(c.x >= 4 && c.x <= 5 && c.y >= 4 && c.y <= 5);

        [Test]
        public void EnemyOnInteriorFloor_IsNotStranded()
        {
            Assert.IsFalse(RoomRules.IsStranded(new Vector2(2.5f, 2.5f), Interior, Floor));
        }

        [Test]
        public void EnemyOutsideInterior_IsStranded()
        {
            Assert.IsTrue(RoomRules.IsStranded(new Vector2(-1.5f, 3.5f), Interior, Floor));
            Assert.IsTrue(RoomRules.IsStranded(new Vector2(3.5f, 9.2f), Interior, Floor));
        }

        [Test]
        public void EnemyInsidePillar_IsStranded()
        {
            Assert.IsTrue(RoomRules.IsStranded(new Vector2(4.5f, 4.5f), Interior, Floor));
        }

        [Test]
        public void NearestCell_PicksClosestCandidate()
        {
            var cells = new List<Vector2Int> { new Vector2Int(1, 1), new Vector2Int(8, 6), new Vector2Int(3, 5) };
            Assert.AreEqual(new Vector2Int(8, 6), RoomRules.NearestCell(new Vector2(11f, 7f), cells, Vector2Int.zero));
        }

        [Test]
        public void NearestCell_NoCandidates_ReturnsFallback()
        {
            var fallback = new Vector2Int(5, 4);
            Assert.AreEqual(fallback, RoomRules.NearestCell(new Vector2(11f, 7f), new List<Vector2Int>(), fallback));
        }
    }
}
