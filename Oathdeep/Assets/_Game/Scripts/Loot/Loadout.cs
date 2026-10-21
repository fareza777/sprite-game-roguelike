using System;
using Oathdeep.Data;

namespace Oathdeep.Loot
{
    /// <summary>Immutable base stats + one item per slot. Equip returns a new Loadout.</summary>
    public sealed class Loadout
    {
        static readonly int SlotCount = Enum.GetValues(typeof(EquipSlot)).Length;

        public StatBlock Base { get; }
        public StatBlock Total { get; }
        readonly ItemData[] slots;

        public Loadout(StatBlock baseStats) : this(baseStats, new ItemData[SlotCount]) { }

        Loadout(StatBlock baseStats, ItemData[] items)
        {
            Base = baseStats;
            slots = items;
            var total = baseStats;
            foreach (var item in items)
                if (item != null) total += item.Bonus;
            Total = total.Clamped();
        }

        public ItemData Get(EquipSlot slot) => slots[(int)slot];

        public Loadout Equip(ItemData item, out ItemData replaced)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var copy = (ItemData[])slots.Clone();
            replaced = copy[(int)item.Slot];
            copy[(int)item.Slot] = item;
            return new Loadout(Base, copy);
        }
    }
}
