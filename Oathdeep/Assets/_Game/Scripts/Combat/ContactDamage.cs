using UnityEngine;

namespace Oathdeep.Combat
{
    /// <summary>Damages the opposing team on touch, with a per-target cooldown.</summary>
    public sealed class ContactDamage : MonoBehaviour
    {
        [SerializeField] AttackSpec spec = new AttackSpec(0.6f, 5f, 0f, 0.03f, 0.15f);
        [SerializeField] float cooldown = 0.6f;

        CombatStats stats;
        Health self;
        float readyAt;

        public bool Active { get; set; } = true;

        void Awake()
        {
            stats = GetComponentInParent<CombatStats>();
            self = GetComponentInParent<Health>();
        }

        void OnTriggerStay2D(Collider2D other) => TryHit(other);
        void OnCollisionStay2D(Collision2D c) => TryHit(c.collider);

        void TryHit(Collider2D other)
        {
            if (!Active || Time.time < readyAt || stats == null || (self != null && self.IsDead)) return;
            var target = other.GetComponentInParent<Health>();
            if (target == null || target.Team == stats.Team) return;
            if (HitQuery.TryHit(stats, target, spec, transform.position)) readyAt = Time.time + cooldown;
        }
    }
}
