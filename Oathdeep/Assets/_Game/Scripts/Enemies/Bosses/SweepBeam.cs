using Oathdeep.Combat;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// The Horologist's clock hand: a long arm turning around the boss. A thin flashing line warns first; then it sweeps
    /// and hits the hero it passes over (rolling through it is the answer).
    /// </summary>
    public sealed class SweepBeam : MonoBehaviour
    {
        const float Width = 1f;   // one 16 px tile tall

        Transform pivot;
        float startAngle, degreesPerSecond, activeAt, endAt, length;
        SpriteRenderer sr;
        Health victim;
        CombatStats owner;
        AttackSpec spec;
        float nextHit;

        public static SweepBeam Spawn(Transform parent, Sprite sprite, Transform pivot, float startAngle, float sweepDegrees, float sweepSeconds,
            float warn, float length, Health victim, CombatStats owner, AttackSpec spec)
        {
            var go = new GameObject("ClockHand");
            go.transform.SetParent(parent, false);
            var b = go.AddComponent<SweepBeam>();
            b.pivot = pivot;
            b.startAngle = startAngle;
            b.degreesPerSecond = sweepDegrees / Mathf.Max(0.1f, sweepSeconds);
            b.activeAt = Time.time + warn;
            b.endAt = b.activeAt + sweepSeconds;
            b.length = length;
            b.victim = victim;
            b.owner = owner;
            b.spec = spec;
            b.sr = go.AddComponent<SpriteRenderer>();
            b.sr.sprite = sprite;
            b.sr.drawMode = SpriteDrawMode.Tiled;
            b.sr.sortingOrder = 50;
            b.Place();
            return b;
        }

        Vector2 Origin => pivot != null ? (Vector2)pivot.position + Vector2.up * 1.2f : (Vector2)transform.position;
        float Angle => startAngle + degreesPerSecond * Mathf.Max(0f, Time.time - activeAt);

        void Update()
        {
            if (Time.time >= endAt || pivot == null) { Destroy(gameObject); return; }
            Place();
            if (Time.time < activeAt || victim == null || victim.IsDead || Time.time < nextHit) return;
            if (!Beam.Hits(Origin, Angle, length, Width / 2f, victim.transform.position)) return;
            nextHit = Time.time + 0.5f;
            HitQuery.TryHit(owner, victim, spec, Origin);
        }

        void Place()
        {
            bool warning = Time.time < activeAt;
            transform.position = Origin;
            transform.rotation = Quaternion.Euler(0f, 0f, Angle);
            sr.size = new Vector2(length, warning ? Width * 0.35f : Width);
            float a = warning ? (Mathf.Sin(Time.time * 30f) > 0f ? 0.7f : 0.25f) : 1f;
            sr.color = new Color(1f, 1f, 1f, a);
        }
    }
}
