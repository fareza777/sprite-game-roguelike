using NUnit.Framework;
using Oathdeep.UI;

namespace Oathdeep.Tests
{
    public class PointerHoldTests
    {
        static readonly (int id, bool pressed)[] NoTouches = new (int, bool)[0];

        [Test]
        public void KnownTouch_IsHeldOnlyWhileThatTouchIsPressed()
        {
            Assert.IsTrue(PointerHold.StillDown(3, new[] { (3, true) }, null));
            Assert.IsFalse(PointerHold.StillDown(3, new[] { (3, false) }, null));
            Assert.IsFalse(PointerHold.StillDown(3, new[] { (5, true) }, null), "another finger does not hold this one");
        }

        // A phone has no mouse. This used to count as "still pressed" forever, so a tap that reached the stick
        // without a touch id (the run-start tap right on the zone edge) left the hero walking on its own.
        [Test]
        public void UnknownPointer_OnAPhoneWithNoTouches_IsReleased()
        {
            Assert.IsFalse(PointerHold.StillDown(-1, NoTouches, null));
            Assert.IsFalse(PointerHold.StillDown(-1, new[] { (0, false) }, null));
        }

        [Test]
        public void UnknownPointer_IsHeldWhileAnyFingerIsDown()
        {
            Assert.IsTrue(PointerHold.StillDown(-1, new[] { (0, true) }, null));
        }

        [Test]
        public void Mouse_IsHeldWhileTheButtonIsDown()
        {
            Assert.IsTrue(PointerHold.StillDown(-1, NoTouches, true));
            Assert.IsFalse(PointerHold.StillDown(-1, NoTouches, false));
        }
    }
}
