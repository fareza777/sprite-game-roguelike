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
            public readonly Vector2 Position;   // offset from the bottom corner on the action side
            public readonly float Size;
            public Slot(Vector2 position, float size) { Position = position; Size = size; }
        }

        public const float EdgeMargin = 14f;        // floating stick never hugs the screen edge
        public static readonly Vector2 StickRest = new Vector2(104f, 132f);

        public readonly Slot Attack, Dodge, Skill;
        public readonly Vector2 MoveZoneMin, MoveZoneMax;   // anchors in the safe area
        public readonly bool LeftHanded;

        ControlLayout(bool leftHanded)
        {
            LeftHanded = leftHanded;
            float s = leftHanded ? 1f : -1f;   // + from the left corner, - from the right corner
            // Big attack in the corner, dodge inward-low, skill stacked above attack on the outer edge
            // so nothing sits over the centre of the play view.
            Attack = new Slot(new Vector2(s * 80f, 104f), 100f);
            Dodge = new Slot(new Vector2(s * 182f, 68f), 64f);
            Skill = new Slot(new Vector2(s * 76f, 210f), 64f);
            MoveZoneMin = leftHanded ? new Vector2(0.5f, 0f) : Vector2.zero;
            MoveZoneMax = leftHanded ? new Vector2(1f, 0.45f) : new Vector2(0.5f, 0.45f);
        }

        public static ControlLayout Compute(bool leftHanded) => new ControlLayout(leftHanded);

        /// <summary>Keeps a floating stick base (size `stickSize`) fully inside the zone with a margin.</summary>
        public static Vector2 ClampStick(Vector2 desired, Vector2 zoneSize, float stickSize)
        {
            float r = stickSize / 2f;
            float minX = r + EdgeMargin, minY = r + EdgeMargin;
            float maxX = Mathf.Max(minX, zoneSize.x - r), maxY = Mathf.Max(minY, zoneSize.y - r);
            return new Vector2(Mathf.Clamp(desired.x, minX, maxX), Mathf.Clamp(desired.y, minY, maxY));
        }
    }
}
