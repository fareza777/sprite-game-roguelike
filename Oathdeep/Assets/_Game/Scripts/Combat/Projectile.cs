using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Combat
{
    /// <summary>Pooled projectile. Flies straight, or curves toward the nearest enemy when given a homing rate.
    /// Dies on walls or on first valid hit.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Projectile : MonoBehaviour
    {
        const float HomingSearchRange = 6f;

        [SerializeField] float lifetime = 3f;
        [SerializeField] GameObject impactVfx;

        Rigidbody2D body;
        CombatStats owner;
        AttackSpec spec;
        float dieAt;
        float speed;
        float homing;

        void Awake() => body = GetComponent<Rigidbody2D>();

        public void Launch(CombatStats shooter, Vector2 direction, float launchSpeed, AttackSpec attack, float homingRate = 0f)
        {
            owner = shooter;
            spec = attack;
            speed = launchSpeed;
            homing = homingRate;
            dieAt = Time.time + lifetime;
            body.linearVelocity = direction.normalized * speed;
            transform.right = direction;
        }

        void Update()
        {
            if (Time.time >= dieAt) Despawn();
        }

        void FixedUpdate()
        {
            if (homing <= 0f || owner == null || owner.Team != Team.Player) return;
            var target = TargetRegistry.Nearest(transform.position, HomingSearchRange);
            if (target == null) return;
            Vector2 want = ((Vector2)target.transform.position + Vector2.up * 0.4f - (Vector2)transform.position).normalized;
            Vector2 now = body.linearVelocity.normalized;
            Vector2 turned = Vector3.RotateTowards(now, want, homing * Time.fixedDeltaTime, 0f);
            body.linearVelocity = turned * speed;
            transform.right = turned;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (owner == null) { Despawn(); return; }

            var health = other.GetComponentInParent<Health>();
            if (health != null)
            {
                if (health.Team == owner.Team || health.IsDead) return;
                if (HitQuery.TryHit(owner, health, spec, transform.position)) Despawn();
                return;
            }
            if (!other.isTrigger) Despawn(); // wall or obstacle
        }

        void Despawn()
        {
            if (impactVfx != null) Pool.Spawn(impactVfx, transform.position, Quaternion.identity);
            body.linearVelocity = Vector2.zero;
            Pool.Despawn(gameObject);
        }
    }
}
