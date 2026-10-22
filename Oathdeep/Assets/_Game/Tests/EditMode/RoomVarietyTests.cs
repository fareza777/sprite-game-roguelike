using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Oathdeep.Combat;
using Oathdeep.Data;
using Oathdeep.Dungeon;
using Oathdeep.Enemies;
using UnityEngine;

namespace Oathdeep.Tests
{
    public class RoomVarietyTests
    {
        static IEnumerable<double> Rolls => Enumerable.Range(0, 100).Select(i => i / 100.0);

        [Test]
        public void FirstFloor_StaysSimple()
        {
            var seen = Rolls.Select(r => RoomChallenges.Pick(1, r)).Distinct().ToList();
            CollectionAssert.IsSubsetOf(seen, new[] { RoomChallenge.Waves, RoomChallenge.Nests });
            Assert.Contains(RoomChallenge.Waves, seen);
        }

        [Test]
        public void DeeperFloors_UseEveryKindOfRoom([Values(2, 5, 10)] int floor)
        {
            var seen = Rolls.Select(r => RoomChallenges.Pick(floor, r)).Distinct().ToList();
            Assert.AreEqual(4, seen.Count);
            int waves = Rolls.Count(r => RoomChallenges.Pick(floor, r) == RoomChallenge.Waves);
            Assert.Greater(waves, 40, "plain waves stay the most common room");
        }

        [Test]
        public void SurviveRooms_LastLongerDeeper()
        {
            Assert.Greater(RoomChallenges.SurviveSeconds(10), RoomChallenges.SurviveSeconds(2));
            Assert.LessOrEqual(RoomChallenges.SurviveSeconds(10), 35f);
        }

        [Test]
        public void EachBiome_HasItsOwnTerrain()
        {
            Assert.IsNull(RoomChallenges.TerrainFor(1), "the crypt keeps its spike traps only");
            var kinds = Enumerable.Range(2, 9).Select(f => RoomChallenges.TerrainFor(f)).ToList();
            Assert.IsTrue(kinds.All(k => k.HasValue));
            Assert.AreEqual(HazardKind.Slow, RoomChallenges.TerrainFor(3).Value.Kind, "cistern puddles slow");
            Assert.AreEqual(HazardKind.Burn, RoomChallenges.TerrainFor(4).Value.Kind, "forge lava burns");
            Assert.AreEqual(HazardKind.Ice, RoomChallenges.TerrainFor(5).Value.Kind, "frost ice slides");
        }

        [Test]
        public void EliteTraits_ChangeTheRightStat()
        {
            var b = new StatBlock(100, 10, 2, 0.05f, 1.5f, 2f);
            var swift = EliteTraits.Apply(b, EliteTrait.Swift);
            Assert.AreEqual(2.8f, swift.Stats.MoveSpeed, 1e-4f);
            var giant = EliteTraits.Apply(b, EliteTrait.Giant);
            Assert.AreEqual(150, giant.Stats.MaxHealth);
            Assert.Greater(giant.Size, 1f);
            var armored = EliteTraits.Apply(b, EliteTrait.Armored);
            Assert.Less(armored.DamageTaken, 1f);
            Assert.AreEqual(b.MaxHealth, armored.Stats.MaxHealth);
        }

        [Test]
        public void EliteTraits_AllAppear()
        {
            var seen = Rolls.Select(EliteTraits.Pick).Distinct().Count();
            Assert.AreEqual(System.Enum.GetValues(typeof(EliteTrait)).Length, seen);
        }
    }
}
