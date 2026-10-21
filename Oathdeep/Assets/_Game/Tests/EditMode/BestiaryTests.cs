using System.Linq;
using NUnit.Framework;
using Oathdeep.Data;
using Oathdeep.Enemies;

namespace Oathdeep.Tests
{
    public class BestiaryTests
    {
        [Test]
        public void HasFortyUniqueCreatures()
        {
            Assert.AreEqual(40, Bestiary.All.Count);
            Assert.AreEqual(40, Bestiary.All.Select(e => e.Id).Distinct().Count());
            Assert.AreEqual(40, Bestiary.All.Select(e => e.Name).Distinct().Count());
        }

        [Test]
        public void EveryFloor_HasThreeRegularsAndOneBoss([Values(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)] int floor)
        {
            var here = Bestiary.OnFloor(floor).ToList();
            Assert.AreEqual(3, here.Count(e => !e.IsBoss));
            Assert.AreEqual(1, here.Count(e => e.IsBoss));
            Assert.IsNotNull(Bestiary.BossOf(floor));
        }

        [Test]
        public void EveryEntry_HasLoreAndPositiveStats()
        {
            foreach (var e in Bestiary.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(e.Lore), e.Id);
                Assert.Greater(e.Stats.MaxHealth, 0, e.Id);
                Assert.Greater(e.Stats.Attack, 0, e.Id);
                Assert.Greater(e.Stats.MoveSpeed, 0f, e.Id);
            }
        }

        [Test]
        public void EveryNewBoss_HasAPatternWithPhaseTwoMoves()
        {
            foreach (var boss in Bestiary.All.Where(e => e.IsBoss && e.Id != "bone_warden"))
            {
                var moves = BossPatterns.For(boss.Id);
                Assert.IsTrue(BossPatterns.Bosses.Contains(boss.Id), boss.Id + " needs its own pattern");
                Assert.IsTrue(moves.Any(m => !m.Phase2Only), boss.Id);
                Assert.IsTrue(moves.Any(m => m.Phase2Only), boss.Id);
                Assert.IsTrue(moves.All(m => m.Weight > 0f && m.Windup > 0f), boss.Id);
            }
        }

        [Test]
        public void PatternPick_RespectsPhaseAndAvoidsRepeats()
        {
            var moves = BossPatterns.For("horologist");
            var rng = new System.Random(3);
            for (int i = 0; i < 200; i++)
            {
                var m = BossPatterns.Pick(moves, false, rng, BossMove.Radial);
                Assert.IsFalse(m.Phase2Only);
                Assert.AreNotEqual(BossMove.Radial, m.Move);
            }
        }
    }
}
