using NUnit.Framework;
using Oathdeep.Data;

namespace Oathdeep.Tests
{
    public class StatBlockTests
    {
        [Test]
        public void Add_SumsEveryField()
        {
            var a = new StatBlock(maxHealth: 100, attack: 10, defense: 5, critChance: 0.05f, critMultiplier: 1.5f, moveSpeed: 4f);
            var b = new StatBlock(maxHealth: 20, attack: 3, defense: 2, critChance: 0.1f, critMultiplier: 0.25f, moveSpeed: 0.5f);

            var sum = a + b;

            Assert.AreEqual(120, sum.MaxHealth);
            Assert.AreEqual(13, sum.Attack);
            Assert.AreEqual(7, sum.Defense);
            Assert.AreEqual(0.15f, sum.CritChance, 1e-5f);
            Assert.AreEqual(1.75f, sum.CritMultiplier, 1e-5f);
            Assert.AreEqual(4.5f, sum.MoveSpeed, 1e-5f);
        }

        [Test]
        public void Add_DoesNotMutateOperands()
        {
            var a = new StatBlock(maxHealth: 100, attack: 10, defense: 5, critChance: 0f, critMultiplier: 1.5f, moveSpeed: 4f);
            var b = new StatBlock(maxHealth: 1, attack: 1, defense: 1, critChance: 0f, critMultiplier: 0f, moveSpeed: 0f);

            _ = a + b;

            Assert.AreEqual(100, a.MaxHealth);
            Assert.AreEqual(1, b.MaxHealth);
        }

        [Test]
        public void Clamped_LimitsCritChanceAndKeepsHealthPositive()
        {
            var s = new StatBlock(maxHealth: -5, attack: -3, defense: -1, critChance: 1.7f, critMultiplier: 0.5f, moveSpeed: -1f).Clamped();

            Assert.AreEqual(1, s.MaxHealth);
            Assert.AreEqual(0, s.Attack);
            Assert.AreEqual(0, s.Defense);
            Assert.AreEqual(1f, s.CritChance, 1e-5f);
            Assert.AreEqual(1f, s.CritMultiplier, 1e-5f);
            Assert.AreEqual(0.5f, s.MoveSpeed, 1e-5f);
        }
    }
}
