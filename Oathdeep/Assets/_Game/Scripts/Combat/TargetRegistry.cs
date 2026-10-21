using System.Collections.Generic;
using UnityEngine;

namespace Oathdeep.Combat
{
    /// <summary>Live enemies, for auto-aim and room-clear checks without physics queries.</summary>
    public static class TargetRegistry
    {
        static readonly List<Health> Enemies = new List<Health>();

        public static IReadOnlyList<Health> All => Enemies;

        public static void Register(Health h) { if (!Enemies.Contains(h)) Enemies.Add(h); }
        public static void Unregister(Health h) => Enemies.Remove(h);
        public static void Clear() => Enemies.Clear();

        public static Health Nearest(Vector2 from, float maxDistance)
        {
            Health best = null;
            float bestSq = maxDistance * maxDistance;
            foreach (var e in Enemies)
            {
                if (e == null || e.IsDead) continue;
                float sq = ((Vector2)e.transform.position - from).sqrMagnitude;
                if (sq < bestSq) { bestSq = sq; best = e; }
            }
            return best;
        }
    }
}
