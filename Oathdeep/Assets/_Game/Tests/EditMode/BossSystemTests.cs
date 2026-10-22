using NUnit.Framework;
using Oathdeep.Combat;
using Oathdeep.Enemies;
using UnityEngine;

namespace Oathdeep.Tests
{
    public class BossSystemTests
    {
        // ---------------- phases ----------------

        [Test]
        public void Phases_SplitHealthIntoThirds()
        {
            Assert.AreEqual(1, BossPhases.PhaseFor(900, 900));
            Assert.AreEqual(1, BossPhases.PhaseFor(601, 900));
            Assert.AreEqual(2, BossPhases.PhaseFor(600, 900));
            Assert.AreEqual(2, BossPhases.PhaseFor(301, 900));
            Assert.AreEqual(3, BossPhases.PhaseFor(300, 900));
            Assert.AreEqual(3, BossPhases.PhaseFor(0, 900));
        }

        // ---------------- break gauge ----------------

        [Test]
        public void BreakGauge_BreaksWhenEnoughDamageLands()
        {
            var g = new BreakGauge(100f);
            Assert.IsFalse(g.AddDamage(60f));
            Assert.AreEqual(0.6f, g.Fraction, 1e-4f);
            Assert.IsTrue(g.AddDamage(40f));
            Assert.IsTrue(g.Broken);
        }

        [Test]
        public void BreakGauge_RecoversAfterTheStagger_AndNeedsMoreNextTime()
        {
            var g = new BreakGauge(100f, duration: 3f);
            g.AddDamage(100f);
            Assert.IsFalse(g.AddDamage(500f), "no damage counts while broken");
            g.Tick(3.1f);
            Assert.IsFalse(g.Broken);
            Assert.AreEqual(0f, g.Fraction, 1e-4f);
            Assert.Greater(g.Capacity, 100f, "each break makes the next one harder");
        }

        [Test]
        public void BreakGauge_DrainsWhenThePlayerStopsHitting()
        {
            var g = new BreakGauge(100f, decayDelay: 2f, decayPerSecond: 0.2f);
            g.AddDamage(50f);
            g.Tick(1.5f);
            Assert.AreEqual(0.5f, g.Fraction, 1e-4f, "no drain inside the delay");
            g.Tick(1.5f);   // 1 s past the delay
            Assert.AreEqual(0.3f, g.Fraction, 1e-3f);
            g.Tick(10f);
            Assert.AreEqual(0f, g.Fraction, 1e-4f);
        }

        // ---------------- hazards ----------------

        [Test]
        public void HazardShapes_ContainPoints()
        {
            Assert.IsTrue(HazardShape.Circle(Vector2.zero, 2f).Contains(new Vector2(1.9f, 0f)));
            Assert.IsFalse(HazardShape.Circle(Vector2.zero, 2f).Contains(new Vector2(1.5f, 1.5f)));
            var box = HazardShape.Box(new Rect(0f, 0f, 4f, 2f));
            Assert.IsTrue(box.Contains(new Vector2(3.9f, 1.9f)));
            Assert.IsFalse(box.Contains(new Vector2(4.1f, 1f)));
        }

        [Test]
        public void MoveModifiers_TakeTheStrongestOfEachKind()
        {
            var m = MoveModifier.Combine(new[] { (HazardKind.Slow, 0.6f), (HazardKind.Slow, 0.4f), (HazardKind.Ice, 0.25f), (HazardKind.Burn, 5f) });
            Assert.AreEqual(0.4f, m.SpeedMultiplier, 1e-4f);
            Assert.AreEqual(0.25f, m.Traction, 1e-4f);
            var none = MoveModifier.Combine(new (HazardKind, float)[0]);
            Assert.AreEqual(1f, none.SpeedMultiplier);
            Assert.AreEqual(1f, none.Traction);
        }

        // ---------------- clock hand ----------------

        [Test]
        public void Beam_HitsAlongItsLengthOnly()
        {
            Assert.IsTrue(Beam.Hits(Vector2.zero, 0f, 8f, 0.5f, new Vector2(5f, 0.3f)));
            Assert.IsFalse(Beam.Hits(Vector2.zero, 0f, 8f, 0.5f, new Vector2(5f, 0.8f)));
            Assert.IsFalse(Beam.Hits(Vector2.zero, 0f, 8f, 0.5f, new Vector2(9f, 0f)), "past the tip");
            Assert.IsFalse(Beam.Hits(Vector2.zero, 0f, 8f, 0.5f, new Vector2(-2f, 0f)), "behind the pivot");
            Assert.IsTrue(Beam.Hits(Vector2.zero, 90f, 8f, 0.5f, new Vector2(0.2f, 6f)));
        }
    }
}
