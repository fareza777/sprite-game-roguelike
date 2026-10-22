using System.Collections.Generic;
using UnityEngine;

namespace Oathdeep.Combat
{
    /// <summary>What standing in a hazard does: Slow scales speed, Ice cuts traction, Burn hurts, Root holds the feet.</summary>
    public enum HazardKind { Slow, Ice, Burn, Root }

    /// <summary>A hazard's footprint: a circle or an axis-aligned box, in world units.</summary>
    public readonly struct HazardShape
    {
        public readonly Vector2 Centre;
        public readonly float Radius;   // circles
        public readonly Rect Bounds;    // boxes
        public readonly bool IsBox;

        HazardShape(Vector2 centre, float radius, Rect bounds, bool isBox) { Centre = centre; Radius = radius; Bounds = bounds; IsBox = isBox; }

        public static HazardShape Circle(Vector2 centre, float radius) => new HazardShape(centre, radius, default, false);
        public static HazardShape Box(Rect box) => new HazardShape(box.center, 0f, box, true);

        public bool Contains(Vector2 p) => IsBox ? Bounds.Contains(p) : (p - Centre).sqrMagnitude <= Radius * Radius;
    }

    /// <summary>How the ground under the hero changes movement. Overlapping hazards keep the strongest of each kind.</summary>
    public readonly struct MoveModifier
    {
        public readonly float SpeedMultiplier;
        public readonly float Traction;   // 1 = normal grip, small = sliding on ice

        public MoveModifier(float speed, float traction) { SpeedMultiplier = speed; Traction = traction; }

        public static readonly MoveModifier None = new MoveModifier(1f, 1f);

        public static MoveModifier Combine(IEnumerable<(HazardKind kind, float strength)> hazards)
        {
            float speed = 1f, traction = 1f;
            foreach (var (kind, strength) in hazards)
            {
                if (kind == HazardKind.Slow) speed = Mathf.Min(speed, strength);
                else if (kind == HazardKind.Ice) traction = Mathf.Min(traction, strength);
            }
            return new MoveModifier(speed, traction);
        }
    }

    /// <summary>A rotating line attack (the Horologist's clock hand): a segment from a pivot, of some length and width.</summary>
    public static class Beam
    {
        public static bool Hits(Vector2 pivot, float angleDegrees, float length, float halfWidth, Vector2 point)
        {
            float a = angleDegrees * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            var to = point - pivot;
            float along = Vector2.Dot(to, dir);
            if (along < 0f || along > length) return false;
            float across = Mathf.Abs(to.x * dir.y - to.y * dir.x);
            return across <= halfWidth;
        }
    }
}
