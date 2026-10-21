using System.Collections.Generic;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Combat
{
    /// <summary>Instant overlap-based hit detection. Each Health is hit at most once per swing via the shared set.</summary>
    public static class HitQuery
    {
        static readonly Collider2D[] Buffer = new Collider2D[32];
        static ContactFilter2D filter = new ContactFilter2D { useTriggers = true };

        public static int StrikeBox(CombatStats attacker, Vector2 center, Vector2 size, float angle, AttackSpec spec, HashSet<Health> alreadyHit)
        {
            int count = Physics2D.OverlapBox(center, size, angle, filter, Buffer);
            return Apply(attacker, count, spec, alreadyHit, center);
        }

        public static int StrikeCircle(CombatStats attacker, Vector2 center, float radius, AttackSpec spec, HashSet<Health> alreadyHit)
        {
            int count = Physics2D.OverlapCircle(center, radius, filter, Buffer);
            return Apply(attacker, count, spec, alreadyHit, center);
        }

        static int Apply(CombatStats attacker, int count, AttackSpec spec, HashSet<Health> alreadyHit, Vector2 hitCenter)
        {
            int hits = 0;
            for (int i = 0; i < count; i++)
            {
                var health = Buffer[i].GetComponentInParent<Health>();
                if (health == null || health.Team == attacker.Team || health.IsDead) continue;
                if (alreadyHit != null && !alreadyHit.Add(health)) continue;
                if (TryHit(attacker, health, spec, attacker.transform.position)) hits++;
            }
            return hits;
        }

        /// <summary>Single-target hit used by projectiles and contact damage.</summary>
        public static bool TryHit(CombatStats attacker, Health target, AttackSpec spec, Vector2 sourcePosition)
        {
            var targetStats = target.GetComponent<CombatStats>();
            int defense = targetStats != null ? targetStats.Stats.Defense : 0;
            var info = attacker.BuildDamage(spec, defense, sourcePosition);
            if (!target.TryTakeDamage(info)) return false;

            GameEvents.RaiseDamageDealt(info, target.transform.position);
            HitStop.Trigger(info.IsCrit ? spec.HitStopSeconds * 1.6f : spec.HitStopSeconds);
            CameraRig.Shake(info.IsCrit ? spec.ShakeAmount * 1.5f : spec.ShakeAmount);
            return true;
        }
    }
}
