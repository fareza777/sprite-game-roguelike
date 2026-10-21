using NUnit.Framework;
using Oathdeep.UI;
using UnityEngine;

namespace Oathdeep.Tests
{
    public class ControlPlacementTests
    {
        static readonly Vector2 Phone = new Vector2(322f, 715f);   // a 20:9 phone's safe area in canvas units

        [Test]
        public void Defaults_MatchTheBuiltInLayout()
        {
            var right = ControlLayout.Compute(false);
            Assert.AreEqual(new Vector2(322f - 80f, 116f), ControlPlacement.DefaultCentre(Control.Attack, right, Phone));
            Assert.AreEqual(new Vector2(161f, 36f), ControlPlacement.DefaultCentre(Control.Dodge, right, Phone));
            Assert.AreEqual(ControlLayout.StickRest, ControlPlacement.DefaultCentre(Control.Stick, right, Phone));

            var left = ControlLayout.Compute(true);
            Assert.AreEqual(new Vector2(80f, 116f), ControlPlacement.DefaultCentre(Control.Attack, left, Phone));
            Assert.AreEqual(new Vector2(161f + 92f, 132f), ControlPlacement.DefaultCentre(Control.Stick, left, Phone));
        }

        [Test]
        public void Clamp_KeepsTheWholeControlOnScreen()
        {
            var c = ControlPlacement.Clamp(new Vector2(-50f, 900f), 100f, Phone);
            Assert.AreEqual(50f, c.x, 1e-4f);
            Assert.AreEqual(Phone.y - 50f, c.y, 1e-4f);
            var inside = new Vector2(150f, 200f);
            Assert.AreEqual(inside, ControlPlacement.Clamp(inside, 64f, Phone));
        }

        [Test]
        public void Normalized_RoundTrips_AndScalesToAnotherScreen()
        {
            var p = new Vector2(100f, 300f);
            var n = ControlPlacement.Normalize(p, Phone);
            Assert.AreEqual(p.x, ControlPlacement.Denormalize(n, Phone).x, 1e-3f);
            Assert.AreEqual(p.y, ControlPlacement.Denormalize(n, Phone).y, 1e-3f);
            var wide = ControlPlacement.Denormalize(n, new Vector2(360f, 640f));
            Assert.AreEqual(100f / 322f * 360f, wide.x, 1e-3f);
        }

        [Test]
        public void MoveZone_IsTheHalfThatHoldsTheStick()
        {
            var (minL, maxL) = ControlPlacement.MoveZoneFor(new Vector2(0.2f, 0.2f));
            Assert.AreEqual(0f, minL.x); Assert.AreEqual(0.5f, maxL.x);
            var (minR, maxR) = ControlPlacement.MoveZoneFor(new Vector2(0.8f, 0.2f));
            Assert.AreEqual(0.5f, minR.x); Assert.AreEqual(1f, maxR.x);
            Assert.AreEqual(0.45f, maxL.y, 1e-4f);
        }

        [Test]
        public void MoveZone_GrowsUpToReachAStickPlacedHigh()
        {
            var (_, max) = ControlPlacement.MoveZoneFor(new Vector2(0.3f, 0.6f));
            Assert.Greater(max.y, 0.6f);
            Assert.LessOrEqual(max.y, 1f);
        }

        [Test]
        public void StickRest_InsideItsZone_IsRelativeToTheZoneCorner()
        {
            var rest = ControlPlacement.RestInZone(new Vector2(0.75f, 0.1f), Vector2.one * 0.5f - Vector2.up * 0.5f, new Vector2(1f, 0.45f));
            Assert.AreEqual(0.5f, rest.x, 1e-4f);
            Assert.AreEqual(0.1f / 0.45f, rest.y, 1e-4f);
        }
    }
}
