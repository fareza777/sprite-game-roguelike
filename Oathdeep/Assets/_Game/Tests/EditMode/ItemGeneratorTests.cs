using System;
using System.Linq;
using NUnit.Framework;
using Oathdeep.Loot;

namespace Oathdeep.Tests
{
    public class ItemGeneratorTests
    {
        [Test]
        public void Catalog_HasAtLeast100UniqueIdentities()
        {
            Assert.GreaterOrEqual(ItemCatalog.Count, 100);
            Assert.AreEqual(ItemCatalog.Count, ItemCatalog.All.Select(b => b.Id).Distinct().Count(), "ids must be unique");
            Assert.AreEqual(ItemCatalog.Count, ItemCatalog.All.Select(b => b.Name).Distinct().Count(), "names must be unique");
        }

        [Test]
        public void Catalog_EverySlotHasBasesAndLegendaries()
        {
            foreach (EquipSlot slot in Enum.GetValues(typeof(EquipSlot)))
            {
                Assert.IsTrue(ItemCatalog.Bases.Any(b => b.Slot == slot && b.MinFloor == 1), $"{slot} needs a floor-1 base");
                Assert.IsTrue(ItemCatalog.Legendaries.Any(b => b.Slot == slot), $"{slot} needs a legendary");
            }
            for (int floor = 1; floor <= 10; floor++)
                Assert.GreaterOrEqual(ItemCatalog.Legendaries.Count(b => b.MinFloor == floor), 4, $"floor {floor} legendaries");
        }

        [Test]
        public void Affixes_HaveUniqueIdsAndBothKinds()
        {
            Assert.GreaterOrEqual(ItemCatalog.Affixes.Count, 20);
            Assert.AreEqual(ItemCatalog.Affixes.Count, ItemCatalog.Affixes.Select(a => a.Id).Distinct().Count());
            Assert.IsTrue(ItemCatalog.Affixes.Any(a => a.IsPrefix));
            Assert.IsTrue(ItemCatalog.Affixes.Any(a => !a.IsPrefix));
        }

        [Test]
        public void Roll_IsDeterministicForASeed()
        {
            var a = ItemGenerator.Roll(new Random(42), 3);
            var b = ItemGenerator.Roll(new Random(42), 3);
            Assert.AreEqual(a.BaseId, b.BaseId);
            Assert.AreEqual(a.Rarity, b.Rarity);
            Assert.AreEqual(a.Name, b.Name);
            Assert.AreEqual(a.Power, b.Power);
        }

        [Test]
        public void Roll_SameBaseDifferentSeeds_GiveDifferentStats()
        {
            var item = ItemCatalog.Get("crypt_longsword");
            var powers = Enumerable.Range(0, 30).Select(s => ItemGenerator.Create(item, Rarity.Rare, 4, new Random(s)).Power).Distinct().Count();
            Assert.Greater(powers, 3, "stats must be procedurally random");
        }

        [Test]
        public void AffixCount_MatchesRarity()
        {
            var item = ItemCatalog.Get("copper_ring");
            foreach (Rarity r in new[] { Rarity.Common, Rarity.Uncommon, Rarity.Rare, Rarity.Epic })
                Assert.AreEqual(RarityInfo.AffixCount(r), ItemGenerator.Create(item, r, 1, new Random(7)).AffixIds.Count, r.ToString());
        }

        [Test]
        public void DeeperFloors_RollStrongerItems()
        {
            var item = ItemCatalog.Get("riveted_mail");
            double shallow = Enumerable.Range(0, 50).Average(s => ItemGenerator.Create(item, Rarity.Common, 1, new Random(s)).Power);
            double deep = Enumerable.Range(0, 50).Average(s => ItemGenerator.Create(item, Rarity.Common, 10, new Random(s)).Power);
            Assert.Greater(deep, shallow * 1.8);
        }

        [Test]
        public void RarityWeights_ShiftUpWithDepthAndLuck()
        {
            double Avg(int floor, float luck) => Enumerable.Range(0, 4000).Average(s => (int)ItemGenerator.RollRarity(new Random(s), floor, luck));
            Assert.Greater(Avg(10, 0f), Avg(1, 0f));
            Assert.Greater(Avg(1, 1f), Avg(1, 0f));
        }

        [Test]
        public void Legendaries_KeepTheirNameAndOnlyDropAtTheirDepth()
        {
            var leg = ItemCatalog.Get("dawnbreaker");
            var rolled = ItemGenerator.Create(leg, Rarity.Common, 10, new Random(1));
            Assert.AreEqual(Rarity.Legendary, rolled.Rarity);
            Assert.AreEqual("Dawnbreaker", rolled.Name);

            for (int s = 0; s < 300; s++)
            {
                var item = ItemGenerator.Roll(new Random(s), 1, 5f);
                if (item.Rarity == Rarity.Legendary) Assert.AreEqual(1, item.Base.MinFloor, item.Name);
            }
        }

        [Test]
        public void Names_IncludeAffixes()
        {
            var item = ItemGenerator.Create(ItemCatalog.Get("rusted_blade"), Rarity.Epic, 2, new Random(3));
            StringAssert.Contains("Rusted Blade", item.Name);
            Assert.AreNotEqual("Rusted Blade", item.Name);
        }

        [Test]
        public void Describe_ListsNonZeroStatsOnly()
        {
            var lines = ItemInstance.Describe(new Oathdeep.Data.StatBlock(0, 5, 0, 0.1f, 0f, 0f), new ItemSpecials(0f, 0.1f, 0f, 0));
            CollectionAssert.AreEquivalent(new[] { "+5 Attack", "+10% Crit chance", "+10% Skill haste" }, lines);
        }
    }
}
