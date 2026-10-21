using System.Linq;
using NUnit.Framework;
using Oathdeep.Data;
using Oathdeep.Loot;

namespace Oathdeep.Tests
{
    public class LoadoutTests
    {
        static readonly StatBlock Base = new StatBlock(100, 10, 2, 0.05f, 1.5f, 4f);

        static ItemInstance Item(EquipSlot slot, int attack, int hp = 0, ItemSpecials specials = default)
        {
            var baseId = ItemCatalog.Bases.First(b => b.Slot == slot).Id;
            return new ItemInstance(baseId, Rarity.Common, 1, new StatBlock(hp, attack, 0, 0f, 0f, 0f), specials, new string[0], "Test");
        }

        [Test]
        public void Empty_TotalEqualsBase()
        {
            var loadout = new Loadout(Base);
            Assert.AreEqual(10, loadout.Total.Attack);
            Assert.AreEqual(100, loadout.Total.MaxHealth);
            Assert.IsTrue(loadout.Specials.IsEmpty);
        }

        [Test]
        public void Equip_AddsBonusAndReturnsNewLoadout()
        {
            var loadout = new Loadout(Base);
            var next = loadout.Equip(Item(EquipSlot.Weapon, 5), out var replaced);

            Assert.IsNull(replaced);
            Assert.AreEqual(15, next.Total.Attack);
            Assert.AreEqual(10, loadout.Total.Attack, "original loadout must be unchanged");
        }

        [Test]
        public void Equip_SameSlot_ReplacesAndReturnsOldItem()
        {
            var a = Item(EquipSlot.Weapon, 5);
            var b = Item(EquipSlot.Weapon, 8);

            var loadout = new Loadout(Base).Equip(a, out _).Equip(b, out var replaced);

            Assert.AreSame(a, replaced);
            Assert.AreEqual(18, loadout.Total.Attack);
            Assert.AreSame(b, loadout.Get(EquipSlot.Weapon));
        }

        [Test]
        public void DifferentSlots_Stack()
        {
            var loadout = new Loadout(Base)
                .Equip(Item(EquipSlot.Weapon, 5), out _)
                .Equip(Item(EquipSlot.Armor, 0, hp: 30), out _)
                .Equip(Item(EquipSlot.Charm, 2), out _)
                .Equip(Item(EquipSlot.Relic, 1), out _);

            Assert.AreEqual(18, loadout.Total.Attack);
            Assert.AreEqual(130, loadout.Total.MaxHealth);
        }

        [Test]
        public void Unequip_RemovesItemAndItsStats()
        {
            var sword = Item(EquipSlot.Weapon, 5);
            var loadout = new Loadout(Base).Equip(sword, out _).Unequip(EquipSlot.Weapon, out var removed);
            Assert.AreSame(sword, removed);
            Assert.AreEqual(10, loadout.Total.Attack);
            Assert.IsNull(loadout.Get(EquipSlot.Weapon));
        }

        [Test]
        public void Specials_SumAndClamp()
        {
            var loadout = new Loadout(Base)
                .Equip(Item(EquipSlot.Weapon, 0, specials: new ItemSpecials(0.2f, 0.5f, 1f, 3)), out _)
                .Equip(Item(EquipSlot.Relic, 0, specials: new ItemSpecials(0.2f, 0.5f, 1f, 4)), out _);
            Assert.AreEqual(0.25f, loadout.Specials.Lifesteal, 1e-5f, "lifesteal capped");
            Assert.AreEqual(0.6f, loadout.Specials.SkillHaste, 1e-5f, "haste capped");
            Assert.AreEqual(2f, loadout.Specials.GoldFind, 1e-5f);
            Assert.AreEqual(7, loadout.Specials.Thorns);
        }

        [Test]
        public void Equip_Null_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new Loadout(Base).Equip(null, out _));
        }
    }
}
