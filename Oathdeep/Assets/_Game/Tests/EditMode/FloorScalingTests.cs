using NUnit.Framework;
using Oathdeep.Dungeon;
using Oathdeep.Loot;

namespace Oathdeep.Tests
{
    public class FloorScalingTests
    {
        [Test]
        public void FirstFloor_IsUnscaled()
        {
            Assert.AreEqual(1f, FloorConfig.StatScaleFor(1), 1e-5f);
        }

        [Test]
        public void EveryFloor_IsStrongerThanTheOneAbove()
        {
            for (int n = 2; n <= 10; n++)
                Assert.Greater(FloorConfig.StatScaleFor(n), FloorConfig.StatScaleFor(n - 1), $"floor {n}");
        }

        // Gear compounds (floor bonus x rarity x affixes x boons), so enemies must accelerate: a straight line let the
        // late floors get easier than the middle ones.
        [Test]
        public void EnemyGrowth_Accelerates()
        {
            for (int n = 3; n <= 10; n++)
            {
                float step = FloorConfig.StatScaleFor(n) - FloorConfig.StatScaleFor(n - 1);
                float before = FloorConfig.StatScaleFor(n - 1) - FloorConfig.StatScaleFor(n - 2);
                Assert.Greater(step, before, $"floor {n}");
            }
        }

        // Enemies must outgrow the gear a floor drops, so each floor asks more of the player than the last.
        [Test]
        public void Enemies_OutgrowTheGearEachFloorDrops()
        {
            for (int n = 2; n <= 10; n++)
            {
                float gear = 1f + ItemGenerator.FloorGrowth * (n - 1);
                float prevGear = 1f + ItemGenerator.FloorGrowth * (n - 2);
                float ratio = FloorConfig.StatScaleFor(n) / gear;
                float prevRatio = FloorConfig.StatScaleFor(n - 1) / prevGear;
                Assert.Greater(ratio, prevRatio, $"floor {n}");
            }
        }
    }
}
