using Oathdeep.Core;
using Oathdeep.Data;
using UnityEngine;

namespace Oathdeep.Loot
{
    /// <summary>Spawns pickups. Prefab and sprites are provided once per run by the game bootstrap.</summary>
    public static class LootDropper
    {
        const int PotionHeal = 30;

        static Pickup prefab;
        static Sprite goldSprite;
        static Sprite potionSprite;

        public static void Configure(Pickup pickupPrefab, Sprite gold, Sprite potion)
        {
            prefab = pickupPrefab;
            goldSprite = gold;
            potionSprite = potion;
        }

        public static void DropFor(EnemyData data, Vector3 at, bool elite = false)
        {
            if (prefab == null || data == null) return;

            int gold = Random.Range(data.GoldDrop.x, data.GoldDrop.y + 1) * (elite ? 3 : 1);
            // Split gold into a few coins for a satisfying burst.
            int coins = Mathf.Clamp(gold / 3, 1, 6);
            for (int i = 0; i < coins && gold > 0; i++)
            {
                int value = i == coins - 1 ? gold : Mathf.Max(1, gold / coins);
                gold -= value;
                Spawn(PickupKind.Gold, value, null, goldSprite, at);
            }

            if (Random.value < data.PotionDropChance * (elite ? 2f : 1f)) Spawn(PickupKind.Potion, PotionHeal, null, potionSprite, at);

            var table = data.LootTable;
            if (table != null && table.Length > 0 && (data.IsBoss || Random.value < data.ItemDropChance * (elite ? 4f : 1f)))
                SpawnItem(table[Random.Range(0, table.Length)], at);
        }

        /// <summary>Scatter gold coins totalling `amount` (chests, breakables).</summary>
        public static void SpawnGold(int amount, Vector3 at)
        {
            int coins = Mathf.Clamp(amount / 3, 1, 8);
            for (int i = 0; i < coins && amount > 0; i++)
            {
                int value = i == coins - 1 ? amount : Mathf.Max(1, amount / coins);
                amount -= value;
                Spawn(PickupKind.Gold, value, null, goldSprite, at);
            }
        }

        public static void SpawnItem(ItemData item, Vector3 at)
        {
            if (item == null) return;
            Spawn(PickupKind.Item, 1, item, item.Icon, at);
        }

        public static void SpawnPotion(Vector3 at) => Spawn(PickupKind.Potion, PotionHeal, null, potionSprite, at);

        static void Spawn(PickupKind kind, int amount, ItemData item, Sprite icon, Vector3 at)
        {
            if (prefab == null) return;
            var p = Pool.Spawn(prefab, at, Quaternion.identity);
            p.Setup(kind, amount, item, icon);
        }
    }
}
