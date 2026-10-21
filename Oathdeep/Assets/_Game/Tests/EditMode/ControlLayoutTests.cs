using NUnit.Framework;
using Oathdeep.UI;
using UnityEngine;

namespace Oathdeep.Tests
{
    public class ControlLayoutTests
    {
        const float RefHeight = 640f;

        static Rect Box(ControlLayout.Slot s) => new Rect(s.Position - Vector2.one * s.Size / 2f, Vector2.one * s.Size);

        [Test]
        public void ActionButtons_DoNotOverlap([Values(false, true)] bool leftHanded)
        {
            var l = ControlLayout.Compute(leftHanded);
            Assert.IsFalse(Box(l.Attack).Overlaps(Box(l.Dodge)));
            Assert.IsFalse(Box(l.Attack).Overlaps(Box(l.Skill)));
            Assert.IsFalse(Box(l.Dodge).Overlaps(Box(l.Skill)));
        }

        // The canvas matches width and height halfway, so a 20:9 phone (1080x2400) is only ~322 units wide.
        [Test]
        public void StickAtRest_ClearsEveryActionButton([Values(false, true)] bool leftHanded, [Values(300f, 322f, 360f, 420f)] float width)
        {
            var l = ControlLayout.Compute(leftHanded);
            var stickCentre = new Vector2(l.MoveZoneMin.x * width, 0f) + ControlLayout.StickRest;
            var stick = new Rect(stickCentre - Vector2.one * ControlLayout.StickSize / 2f, Vector2.one * ControlLayout.StickSize);
            var corner = new Vector2(leftHanded ? 0f : width, 0f);
            foreach (var (name, slot) in new[] { ("Attack", l.Attack), ("Dodge", l.Dodge), ("Skill", l.Skill) })
            {
                var button = Box(slot);
                button.position += corner;
                Assert.IsFalse(stick.Overlaps(button), $"{name} overlaps the joystick at width {width}");
            }
        }

        [Test]
        public void ActionButtons_StayInBottomFortyPercent([Values(false, true)] bool leftHanded)
        {
            var l = ControlLayout.Compute(leftHanded);
            foreach (var s in new[] { l.Attack, l.Dodge, l.Skill })
                Assert.LessOrEqual(s.Position.y + s.Size / 2f, RefHeight * 0.4f);
        }

        [Test]
        public void LeftHanded_MirrorsHorizontally()
        {
            var r = ControlLayout.Compute(false);
            var l = ControlLayout.Compute(true);
            Assert.AreEqual(-r.Attack.Position.x, l.Attack.Position.x, 1e-4f);
            Assert.AreEqual(r.Attack.Position.y, l.Attack.Position.y, 1e-4f);
            Assert.AreEqual(1f - r.MoveZoneMax.x, l.MoveZoneMin.x, 1e-4f);
        }

        [Test]
        public void FloatingStick_IsKeptAwayFromZoneEdges()
        {
            var zone = new Vector2(180f, 288f);
            var p = ControlLayout.ClampStick(new Vector2(3f, 2f), zone, 112f);
            Assert.GreaterOrEqual(p.x, 56f + ControlLayout.EdgeMargin - 1e-4f);
            Assert.GreaterOrEqual(p.y, 56f + ControlLayout.EdgeMargin - 1e-4f);
            var q = ControlLayout.ClampStick(new Vector2(500f, 500f), zone, 112f);
            Assert.LessOrEqual(q.x, zone.x + 1e-4f);
            Assert.LessOrEqual(q.y, zone.y + 1e-4f);
        }

        [Test]
        public void FloatingStick_AppearsWhereTheThumbLands_AcrossTheWholeZone()
        {
            var zone = new Vector2(180f, 288f);
            // Used to be clamped to x <= 124 (zone width - stick radius), which made "floating" feel fixed.
            var p = ControlLayout.ClampStick(new Vector2(160f, 140f), zone, 112f);
            Assert.AreEqual(160f, p.x, 1e-4f);
            Assert.AreEqual(140f, p.y, 1e-4f);
        }

        [Test]
        public void FollowStick_DragsTheBaseOnlyBeyondTheRadius()
        {
            var basePos = new Vector2(100f, 100f);
            Assert.AreEqual(basePos, ControlLayout.FollowStick(basePos, new Vector2(30f, 0f), 56f));
            var moved = ControlLayout.FollowStick(basePos, new Vector2(80f, 0f), 56f);
            Assert.AreEqual(124f, moved.x, 1e-4f);
            Assert.AreEqual(100f, moved.y, 1e-4f);
        }
    }
}
