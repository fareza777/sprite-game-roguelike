using System;
using Oathdeep.Data;

namespace Oathdeep.Loot
{
    /// <summary>Immutable hero base stats + one item per slot. Equip/Unequip return a new Loadout.</summary>
    public sealed class Loadout
    {
        static readonly int SlotCount = Enum.GetValues(typeof(EquipSlot)).Length;

        public StatBlock Base { get; }
        public StatBlock Total { get; }
        public ItemSpecials Specials { get; }
        readonly ItemInstance[] slots;

        public Loadout(StatBlock baseStats) : this(baseStats, new ItemInstance[SlotCount]) { }

        Loadout(StatBlock baseStats, ItemInstance[] items)
        {
            Base = baseStats;
            slots = items;
            var total = baseStats;
            var specials = ItemSpecials.None;
            foreach (var item in items)
            {
                if (item == null) continue;
                total += item.Stats;
                specials += item.Specials;
            }
            Total = total.Clamped();
            Specials = specials.Clamped();
        }

        public ItemInstance Get(EquipSlot slot) => slots[(int)slot];

        public Loadout Equip(ItemInstance item, out ItemInstance replaced)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var copy = (ItemInstance[])slots.Clone();
            replaced = copy[(int)item.Slot];
            copy[(int)item.Slot] = item;
            return new Loadout(Base, copy);
        }

        public Loadout Unequip(EquipSlot slot, out ItemInstance removed)
        {
            var copy = (ItemInstance[])slots.Clone();
            removed = copy[(int)slot];
            copy[(int)slot] = null;
            return new Loadout(Base, copy);
        }
    }
}
