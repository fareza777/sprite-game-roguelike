using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// The Horologist turns giant clock hands around itself; roll through them. From phase two a second hand turns
    /// opposite the first and bubbles of slow time drift near the hero.
    /// </summary>
    public sealed class ClockHands : BossMechanic
    {
        static readonly AttackSpec HandSpec = new AttackSpec(0.9f, 6f, 0.15f, 0.06f, 0.25f);
        static readonly Color BubbleTint = new Color(1f, 1f, 1f, 0.7f);

        float timer = 4f;
        float bubbleTimer = 6f;
        bool hinted;

        public override void Tick(float dt)
        {
            if (Fx == null) return;
            if (Every(ref timer, Core.Phase >= 3 ? 7f : 10f, dt)) Sweep();
            if (Core.Phase >= 2 && Every(ref bubbleTimer, 8f, dt))
                for (int i = 0; i < 2; i++)
                    Zone(Fx.timeBubble, BubbleTint, HazardKind.Slow, 0.55f, HazardShape.Circle(PointNear(TargetPos, 4f), 2.5f), 1f, 7f);
        }

        void Sweep()
        {
            Vector2 to = TargetPos - (Vector2)Core.transform.position;
            float away = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg + 100f;   // start well off the hero
            float turn = Rng.Next(2) == 0 ? 360f : -360f;
            int hands = Core.Phase >= 2 ? 2 : 1;
            for (int i = 0; i < hands; i++)
                SweepBeam.Spawn(Core.HazardRoot, Fx.clockHand, Core.transform, away + i * 180f, turn, 4.5f, 1.2f, 15f, Core.TargetHealth, Core.Stats, HandSpec);
            if (!hinted) { hinted = true; GameEvents.RaiseToast("The clock hands turn! Roll through them."); }
        }
    }
}
