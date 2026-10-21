using NUnit.Framework;
using Oathdeep.Combat;

namespace Oathdeep.Tests
{
    public class DamageCalculatorTests
    {
        [Test]
        public void ZeroDefense_DealsFullAttack()
        {
            var result = DamageCalculator.Compute(attack: 10, defense: 0, multiplier: 1f, critChance: 0f, critMultiplier: 2f, roll: 0.5f);
            Assert.AreEqual(10, result.Amount);
            Assert.IsFalse(result.IsCrit);
        }

        [Test]
        public void Defense_ReducesDamageWithDiminishingReturns()
        {
            var result = DamageCalculator.Compute(attack: 20, defense: 100, multiplier: 1f, critChance: 0f, critMultiplier: 2f, roll: 0.5f);
            Assert.AreEqual(10, result.Amount);
        }

        [Test]
        public void RollBelowCritChance_IsCritAndMultiplies()
        {
            var result = DamageCalculator.Compute(attack: 10, defense: 0, multiplier: 1f, critChance: 0.25f, critMultiplier: 2f, roll: 0.1f);
            Assert.IsTrue(result.IsCrit);
            Assert.AreEqual(20, result.Amount);
        }

        [Test]
        public void RollAtOrAboveCritChance_IsNotCrit()
        {
            var result = DamageCalculator.Compute(attack: 10, defense: 0, multiplier: 1f, critChance: 0.25f, critMultiplier: 2f, roll: 0.25f);
            Assert.IsFalse(result.IsCrit);
        }

        [Test]
        public void SkillMultiplier_ScalesDamage()
        {
            var result = DamageCalculator.Compute(attack: 10, defense: 0, multiplier: 1.5f, critChance: 0f, critMultiplier: 2f, roll: 0.5f);
            Assert.AreEqual(15, result.Amount);
        }

        [Test]
        public void Damage_IsNeverBelowOne()
        {
            var result = DamageCalculator.Compute(attack: 1, defense: 10000, multiplier: 0.1f, critChance: 0f, critMultiplier: 2f, roll: 0.5f);
            Assert.AreEqual(1, result.Amount);
        }

        [Test]
        public void NegativeDefense_IsTreatedAsZero()
        {
            var result = DamageCalculator.Compute(attack: 10, defense: -50, multiplier: 1f, critChance: 0f, critMultiplier: 2f, roll: 0.5f);
            Assert.AreEqual(10, result.Amount);
        }
    }
}
