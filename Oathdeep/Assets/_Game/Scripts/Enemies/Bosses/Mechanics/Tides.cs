using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// The Tidecaller floods half the arena, alternating sides, after a flashing warning. Wading is slow, so the hero
    /// has to keep to the dry side. Late in the fight only a third of the floor stays dry.
    /// </summary>
    public sealed class Tides : BossMechanic
    {
        static readonly Color Tint = new Color(1f, 1f, 1f, 0.55f);

        float timer = 4f;
        bool left;
        bool hinted;

        public override void Tick(float dt)
        {
            if (Fx == null || !Every(ref timer, Core.Phase >= 3 ? 7f : 9f, dt)) return;
            left = !left;
            var a = Arena;
            if (Core.Phase >= 3)
            {
                // Two thirds flood; a random vertical third stays dry.
                int dry = Rng.Next(3);
                float w = a.width / 3f;
                for (int i = 0; i < 3; i++)
                    if (i != dry) Flood(new Rect(a.xMin + w * i, a.yMin, w, a.height));
            }
            else
            {
                Flood(new Rect(left ? a.xMin : a.center.x, a.yMin, a.width / 2f, a.height));
            }
            if (!hinted) { hinted = true; GameEvents.RaiseToast("The tide rises! Keep to dry ground."); }
        }

        void Flood(Rect r) => Zone(Fx.water, Tint, HazardKind.Slow, 0.45f, HazardShape.Box(r), 1.6f, 5.5f);
    }
}
