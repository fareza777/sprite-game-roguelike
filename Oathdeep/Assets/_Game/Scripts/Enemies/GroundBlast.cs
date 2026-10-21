using System.Collections.Generic;
using Oathdeep.Audio;
using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// Telegraphed area attack on the ground: a pulsing warning ring, then a burst that hits everything inside.
    /// Pooled; casters and bosses drop several at once.
    /// </summary>
    public sealed class GroundBlast : MonoBehaviour
    {
        [SerializeField] SpriteRenderer warning;
        [SerializeField] GameObject burstVfx;

        CombatStats owner;
        AttackSpec spec;
        float radius;
        float detonateAt;
        readonly HashSet<Health> hit = new HashSet<Health>();

        public static void Spawn(GroundBlast prefab, Vector2 at, float radius, float delay, CombatStats owner, AttackSpec spec)
        {
            if (prefab == null) return;
            var b = Pool.Spawn(prefab, at, Quaternion.identity);
            b.owner = owner;
            b.spec = spec;
            b.radius = radius;
            b.detonateAt = Time.time + delay;
            b.transform.localScale = Vector3.one * radius;
            if (b.warning != null) b.warning.enabled = true;
        }

        void Update()
        {
            if (warning != null)
            {
                float left = detonateAt - Time.time;
                float blink = left < 0.25f ? (Mathf.Sin(Time.time * 50f) > 0f ? 1f : 0.4f) : 0.75f;
                warning.color = new Color(1f, 1f, 1f, blink);
            }
            if (Time.time < detonateAt) return;

            hit.Clear();
            if (owner != null) HitQuery.StrikeCircle(owner, transform.position, radius, spec, hit);
            if (burstVfx != null)
                Pool.Spawn(burstVfx, transform.position, Quaternion.identity).transform.localScale = Vector3.one * radius * 0.9f;
            AudioManager.Play(Sfx.BossSlam, transform.position, 0.6f);
            CameraRig.Shake(0.12f);
            Pool.Despawn(gameObject);
        }
    }
}
