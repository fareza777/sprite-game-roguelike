using Oathdeep.Data;
using UnityEngine;

namespace Oathdeep.Combat
{
    /// <summary>Holds the live stat block of a combatant and turns attacks into DamageInfo.</summary>
    public sealed class CombatStats : MonoBehaviour
    {
        public StatBlock Stats { get; private set; }
        public Team Team { get; private set; }

        public void Set(StatBlock stats, Team team)
        {
            Stats = stats.Clamped();
            Team = team;
        }

        public DamageInfo BuildDamage(AttackSpec spec, int targetDefense, Vector2 sourcePosition)
        {
            var result = DamageCalculator.Compute(Stats.Attack, targetDefense, spec.Multiplier, Stats.CritChance, Stats.CritMultiplier, Random.value);
            return new DamageInfo(result.Amount, result.IsCrit, sourcePosition, spec.Knockback, spec.StunSeconds, Team, spec.Element);
        }
    }
}
