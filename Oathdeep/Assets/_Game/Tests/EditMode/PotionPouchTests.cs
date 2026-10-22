using NUnit.Framework;
using Oathdeep.Player;

namespace Oathdeep.Tests
{
    public class PotionPouchTests
    {
        [Test]
        public void Pickup_GoesIntoThePouch_UntilItIsFull()
        {
            var pouch = new PotionPouch(1, 3);
            Assert.AreEqual(PotionPickup.Stored, pouch.Pickup(hpFull: true));
            Assert.AreEqual(PotionPickup.Stored, pouch.Pickup(hpFull: true));
            Assert.AreEqual(3, pouch.Count);
        }

        [Test]
        public void FullPouch_DrinksOnTheSpot_WhenHurt()
        {
            var pouch = new PotionPouch(3, 3);
            Assert.AreEqual(PotionPickup.DrinkNow, pouch.Pickup(hpFull: false));
            Assert.AreEqual(3, pouch.Count);
        }

        // Full health and a full pouch: the potion stays on the floor and stops chasing the hero.
        [Test]
        public void FullPouch_AtFullHealth_LeavesThePotion()
        {
            var pouch = new PotionPouch(3, 3);
            Assert.AreEqual(PotionPickup.Leave, pouch.Pickup(hpFull: true));
            Assert.IsFalse(pouch.WantsMore(hpFull: true));
            Assert.IsTrue(pouch.WantsMore(hpFull: false));
        }

        [Test]
        public void Drinking_UsesOne_OnlyWhenHurt()
        {
            var pouch = new PotionPouch(2, 3);
            Assert.IsFalse(pouch.TryDrink(hpFull: true), "no point drinking at full health");
            Assert.AreEqual(2, pouch.Count);
            Assert.IsTrue(pouch.TryDrink(hpFull: false));
            Assert.AreEqual(1, pouch.Count);
            pouch.TryDrink(false);
            Assert.IsFalse(pouch.TryDrink(false), "empty");
        }

        [Test]
        public void Heal_IsAShareOfMaxHealth()
        {
            Assert.AreEqual(35, PotionPouch.HealFor(100));
            Assert.AreEqual(140, PotionPouch.HealFor(400));
        }
    }
}
