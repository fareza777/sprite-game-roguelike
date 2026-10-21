using Oathdeep.Combat;
using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>Per-run numbers shown on the Game Over / Victory screens. Listens to GameEvents.</summary>
    public sealed class RunStats
    {
        public int Kills { get; private set; }
        public int EliteKills { get; private set; }
        public int RoomsCleared { get; private set; }
        public int DamageDealt { get; private set; }
        public int HighestHit { get; private set; }
        public int Gold { get; private set; }
        public float StartedAt { get; private set; }
        public float EndedAt { get; private set; }
        public float Duration => (EndedAt > 0f ? EndedAt : Time.time) - StartedAt;

        public void Begin()
        {
            StartedAt = Time.time;
            EndedAt = 0f;
            GameEvents.EnemyKilled += (_, boss) => Kills++;
            GameEvents.RoomCleared += () => RoomsCleared++;
            GameEvents.GoldChanged += total => Gold = total;
            GameEvents.DamageDealt += OnDamage;
        }

        void OnDamage(DamageInfo info, Vector3 _)
        {
            if (info.SourceTeam != Team.Player) return;
            DamageDealt += info.Amount;
            if (info.Amount > HighestHit) HighestHit = info.Amount;
        }

        public void End() => EndedAt = Time.time;

        public static string FormatTime(float seconds)
        {
            int s = Mathf.FloorToInt(seconds);
            return $"{s / 60:00}:{s % 60:00}";
        }
    }
}
