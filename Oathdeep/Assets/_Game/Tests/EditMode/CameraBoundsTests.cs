using NUnit.Framework;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Tests
{
    public class CameraBoundsTests
    {
        const float HalfW = 6.75f, HalfH = 12f;   // 216x384 reference at 16 PPU

        [Test]
        public void NoBounds_FollowsTargetFreely()
        {
            var b = new CameraBounds();
            Assert.AreEqual(new Vector2(-20f, 3f), b.Clamp(new Vector2(-20f, 3f), HalfW, HalfH));
        }

        [Test]
        public void RoomNarrowerThanView_CentresOnRoom()
        {
            var b = new CameraBounds();
            b.Set(new Rect(0f, 0f, 13f, 30f));
            Assert.AreEqual(6.5f, b.Clamp(new Vector2(-14f, 15f), HalfW, HalfH).x, 1e-4f);
        }

        [Test]
        public void RoomWiderThanView_ClampsInsideEdges()
        {
            var b = new CameraBounds();
            b.Set(new Rect(0f, 0f, 30f, 30f));
            var p = b.Clamp(new Vector2(-5f, 40f), HalfW, HalfH);
            Assert.AreEqual(HalfW, p.x, 1e-4f);
            Assert.AreEqual(30f - HalfH, p.y, 1e-4f);
        }

        [Test]
        public void LeavingCurrentRoom_ReleasesClamp()
        {
            var room = new Rect(0f, 0f, 13f, 13f);
            var b = new CameraBounds();
            b.Set(room);
            b.Release(room);
            Assert.AreEqual(new Vector2(-14f, 0.5f), b.Clamp(new Vector2(-14f, 0.5f), HalfW, HalfH));
        }

        [Test]
        public void LeavingAnotherRoom_KeepsCurrentClamp()
        {
            var current = new Rect(0f, 0f, 13f, 13f);
            var b = new CameraBounds();
            b.Set(current);
            b.Release(new Rect(40f, 0f, 13f, 13f));
            Assert.IsTrue(b.Active);
        }
    }
}
