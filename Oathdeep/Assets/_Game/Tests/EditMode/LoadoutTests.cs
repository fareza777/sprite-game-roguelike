using NUnit.Framework;
using Oathdeep.Data;
using Oathdeep.Loot;
using UnityEngine;

namespace Oathdeep.Tests
{
    public class LoadoutTests
    {
        static readonly StatBlock Base = new StatBlock(100, 10, 2, 0.05f, 1.5f, 4f);

        static ItemData Item(EquipSlot slot, int attack, int hp = 0)
        {
            var item = ScriptableObject.CreateInstance<ItemData>();
            item.EditorInit("test", "Test", "", null, Rarity.Common, slot, new StatBlock(hp, attack, 0, 0f, 0f, 0f));
            return item;
        }

        [Test]
        public void Empty_TotalEqualsBase()
        {
            var loadout = new Loadout(Base);
            Assert.AreEqual(10, loadout.Total.Attack);
            Assert.AreEqual(100, loadout.Total.MaxHealth);
        }

        [Test]
        public void Equip_AddsBonusAndReturnsNewLoadout()
        {
            var loadout = new Loadout(Base);
            var sword = Item(EquipSlot.Weapon, 5);

            var next = loadout.Equip(sword, out var replaced);

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
                .Equip(Item(EquipSlot.Accessory, 2), out _);

            Assert.AreEqual(17, loadout.Total.Attack);
            Assert.AreEqual(130, loadout.Total.MaxHealth);
        }

        [Test]
        public void Equip_Null_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new Loadout(Base).Equip(null, out _));
        }
    }
}
