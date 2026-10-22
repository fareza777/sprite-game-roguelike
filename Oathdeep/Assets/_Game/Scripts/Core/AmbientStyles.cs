using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>How a floor's air moves: dust, spores, drips, embers, snow, leaves, sparks, sand, glints, ash.</summary>
    public readonly struct AmbientStyle
    {
        public readonly int Count;
        public readonly Vector2 Velocity;   // world units per second
        public readonly float Sway;         // side-to-side wobble, units
        public readonly float Alpha;
        public readonly bool Glow;          // unlit: embers and glints shine in the dark

        public AmbientStyle(int count, Vector2 velocity, float sway, float alpha, bool glow)
        {
            Count = count; Velocity = velocity; Sway = sway; Alpha = alpha; Glow = glow;
        }
    }

    /// <summary>The per-floor table, kept sparse and faint so the air sets a mood without hiding the fight.</summary>
    public static class AmbientStyles
    {
        static readonly AmbientStyle[] Floors =
        {
            new AmbientStyle(20, new Vector2(0.1f, -0.05f), 0.3f, 0.45f, false),   // 1 crypt: dust
            new AmbientStyle(24, new Vector2(0f, 0.25f), 0.5f, 0.6f, true),        // 2 hollow: glowing spores
            new AmbientStyle(16, new Vector2(0f, -2.2f), 0f, 0.5f, false),         // 3 cistern: drips
            new AmbientStyle(24, new Vector2(0.2f, 0.9f), 0.4f, 0.75f, true),      // 4 forge: embers
            new AmbientStyle(32, new Vector2(0.3f, -0.8f), 0.6f, 0.7f, false),     // 5 frost: snow
            new AmbientStyle(18, new Vector2(0.5f, -0.5f), 0.8f, 0.7f, false),     // 6 thornwild: leaves
            new AmbientStyle(16, new Vector2(0f, -0.3f), 0.2f, 0.6f, true),        // 7 clockwork: brass glints
            new AmbientStyle(28, new Vector2(1.6f, -0.15f), 0.2f, 0.5f, false),    // 8 necropolis: blowing sand
            new AmbientStyle(18, new Vector2(0f, 0.15f), 0.2f, 0.75f, true),       // 9 glass: glints
            new AmbientStyle(24, new Vector2(0.15f, 0.5f), 0.5f, 0.7f, true),      // 10 dusk: ash and embers
        };

        public static AmbientStyle For(int floor) => Floors[Mathf.Clamp(floor, 1, Floors.Length) - 1];

        /// <summary>A mote that drifted out of the view comes back in on the opposite edge.</summary>
        public static Vector2 Wrap(Vector2 p, Rect view)
        {
            float x = p.x, y = p.y;
            if (x > view.xMax) x -= view.width; else if (x < view.xMin) x += view.width;
            if (y > view.yMax) y -= view.height; else if (y < view.yMin) y += view.height;
            return new Vector2(x, y);
        }
    }
}
