using System.Collections.Generic;
using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// Mother Morel: lingering spore clouds bloom around the hero and slowly fill the arena. Breaking a spore pod
    /// blows away every cloud near it.
    /// </summary>
    public sealed class SporeGarden : BossMechanic
    {
        static readonly Color Tint = new Color(1f, 1f, 1f, 0.85f);
        const float PodClearRadius = 4.5f;

        readonly List<HazardZone> clouds = new List<HazardZone>();
        readonly List<BossAnchor> pods = new List<BossAnchor>();
        float timer = 3f;
        bool hinted;

        public override void Tick(float dt)
        {
            if (Fx == null || !Every(ref timer, Core.Phase >= 3 ? 4.5f : 7f, dt)) return;
            clouds.RemoveAll(c => c == null);
            for (int i = 0; i < 2; i++)
                clouds.Add(Zone(Fx.sporeCloud, Tint, HazardKind.Burn, 0f, HazardShape.Circle(PointNear(TargetPos, 3.5f), 2.25f), 1f, 12f));
            pods.RemoveAll(p => p == null);
            if (pods.Count < 2)
            {
                var pod = BossAnchor.Spawn(Core.HazardRoot, Fx.sporePod, PointNear(TargetPos, 5f), Mathf.Max(20, Mathf.RoundToInt(Core.Health.Max * 0.02f)), new Vector2(1f, 1f));
                pod.Broken += Burst;
                pods.Add(pod);
            }
            if (!hinted) { hinted = true; GameEvents.RaiseToast("Spores! Break a pod to clear them."); }
        }

        void Burst(BossAnchor pod)
        {
            pods.Remove(pod);
            Vector2 at = pod.transform.position;
            foreach (var c in clouds)
                if (c != null && (c.Shape.Centre - at).sqrMagnitude <= PodClearRadius * PodClearRadius) c.Dismiss();
        }

        public override void End()
        {
            foreach (var p in pods) if (p != null) Destroy(p.gameObject);
            pods.Clear();
            clouds.Clear();
        }
    }
}
