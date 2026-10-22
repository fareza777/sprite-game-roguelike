using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>Every Chorister fights in three phases: full strength, below two thirds, below one third.</summary>
    public static class BossPhases
    {
        public const int Count = 3;

        public static int PhaseFor(int current, int max)
        {
            if (max <= 0) return 1;
            if (current * 3 <= max) return 3;
            if (current * 3 <= max * 2) return 2;
            return 1;
        }
    }

    /// <summary>
    /// Stagger meter: enough damage in a short time breaks the boss's guard for a few seconds. It drains when the
    /// player stops hitting, ignores damage while broken, and grows after every break so it cannot be chained.
    /// </summary>
    public sealed class BreakGauge
    {
        readonly float duration, decayDelay, decayPerSecond, growth;
        float value, brokenFor, sinceHit;

        public float Capacity { get; private set; }
        public bool Broken => brokenFor > 0f;
        public float Fraction => Broken ? 1f : Mathf.Clamp01(value / Capacity);

        public BreakGauge(float capacity, float duration = 3f, float decayDelay = 2f, float decayPerSecond = 0.12f, float growth = 1.3f)
        {
            Capacity = Mathf.Max(1f, capacity);
            this.duration = duration;
            this.decayDelay = decayDelay;
            this.decayPerSecond = decayPerSecond;
            this.growth = growth;
        }

        /// <summary>Adds damage; true on the hit that breaks the guard.</summary>
        public bool AddDamage(float amount)
        {
            if (Broken || amount <= 0f) return false;
            value += amount;
            sinceHit = 0f;
            if (value < Capacity) return false;
            value = 0f;
            brokenFor = duration;
            Capacity *= growth;
            return true;
        }

        public void Tick(float dt)
        {
            if (Broken) { brokenFor = Mathf.Max(0f, brokenFor - dt); return; }
            float before = sinceHit;
            sinceHit += dt;
            float draining = Mathf.Max(0f, sinceHit - Mathf.Max(decayDelay, before));
            value = Mathf.Max(0f, value - decayPerSecond * Capacity * draining);
        }
    }
}
