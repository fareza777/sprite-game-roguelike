using System;
using Oathdeep.Combat;
using Oathdeep.Loot;
using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>Global game-wide signals. Cleared on every new run to avoid stale listeners.</summary>
    public static class GameEvents
    {
        public static event Action<DamageInfo, Vector3> DamageDealt;
        public static event Action<Vector3, bool> EnemyKilled;          // position, wasBoss
        public static event Action<int> GoldChanged;                     // new total
        public static event Action<ItemInstance> ItemEquipped;
        public static event Action<ItemInstance> ItemFound;
        public static event Action<string> Toast;
        public static event Action EliteKilled;
        public static event Action ChestOpened;
        public static event Action Purchased;
        public static event Action<string> EncounterUsed;                 // "pilgrim", "sunstone", "dice"...
        public static event Action RoomCleared;
        public static event Action PlayerDied;
        public static event Action BossDefeated;
        public static event Action<float, float> BossHealthChanged;      // current, max
        public static event Action<string> BossEncountered;
        public static event Action<Vector2Int> RoomEntered;

        public static void RaiseDamageDealt(DamageInfo info, Vector3 at) => DamageDealt?.Invoke(info, at);
        public static void RaiseEnemyKilled(Vector3 at, bool boss) => EnemyKilled?.Invoke(at, boss);
        public static void RaiseGoldChanged(int total) => GoldChanged?.Invoke(total);
        public static void RaiseItemEquipped(ItemInstance item) => ItemEquipped?.Invoke(item);
        public static void RaiseItemFound(ItemInstance item) => ItemFound?.Invoke(item);
        public static void RaiseToast(string message) => Toast?.Invoke(message);
        public static void RaiseEliteKilled() => EliteKilled?.Invoke();
        public static void RaiseChestOpened() => ChestOpened?.Invoke();
        public static void RaisePurchased() => Purchased?.Invoke();
        public static void RaiseEncounterUsed(string kind) => EncounterUsed?.Invoke(kind);
        public static void RaiseRoomCleared() => RoomCleared?.Invoke();
        public static void RaisePlayerDied() => PlayerDied?.Invoke();
        public static void RaiseBossDefeated() => BossDefeated?.Invoke();
        public static void RaiseBossHealthChanged(float cur, float max) => BossHealthChanged?.Invoke(cur, max);
        public static void RaiseBossEncountered(string name) => BossEncountered?.Invoke(name);
        public static void RaiseRoomEntered(Vector2Int cell) => RoomEntered?.Invoke(cell);

        public static void ClearAll()
        {
            DamageDealt = null; EnemyKilled = null; GoldChanged = null; ItemEquipped = null; ItemFound = null; Toast = null;
            EliteKilled = null; ChestOpened = null; Purchased = null; EncounterUsed = null;
            RoomCleared = null; PlayerDied = null; BossDefeated = null; BossHealthChanged = null; BossEncountered = null; RoomEntered = null;
        }
    }
}
