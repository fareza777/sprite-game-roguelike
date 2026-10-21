using System;
using System.Linq;
using NUnit.Framework;
using Oathdeep.Player;

namespace Oathdeep.Tests
{
    public class HeroMovesTests
    {
        static readonly HeroId[] AllHeroes = (HeroId[])Enum.GetValues(typeof(HeroId));

        [Test]
        public void EveryHero_HasThreeHitComboWithActiveWindowInsideDuration()
        {
            foreach (var id in AllHeroes)
            {
                var moves = HeroMoves.For(id);
                Assert.AreEqual(3, moves.Combo.Length, id.ToString());
                foreach (var m in moves.Combo)
                {
                    Assert.Greater(m.ActiveStart, 0f, $"{id} {m.Anim}");
                    Assert.LessOrEqual(m.ActiveStart, m.ActiveEnd, $"{id} {m.Anim}");
                    Assert.Less(m.ActiveEnd, m.Duration, $"{id} {m.Anim}");
                    Assert.Greater(m.Spec.Multiplier, 0f, $"{id} {m.Anim}");
                }
            }
        }

        [Test]
        public void EveryHero_HasADistinctSkill()
        {
            var skills = AllHeroes.Select(id => HeroMoves.For(id).Skill).ToArray();
            CollectionAssert.AllItemsAreUnique(skills);
        }

        [Test]
        public void RangedMoves_FireAtLeastOneShotWithSpeed()
        {
            foreach (var id in AllHeroes)
                foreach (var m in HeroMoves.For(id).Combo.Where(m => m.IsRanged))
                {
                    Assert.GreaterOrEqual(m.ShotCount, 1, $"{id} {m.Anim}");
                    Assert.Greater(m.ShotSpeed, 0f, $"{id} {m.Anim}");
                }
        }

        [Test]
        public void RangedHeroes_ReachFurtherThanMeleeHeroes()
        {
            Assert.Greater(HeroMoves.Wren.AutoAimRange, HeroMoves.Aldric.AutoAimRange);
            Assert.Greater(HeroMoves.Morrow.AutoAimRange, HeroMoves.Brakka.AutoAimRange);
            Assert.IsTrue(HeroMoves.Wren.Combo.All(m => m.IsRanged));
            Assert.IsTrue(HeroMoves.Sable.Combo.All(m => !m.IsRanged));
        }

        [Test]
        public void SkillImpact_HappensBeforeTheSkillEnds()
        {
            foreach (var id in AllHeroes)
            {
                var moves = HeroMoves.For(id);
                Assert.Less(moves.SkillImpact, moves.SkillDuration, id.ToString());
                Assert.IsFalse(string.IsNullOrEmpty(moves.SkillAnim), id.ToString());
            }
        }

        [Test]
        public void Sable_IsTheFastestAttackerAndBrakkaTheSlowest()
        {
            float First(HeroMoves m) => m.Combo[0].Duration;
            Assert.IsTrue(AllHeroes.All(id => First(HeroMoves.Sable) <= First(HeroMoves.For(id))));
            Assert.IsTrue(AllHeroes.All(id => First(HeroMoves.Brakka) >= First(HeroMoves.For(id))));
        }
    }
}
