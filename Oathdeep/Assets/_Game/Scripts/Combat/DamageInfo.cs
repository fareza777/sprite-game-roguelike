using UnityEngine;

namespace Oathdeep.Combat
{
    public enum Team { Player, Enemy, Hazard }

    public enum Element { Physical, Fire, Frost, Shock, Poison }

    /// <summary>Everything a hit carries. Immutable value passed from attacker to victim.</summary>
    public readonly struct DamageInfo
    {
        public readonly int Amount;
        public readonly bool IsCrit;
        public readonly Vector2 SourcePosition;
        public readonly float Knockback;
        public readonly float StunSeconds;
        public readonly Team SourceTeam;
        public readonly Element Element;

        public DamageInfo(int amount, bool isCrit, Vector2 sourcePosition, float knockback, float stunSeconds, Team sourceTeam, Element element = Element.Physical)
        {
            Amount = amount;
            IsCrit = isCrit;
            SourcePosition = sourcePosition;
            Knockback = knockback;
            StunSeconds = stunSeconds;
            SourceTeam = sourceTeam;
            Element = element;
        }
    }

    /// <summary>Attack properties authored per move (combo hit, skill, enemy attack).</summary>
    [System.Serializable]
    public struct AttackSpec
    {
        public float Multiplier;
        public float Knockback;
        public float StunSeconds;
        public float HitStopSeconds;
        public float ShakeAmount;
        public Element Element;

        public AttackSpec(float multiplier, float knockback, float stunSeconds, float hitStopSeconds, float shakeAmount, Element element = Element.Physical)
        {
            Multiplier = multiplier;
            Knockback = knockback;
            StunSeconds = stunSeconds;
            HitStopSeconds = hitStopSeconds;
            ShakeAmount = shakeAmount;
            Element = element;
        }
    }
}
