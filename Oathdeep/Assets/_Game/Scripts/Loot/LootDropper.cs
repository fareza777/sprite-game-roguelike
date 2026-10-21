using Oathdeep.Core;
using Oathdeep.Data;
using UnityEngine;

namespace Oathdeep.Loot
{
    /// <summary>Spawns pickups. Prefab and sprites are provided once per run by the game bootstrap;
    /// the current floor decides how strong rolled items are.</summary>
    public static class LootDropper
    {
        const int PotionHeal = 30;
        const float EliteLuck = 0.6f, BossLuck = 2.5f;

        static Pickup prefab;
        static Sprite goldSprite;
        static Sprite potionSprite;
        static System.Random rng = new System.Random();

        public static int Floor { get; private set; } = 1;

        public static void Configure(Pickup pickupPrefab, Sprite gold, Sprite potion)
        {
            prefab = pickupPrefab;
            goldSprite = gold;
            potionSprite = potion;
        }

        public static void BeginFloor(int floor, int seed)
        {
            Floor = Mathf.Max(1, floor);
            rng = new System.Random(seed ^ 0x5EED);
        }

        public static void DropFor(EnemyData data, Vector3 at, bool elite = false)
        {
            if (prefab == null || data == null) return;

            int gold = Random.Range(data.GoldDrop.x, data.GoldDrop.y + 1) * (elite ? 3 : 1);
            gold = Mathf.RoundToInt(gold * (1f + 0.15f * (Floor - 1)));
            SpawnGold(gold, at);

            if (Random.value < data.PotionDropChance * (elite ? 2f : 1f)) Spawn(PickupKind.Potion, PotionHeal, null, potionSprite, at);

            if (data.IsBoss)
            {
                // Bosses always pay out: one strong item plus a good chance of a second.
                SpawnItem(ItemGenerator.Roll(rng, Floor, BossLuck), at);
                if (rng.NextDouble() < 0.5) SpawnItem(ItemGenerator.Roll(rng, Floor, BossLuck), at);
            }
            else if (Random.value < data.ItemDropChance * (elite ? 4f : 1f))
                SpawnItem(ItemGenerator.Roll(rng, Floor, elite ? EliteLuck : 0f), at);
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

        public static void SpawnItem(ItemInstance item, Vector3 at)
        {
            if (item == null) return;
            Spawn(PickupKind.Item, 1, item, ItemIcons.For(item), at);
        }

        public static void SpawnPotion(Vector3 at) => Spawn(PickupKind.Potion, PotionHeal, null, potionSprite, at);

        static void Spawn(PickupKind kind, int amount, ItemInstance item, Sprite icon, Vector3 at)
        {
            if (prefab == null) return;
            var p = Pool.Spawn(prefab, at, Quaternion.identity);
            p.Setup(kind, amount, item, icon);
        }
    }
}
