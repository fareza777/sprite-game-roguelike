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
        readonly ItemSpecials bonusSpecials;

        public Loadout(StatBlock baseStats) : this(baseStats, new ItemInstance[SlotCount], ItemSpecials.None) { }

        Loadout(StatBlock baseStats, ItemInstance[] items, ItemSpecials bonus)
        {
            Base = baseStats;
            slots = items;
            bonusSpecials = bonus;
            var total = baseStats;
            var specials = bonus;
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

        /// <summary>Same gear on a new base (run-long blessings change the base, not the items).</summary>
        public Loadout WithBase(StatBlock newBase) => new Loadout(newBase, (ItemInstance[])slots.Clone(), bonusSpecials);

        /// <summary>Same gear with extra specials that do not come from an item (shrine blessings).</summary>
        public Loadout WithBonusSpecials(ItemSpecials extra) => new Loadout(Base, (ItemInstance[])slots.Clone(), bonusSpecials + extra);

        public Loadout Equip(ItemInstance item, out ItemInstance replaced)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var copy = (ItemInstance[])slots.Clone();
            replaced = copy[(int)item.Slot];
            copy[(int)item.Slot] = item;
            return new Loadout(Base, copy, bonusSpecials);
        }

        public Loadout Unequip(EquipSlot slot, out ItemInstance removed)
        {
            var copy = (ItemInstance[])slots.Clone();
            removed = copy[(int)slot];
            copy[(int)slot] = null;
            return new Loadout(Base, copy, bonusSpecials);
        }
    }
}
