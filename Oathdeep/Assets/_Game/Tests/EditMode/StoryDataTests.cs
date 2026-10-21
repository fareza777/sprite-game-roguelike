using System.IO;
using NUnit.Framework;
using Oathdeep.Core;

namespace Oathdeep.Tests
{
    public class StoryDataTests
    {
        static StoryData Load() => StoryData.Parse(File.ReadAllText("Assets/_Game/Story/story.json"));

        [Test]
        public void Story_HasFullPrologueEndingAndTenFloors()
        {
            var story = Load();
            Assert.AreEqual(6, story.prologue.Length);
            Assert.AreEqual(4, story.ending.Length);
            Assert.AreEqual(10, story.floors.Length);
        }

        [Test]
        public void EveryFloor_HasBiomeVerseBossAndLines()
        {
            var story = Load();
            for (int i = 0; i < story.floors.Length; i++)
            {
                var f = story.floors[i];
                Assert.AreEqual(i + 1, f.floor);
                Assert.IsFalse(string.IsNullOrWhiteSpace(f.biome), $"floor {f.floor} biome");
                Assert.IsFalse(string.IsNullOrWhiteSpace(f.verse), $"floor {f.floor} verse");
                Assert.IsFalse(string.IsNullOrWhiteSpace(f.boss), $"floor {f.floor} boss");
                Assert.IsFalse(string.IsNullOrWhiteSpace(f.entry), $"floor {f.floor} entry");
                Assert.IsFalse(string.IsNullOrWhiteSpace(f.bossLine), $"floor {f.floor} boss line");
            }
        }

        [Test]
        public void Floor_LooksUpByNumberAndClamps()
        {
            var story = Load();
            Assert.AreEqual("Sunken Crypt", story.Floor(1).biome);
            Assert.AreEqual("Heart of Dusk", story.Floor(10).biome);
            Assert.AreEqual("Heart of Dusk", story.Floor(99).biome);
            Assert.AreEqual("Sunken Crypt", story.Floor(0).biome);
        }

        [Test]
        public void EveryNarratedLine_HasAVoiceClipOnDisk()
        {
            var story = Load();
            foreach (var line in story.prologue) Assert.IsTrue(File.Exists($"Assets/_Game/Audio/VO/vo_{line.id}.wav"), line.id);
            foreach (var line in story.ending) Assert.IsTrue(File.Exists($"Assets/_Game/Audio/VO/vo_{line.id}.wav"), line.id);
            foreach (var f in story.floors) Assert.IsTrue(File.Exists($"Assets/_Game/Audio/VO/vo_{f.id}.wav"), f.id);
        }
    }
}
