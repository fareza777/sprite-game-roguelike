using UnityEngine;

namespace Oathdeep.Player
{
    public enum PotionPickup { Stored, DrinkNow, Leave }

    /// <summary>
    /// The hero's potions: picked-up potions are kept (up to a few) and drunk from the HUD when needed.
    /// A potion the hero has no use for stays on the floor instead of following them around.
    /// </summary>
    public sealed class PotionPouch
    {
        public const float HealShare = 0.35f;

        public int Count { get; private set; }
        public int Capacity { get; }

        public PotionPouch(int start, int capacity)
        {
            Capacity = Mathf.Max(1, capacity);
            Count = Mathf.Clamp(start, 0, Capacity);
        }

        public static int HealFor(int maxHealth) => Mathf.RoundToInt(maxHealth * HealShare);

        /// <summary>Would picking a potion up do anything right now? (Pickups only home in when it would.)</summary>
        public bool WantsMore(bool hpFull) => Count < Capacity || !hpFull;

        public PotionPickup Pickup(bool hpFull)
        {
            if (Count < Capacity) { Count++; return PotionPickup.Stored; }
            return hpFull ? PotionPickup.Leave : PotionPickup.DrinkNow;
        }

        public bool TryDrink(bool hpFull)
        {
            if (Count == 0 || hpFull) return false;
            Count--;
            return true;
        }
    }
}
