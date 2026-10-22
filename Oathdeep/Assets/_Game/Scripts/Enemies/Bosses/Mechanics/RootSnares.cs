using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>The Verdant Hierophant marks the ground under the hero; stay on the mark and roots hold the feet.</summary>
    public sealed class RootSnares : BossMechanic
    {
        float timer = 3f;
        bool hinted;

        public override void Tick(float dt)
        {
            if (Fx == null || !Every(ref timer, Core.Phase >= 3 ? 4.5f : 6.5f, dt)) return;
            int count = Core.Phase >= 3 ? 2 : 1;
            for (int i = 0; i < count; i++)
            {
                var at = i == 0 ? TargetPos : PointNear(TargetPos, 3f);
                Zone(Fx.rootSnare, Color.white, HazardKind.Root, Core.Phase >= 2 ? 1.8f : 1.4f, HazardShape.Circle(at, 1.25f), 1f, 0.6f);
            }
            if (!hinted) { hinted = true; GameEvents.RaiseToast("Roots stir underfoot! Move off the mark."); }
        }
    }
}
