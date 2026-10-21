using System.Collections.Generic;
using NUnit.Framework;
using Oathdeep.Dungeon;
using UnityEngine;

namespace Oathdeep.Tests
{
    public class WallAutotileTests
    {
        static System.Func<Vector2Int, bool> FloorAt(params Vector2Int[] cells)
        {
            var set = new HashSet<Vector2Int>(cells);
            return c => set.Contains(c);
        }

        [Test]
        public void WallAboveFloor_IsFace()
        {
            var kind = WallAutotile.Classify(new Vector2Int(0, 1), FloorAt(Vector2Int.zero), out _);
            Assert.AreEqual(WallKind.Face, kind);
        }

        [Test]
        public void WallWithoutFloorBelow_IsCap()
        {
            var kind = WallAutotile.Classify(new Vector2Int(1, 0), FloorAt(Vector2Int.zero), out _);
            Assert.AreEqual(WallKind.Cap, kind);
        }

        [Test]
        public void CapMask_FlagsFloorSides()
        {
            // Cap at (0,0) with floor to the west and to the south-west only.
            WallAutotile.Classify(Vector2Int.zero, FloorAt(new Vector2Int(-1, 0)), out int mask);
            Assert.AreEqual(WallAutotile.West, mask);
        }

        [Test]
        public void CapAboveFace_FlagsSouthEdge()
        {
            // Floor at y=0, face at y=1, cap at y=2: the cap's south neighbour is a face → treat as open edge.
            WallAutotile.Classify(new Vector2Int(0, 2), FloorAt(Vector2Int.zero), out int mask);
            Assert.AreEqual(WallAutotile.South, mask & WallAutotile.South);
        }

        [Test]
        public void CapWithFloorAboveAndRight_HasBothBits()
        {
            WallAutotile.Classify(Vector2Int.zero, FloorAt(Vector2Int.up, Vector2Int.right), out int mask);
            Assert.AreEqual(WallAutotile.North | WallAutotile.East, mask);
        }
    }
}
