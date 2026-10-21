using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oathdeep.Dungeon
{
    /// <summary>Pure placement rules shared by rooms: is an enemy somewhere the player can reach, and where to put it back.</summary>
    public static class RoomRules
    {
        /// <summary>True when the position is outside the room interior or on a non-floor cell (pillar, wall).</summary>
        public static bool IsStranded(Vector2 position, RectInt interior, Func<Vector2Int, bool> isFloor)
        {
            var cell = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
            return !interior.Contains(cell) || !isFloor(cell);
        }

        public static Vector2Int NearestCell(Vector2 from, IReadOnlyList<Vector2Int> cells, Vector2Int fallback)
        {
            var best = fallback;
            float bestDist = float.MaxValue;
            foreach (var c in cells)
            {
                float d = (new Vector2(c.x + 0.5f, c.y + 0.5f) - from).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = c; }
            }
            return best;
        }
    }
}
