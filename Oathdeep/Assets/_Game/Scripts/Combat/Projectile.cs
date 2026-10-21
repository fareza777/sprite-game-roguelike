using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Combat
{
    /// <summary>Straight-flying pooled projectile. Dies on walls or on first valid hit.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] float lifetime = 3f;
        [SerializeField] GameObject impactVfx;

        Rigidbody2D body;
        CombatStats owner;
        AttackSpec spec;
        float dieAt;

        void Awake() => body = GetComponent<Rigidbody2D>();

        public void Launch(CombatStats shooter, Vector2 direction, float speed, AttackSpec attack)
        {
            owner = shooter;
            spec = attack;
            dieAt = Time.time + lifetime;
            body.linearVelocity = direction.normalized * speed;
            transform.right = direction;
        }

        void Update()
        {
            if (Time.time >= dieAt) Despawn();
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
