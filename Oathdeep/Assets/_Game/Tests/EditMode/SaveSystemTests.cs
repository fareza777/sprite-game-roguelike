using NUnit.Framework;
using Oathdeep.Core;

namespace Oathdeep.Tests
{
    public class SaveSystemTests
    {
        [Test]
        public void RoundTrip_PreservesProgressAndSettings()
        {
            var data = new SaveData();
            data.settings.music = 0.25f;
            data.settings.leftHanded = true;
            data.introSeen = true;
            data.bestFloor = 7;
            data.UnlockCodex("bone_warden");
            data.Quest("bones_for_pilgrim").progress = 3;

            var back = SaveSystem.FromJson(SaveSystem.ToJson(data));

            Assert.AreEqual(0.25f, back.settings.music, 1e-4f);
            Assert.IsTrue(back.settings.leftHanded);
            Assert.IsTrue(back.introSeen);
            Assert.AreEqual(7, back.bestFloor);
            Assert.IsTrue(back.IsCodexUnlocked("bone_warden"));
            Assert.AreEqual(3, back.Quest("bones_for_pilgrim").progress);
        }

        [Test]
        public void CorruptOrEmptyJson_FallsBackToDefaults()
        {
            Assert.AreEqual(0.8f, SaveSystem.FromJson("{not json").settings.music, 1e-4f);
            Assert.IsFalse(SaveSystem.FromJson("").introSeen);
            Assert.IsNotNull(SaveSystem.FromJson(null).codex);
        }

        [Test]
        public void UnlockCodex_ReportsFirstUnlockOnly()
        {
            var data = new SaveData();
            Assert.IsTrue(data.UnlockCodex("skeleton"));
            Assert.IsFalse(data.UnlockCodex("skeleton"));
            Assert.AreEqual(1, data.codex.Count);
        }

        [Test]
        public void Quest_IsCreatedOnceAndReused()
        {
            var data = new SaveData();
            data.Quest("light_lanterns").progress = 2;
            Assert.AreEqual(2, data.Quest("light_lanterns").progress);
            Assert.AreEqual(1, data.quests.Count);
        }

        [Test]
        public void OlderSaveWithoutNewFields_GetsDefaults()
        {
            var data = SaveSystem.FromJson("{\"version\":1,\"bestFloor\":2}");
            Assert.AreEqual(2, data.bestFloor);
            Assert.IsNotNull(data.settings);
            Assert.AreEqual(1f, data.settings.sfx, 1e-4f);
        }
    }
}
