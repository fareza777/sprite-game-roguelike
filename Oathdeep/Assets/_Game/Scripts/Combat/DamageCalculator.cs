using UnityEngine;

namespace Oathdeep.Combat
{
    public readonly struct DamageResult
    {
        public readonly int Amount;
        public readonly bool IsCrit;

        public DamageResult(int amount, bool isCrit)
        {
            Amount = amount;
            IsCrit = isCrit;
        }
    }

    /// <summary>Pure damage formula. Defense uses diminishing returns: dmg * 100 / (100 + def).</summary>
    public static class DamageCalculator
    {
        const float DefenseScale = 100f;

        public static DamageResult Compute(int attack, int defense, float multiplier, float critChance, float critMultiplier, float roll)
        {
            float raw = attack * multiplier;
            float mitigated = raw * DefenseScale / (DefenseScale + Mathf.Max(0, defense));
            bool isCrit = roll < critChance;
            if (isCrit) mitigated *= critMultiplier;
            return new DamageResult(Mathf.Max(1, Mathf.RoundToInt(mitigated)), isCrit);
        }
    }
}
