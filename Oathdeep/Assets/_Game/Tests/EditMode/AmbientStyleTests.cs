using NUnit.Framework;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Tests
{
    public class AmbientStyleTests
    {
        [Test]
        public void EveryFloor_HasItsOwnAir([Values(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)] int floor)
        {
            var s = AmbientStyles.For(floor);
            Assert.Greater(s.Count, 0);
            Assert.Greater(s.Alpha, 0f);
        }

        // The air should set a mood, never hide the fight: a few dozen faint motes at most.
        [Test]
        public void Motes_StaySparseAndFaint([Values(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)] int floor)
        {
            var s = AmbientStyles.For(floor);
            Assert.LessOrEqual(s.Count, 36);
            Assert.LessOrEqual(s.Alpha, 0.8f);
        }

        [Test]
        public void Floors_FeelDifferent()
        {
            var a = AmbientStyles.For(3).Velocity;   // cistern drips fall
            var b = AmbientStyles.For(4).Velocity;   // forge embers rise
            Assert.Less(a.y, 0f);
            Assert.Greater(b.y, 0f);
            Assert.Greater(Mathf.Abs(AmbientStyles.For(8).Velocity.x), Mathf.Abs(AmbientStyles.For(8).Velocity.y), "sand blows sideways");
        }

        [Test]
        public void Wrap_KeepsAMoteInsideTheView()
        {
            var view = new Rect(0f, 0f, 10f, 20f);
            Assert.AreEqual(new Vector2(0.5f, 5f), AmbientStyles.Wrap(new Vector2(10.5f, 5f), view));
            Assert.AreEqual(new Vector2(3f, 19f), AmbientStyles.Wrap(new Vector2(3f, -1f), view));
            Assert.AreEqual(new Vector2(4f, 4f), AmbientStyles.Wrap(new Vector2(4f, 4f), view));
        }
    }
}
