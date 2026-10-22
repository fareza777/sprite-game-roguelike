using Oathdeep.Combat;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// One signature rule of a boss fight (rising tides, root snares, a sweeping clock hand...). Runs only in its
    /// phase range, so the Cantor can stack several that switch on as the fight goes on.
    /// </summary>
    public abstract class BossMechanic : MonoBehaviour
    {
        [SerializeField, Range(1, 3)] int fromPhase = 1;
        [SerializeField, Range(1, 3)] int untilPhase = 3;

        /// <summary>Ground hazards burn for a third of the boss's attack every 0.6 s.</summary>
        protected static readonly AttackSpec BurnSpec = new AttackSpec(0.35f, 0f, 0f, 0f, 0.05f);

        protected BossCore Core { get; private set; }
        protected readonly System.Random Rng = new System.Random();

        public void Attach(BossCore core) => Core = core;
        public bool ActiveIn(int phase) => phase >= fromPhase && phase <= untilPhase;

        /// <summary>Entering a phase this mechanic runs in (phase 1 when the fight starts).</summary>
        public virtual void OnPhase(int phase) { }
        public virtual void Tick(float dt) { }
        public virtual void OnMove(BossMove move) { }
        /// <summary>0 shields the boss, 2 exposes it.</summary>
        public virtual float DamageMultiplier => 1f;
        /// <summary>What the toast says when a hit bounces off the boss.</summary>
        public virtual string BlockedHint => null;
        public virtual void End() { }

        protected Vector2 TargetPos => Core.Target != null ? (Vector2)Core.Target.position : Core.Arena.center;
        protected Rect Arena => Core.Arena;
        protected BossFx Fx => Core.Fx;

        /// <summary>True every `interval` seconds; `timer` counts down.</summary>
        protected static bool Every(ref float timer, float interval, float dt)
        {
            timer -= dt;
            if (timer > 0f) return false;
            timer += interval;
            return true;
        }

        /// <summary>A point within `spread` of `centre`, kept `margin` away from the arena walls.</summary>
        protected Vector2 PointNear(Vector2 centre, float spread, float margin = 1.5f)
        {
            Vector2 p = centre;
            for (int i = 0; i < 12; i++)   // prefer open floor over pillars
            {
                p = centre + new Vector2((float)(Rng.NextDouble() * 2 - 1), (float)(Rng.NextDouble() * 2 - 1)) * spread;
                p = new Vector2(Mathf.Clamp(p.x, Arena.xMin + margin, Arena.xMax - margin), Mathf.Clamp(p.y, Arena.yMin + margin, Arena.yMax - margin));
                if (Core.IsOpenFloor(p)) return p;
            }
            return p;
        }

        /// <summary>`count` open-floor points on a ring of `radius` around the arena centre, starting at `startDegrees`.</summary>
        protected System.Collections.Generic.List<Vector2> RingPoints(int count, float radius, float startDegrees)
        {
            var list = new System.Collections.Generic.List<Vector2>();
            for (int i = 0; i < count; i++)
            {
                float a = (startDegrees + i * 360f / count) * Mathf.Deg2Rad;
                var p = Arena.center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                list.Add(Core.IsOpenFloor(p) ? p : PointNear(p, 2f));
            }
            return list;
        }

        protected HazardZone Zone(Sprite sprite, Color tint, HazardKind kind, float strength, HazardShape shape, float warn, float life) =>
            HazardZone.Spawn(Core.HazardRoot, sprite, tint, kind, strength, shape, warn, life, Core.TargetHealth, Core.Stats, BurnSpec);

#if UNITY_EDITOR
        public void EditorPhases(int from, int until) { fromPhase = from; untilPhase = until; }
#endif
    }
}
