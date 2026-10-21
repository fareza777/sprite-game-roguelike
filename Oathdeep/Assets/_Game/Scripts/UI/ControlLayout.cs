using UnityEngine;

namespace Oathdeep.UI
{
    /// <summary>
    /// Thumb-friendly touch layout in reference-canvas units (360x640). Action buttons are positioned from the
    /// bottom corner on the action side (x mirrored for left-handed play); the move zone covers the other half.
    /// </summary>
    public readonly struct ControlLayout
    {
        public readonly struct Slot
        {
            public readonly Vector2 Position;   // offset from its bottom anchor
            public readonly float Size;
            public readonly float AnchorX;      // where along the bottom edge it hangs: 0 left, 0.5 centre, 1 right
            public Slot(Vector2 position, float size, float anchorX) { Position = position; Size = size; AnchorX = anchorX; }
        }

        public const float EdgeMargin = 14f;        // floating stick never hugs the screen edge
        public static readonly Vector2 StickRest = new Vector2(92f, 132f);
        public const float StickSize = 112f;

        public readonly Slot Attack, Dodge, Skill;
        public readonly Vector2 MoveZoneMin, MoveZoneMax;   // anchors in the safe area
        public readonly bool LeftHanded;

        ControlLayout(bool leftHanded)
        {
            LeftHanded = leftHanded;
            float s = leftHanded ? 1f : -1f;   // + from the left corner, - from the right corner
            // Big attack in the corner with the skill stacked above it on the outer edge. Roll sits low in the bottom
            // centre, under the gap between stick and attack: a 20:9 phone is only ~322 units wide, so it fits below
            // their bottom edges rather than between them.
            float corner = leftHanded ? 0f : 1f;
            Attack = new Slot(new Vector2(s * 80f, 116f), 100f, corner);
            Dodge = new Slot(new Vector2(0f, 36f), 56f, 0.5f);
            Skill = new Slot(new Vector2(s * 76f, 222f), 64f, corner);
            MoveZoneMin = leftHanded ? new Vector2(0.5f, 0f) : Vector2.zero;
            MoveZoneMax = leftHanded ? new Vector2(1f, 0.45f) : new Vector2(0.5f, 0.45f);
        }

        public static ControlLayout Compute(bool leftHanded) => new ControlLayout(leftHanded);

        /// <summary>
        /// Where a floating stick base (size `stickSize`) appears for a touch at `desired`: right under the thumb, kept
        /// off the screen's outer and bottom edges. It may overhang the zone's inner and top edges; only touches are
        /// confined to the zone, not the drawing.
        /// </summary>
        public static Vector2 ClampStick(Vector2 desired, Vector2 zoneSize, float stickSize)
        {
            float r = stickSize / 2f;
            float minX = r + EdgeMargin, minY = r + EdgeMargin;
            return new Vector2(Mathf.Clamp(desired.x, minX, Mathf.Max(minX, zoneSize.x)), Mathf.Clamp(desired.y, minY, Mathf.Max(minY, zoneSize.y)));
        }

        /// <summary>A floating stick follows the thumb: once the thumb is `radius` away, the base is dragged along.</summary>
        public static Vector2 FollowStick(Vector2 basePos, Vector2 thumbFromBase, float radius)
        {
            float d = thumbFromBase.magnitude;
            return d <= radius ? basePos : basePos + thumbFromBase / d * (d - radius);
        }
    }
}
