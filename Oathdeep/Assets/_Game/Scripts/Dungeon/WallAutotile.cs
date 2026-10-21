using System;
using UnityEngine;

namespace Oathdeep.Dungeon
{
    public enum WallKind { Face, Cap }

    /// <summary>
    /// 3/4 top-down wall classification. A wall directly above floor shows its brick face;
    /// every other wall shows a dark cap whose edges (mask bits) light up where it borders open space.
    /// </summary>
    public static class WallAutotile
    {
        public const int North = 1, East = 2, South = 4, West = 8;

        public static WallKind Classify(Vector2Int cell, Func<Vector2Int, bool> isFloor, out int capMask)
        {
            capMask = 0;
            var below = cell + Vector2Int.down;
            if (isFloor(below)) return WallKind.Face;

            if (isFloor(cell + Vector2Int.up)) capMask |= North;
            if (isFloor(cell + Vector2Int.right)) capMask |= East;
            if (isFloor(cell + Vector2Int.left)) capMask |= West;
            // The cap sitting on top of a face is the wall's visible lip.
            if (isFloor(below + Vector2Int.down)) capMask |= South;
            return WallKind.Cap;
        }
    }
}
