using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Oathdeep.Player;

namespace Oathdeep.Tests
{
    public class TalentTests
    {
        [Test]
        public void EveryTalent_IsDescribed()
        {
            foreach (Talent t in System.Enum.GetValues(typeof(Talent)))
            {
                var info = TalentCatalog.Info(t);
                Assert.IsFalse(string.IsNullOrEmpty(info.Name), t.ToString());
                Assert.IsFalse(string.IsNullOrEmpty(info.Text), t.ToString());
            }
            Assert.AreEqual(TalentCatalog.All.Count, TalentCatalog.All.Select(i => i.Name).Distinct().Count());
        }

        [Test]
        public void Draw_NeverOffersATalentAlreadyOwned()
        {
            var owned = new HashSet<Talent> { Talent.EmberTrail, Talent.ChainSpark };
            var rng = new System.Random(3);
            for (int i = 0; i < 200; i++)
            {
                var t = TalentCatalog.Draw(rng, owned);
                Assert.IsTrue(t.HasValue);
                Assert.IsFalse(owned.Contains(t.Value));
            }
        }

        [Test]
        public void Draw_IsEmptyOnceEverythingIsOwned()
        {
            var all = new HashSet<Talent>(TalentCatalog.All.Select(i => i.Id));
            Assert.IsNull(TalentCatalog.Draw(new System.Random(1), all));
        }

        [Test]
        public void ChainSpark_FiresOnEveryThirdHit()
        {
            var fired = Enumerable.Range(1, 9).Where(TalentCatalog.ChainSparkFires).ToArray();
            CollectionAssert.AreEqual(new[] { 3, 6, 9 }, fired);
        }

        [Test]
        public void SecondWind_TriggersBelowAQuarter()
        {
            Assert.IsTrue(TalentCatalog.SecondWindDue(24, 100, false));
            Assert.IsFalse(TalentCatalog.SecondWindDue(25, 100, false));
            Assert.IsFalse(TalentCatalog.SecondWindDue(10, 100, true), "once per floor");
            Assert.IsFalse(TalentCatalog.SecondWindDue(0, 100, false), "not when already dead");
        }
    }
}
