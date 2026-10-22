using System;
using UnityEngine;

namespace Oathdeep.Combat
{
    /// <summary>Hit points, invulnerability window and death for any combatant.</summary>
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] Team team = Team.Enemy;
        [SerializeField] float invulnerableAfterHit = 0f;

        public Team Team => team;
        public int Current { get; private set; }
        public int Max { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsInvulnerable => Time.time < invulnerableUntil || invulnerableLocks > 0;

        public event Action<DamageInfo> Damaged;
        public event Action<int> Healed;
        public event Action Died;
        public event Action<int, int> Changed; // current, max
        public event Action<DamageInfo> Blocked;  // a hit shrugged off while immune (a shielded boss)

        /// <summary>Scales incoming damage: 0 is immune (shielded), 2 is an exposed weak point.</summary>
        public float DamageTakenMultiplier { get; set; } = 1f;

        /// <summary>The last hit as actually taken, after <see cref="DamageTakenMultiplier"/>.</summary>
        public DamageInfo LastHit { get; private set; }

        float invulnerableUntil;
        int invulnerableLocks;

        public void Init(int max, Team owner)
        {
            team = owner;
            Max = Mathf.Max(1, max);
            Current = Max;
            IsDead = false;
            invulnerableLocks = 0;
            invulnerableUntil = 0f;
            Changed?.Invoke(Current, Max);
        }

        /// <summary>Raise max HP keeping the same missing amount (used when gear changes).</summary>
        public void SetMax(int max)
        {
            int missing = Max - Current;
            Max = Mathf.Max(1, max);
            Current = Mathf.Clamp(Max - missing, 1, Max);
            Changed?.Invoke(Current, Max);
        }

        public bool TryTakeDamage(DamageInfo info)
        {
            if (IsDead || IsInvulnerable || info.SourceTeam == team) return false;
            if (DamageTakenMultiplier <= 0f) { Blocked?.Invoke(info); return false; }
            if (!Mathf.Approximately(DamageTakenMultiplier, 1f))
                info = new DamageInfo(Mathf.Max(1, Mathf.RoundToInt(info.Amount * DamageTakenMultiplier)), info.IsCrit, info.SourcePosition,
                    info.Knockback, info.StunSeconds, info.SourceTeam, info.Element);
            LastHit = info;

            Current = Mathf.Max(0, Current - info.Amount);
            if (invulnerableAfterHit > 0f) invulnerableUntil = Time.time + invulnerableAfterHit;
            Damaged?.Invoke(info);
            Changed?.Invoke(Current, Max);

            if (Current == 0)
            {
                IsDead = true;
                Died?.Invoke();
            }
            return true;
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0) return;
            int before = Current;
            Current = Mathf.Min(Max, Current + amount);
            if (Current != before)
            {
                Healed?.Invoke(Current - before);
                Changed?.Invoke(Current, Max);
            }
        }

        /// <summary>Stackable invulnerability (dodge i-frames, cutscenes).</summary>
        public void PushInvulnerable() => invulnerableLocks++;
        public void PopInvulnerable() => invulnerableLocks = Mathf.Max(0, invulnerableLocks - 1);
    }
}
