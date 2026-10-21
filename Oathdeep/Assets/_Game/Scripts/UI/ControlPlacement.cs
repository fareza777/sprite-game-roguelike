using UnityEngine;

namespace Oathdeep.UI
{
    public enum Control { Stick, Attack, Dodge, Skill }

    /// <summary>
    /// Pure maths for player-placed touch controls. Positions are stored as fractions of the HUD safe area so a
    /// layout saved on one phone lands in the same place on another; centres are in canvas units.
    /// </summary>
    public static class ControlPlacement
    {
        const float ZoneHeight = 0.45f;       // the default move zone covers the bottom 45% of the screen
        const float ZoneHeadroom = 0.12f;     // a stick placed higher than that still gets room above it

        public static float SizeOf(Control c, ControlLayout l) => c switch
        {
            Control.Stick => ControlLayout.StickSize,
            Control.Attack => l.Attack.Size,
            Control.Dodge => l.Dodge.Size,
            _ => l.Skill.Size,
        };

        /// <summary>Where a control sits in the built-in layout, on a screen of size `screen`.</summary>
        public static Vector2 DefaultCentre(Control c, ControlLayout l, Vector2 screen)
        {
            if (c == Control.Stick) return new Vector2(l.MoveZoneMin.x * screen.x, 0f) + ControlLayout.StickRest;
            var slot = c == Control.Attack ? l.Attack : c == Control.Dodge ? l.Dodge : l.Skill;
            return new Vector2(slot.AnchorX * screen.x, 0f) + slot.Position;
        }

        /// <summary>Keeps a control of diameter `size` entirely on screen.</summary>
        public static Vector2 Clamp(Vector2 centre, float size, Vector2 screen)
        {
            float r = size / 2f;
            return new Vector2(Mathf.Clamp(centre.x, r, Mathf.Max(r, screen.x - r)), Mathf.Clamp(centre.y, r, Mathf.Max(r, screen.y - r)));
        }

        public static Vector2 Normalize(Vector2 centre, Vector2 screen) =>
            new Vector2(screen.x > 0f ? centre.x / screen.x : 0f, screen.y > 0f ? centre.y / screen.y : 0f);

        public static Vector2 Denormalize(Vector2 normalized, Vector2 screen) => Vector2.Scale(normalized, screen);

        /// <summary>The touch zone for a stick resting at `stick` (normalized): the half of the screen that holds it.</summary>
        public static (Vector2 min, Vector2 max) MoveZoneFor(Vector2 stick)
        {
            bool right = stick.x > 0.5f;
            float top = Mathf.Min(1f, Mathf.Max(ZoneHeight, stick.y + ZoneHeadroom));
            return (new Vector2(right ? 0.5f : 0f, 0f), new Vector2(right ? 1f : 0.5f, top));
        }

        /// <summary>A stick position (normalized to the screen) as a fraction of its zone, measured from the zone corner.</summary>
        public static Vector2 RestInZone(Vector2 stick, Vector2 zoneMin, Vector2 zoneMax)
        {
            var span = zoneMax - zoneMin;
            return new Vector2(span.x > 0f ? (stick.x - zoneMin.x) / span.x : 0f, span.y > 0f ? (stick.y - zoneMin.y) / span.y : 0f);
        }
    }
}
