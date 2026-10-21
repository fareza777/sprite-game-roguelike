using System;
using UnityEngine;

namespace Oathdeep.Data
{
    /// <summary>Immutable bundle of combat stats. Combine with +, never mutate.</summary>
    [Serializable]
    public struct StatBlock
    {
        [SerializeField] int maxHealth;
        [SerializeField] int attack;
        [SerializeField] int defense;
        [SerializeField, Range(0f, 1f)] float critChance;
        [SerializeField] float critMultiplier;
        [SerializeField] float moveSpeed;

        public int MaxHealth => maxHealth;
        public int Attack => attack;
        public int Defense => defense;
        public float CritChance => critChance;
        public float CritMultiplier => critMultiplier;
        public float MoveSpeed => moveSpeed;

        public StatBlock(int maxHealth, int attack, int defense, float critChance, float critMultiplier, float moveSpeed)
        {
            this.maxHealth = maxHealth;
            this.attack = attack;
            this.defense = defense;
            this.critChance = critChance;
            this.critMultiplier = critMultiplier;
            this.moveSpeed = moveSpeed;
        }

        public static StatBlock operator +(StatBlock a, StatBlock b) => new StatBlock(
            a.maxHealth + b.maxHealth,
            a.attack + b.attack,
            a.defense + b.defense,
            a.critChance + b.critChance,
            a.critMultiplier + b.critMultiplier,
            a.moveSpeed + b.moveSpeed);

        /// <summary>Returns a copy with every stat inside its playable range.</summary>
        public StatBlock Clamped() => new StatBlock(
            Mathf.Max(1, maxHealth),
            Mathf.Max(0, attack),
            Mathf.Max(0, defense),
            Mathf.Clamp01(critChance),
            Mathf.Max(1f, critMultiplier),
            Mathf.Max(0.5f, moveSpeed));
    }
}
