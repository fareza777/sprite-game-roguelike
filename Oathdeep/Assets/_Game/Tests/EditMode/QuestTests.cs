using System.Linq;
using NUnit.Framework;
using Oathdeep.Core;

namespace Oathdeep.Tests
{
    public class QuestTests
    {
        [Test]
        public void Catalog_HasTwelveSideQuestsAndTwelveChapters()
        {
            Assert.AreEqual(12, QuestCatalog.Side.Count);
            Assert.AreEqual(12, QuestCatalog.Side.Select(q => q.Id).Distinct().Count());
            Assert.IsTrue(QuestCatalog.Side.All(q => q.Goal > 0 && !string.IsNullOrEmpty(q.Reward.Describe())));
            Assert.AreEqual(12, QuestCatalog.Main.Count, "prologue + 10 chapters + epilogue");
        }

        [Test]
        public void Advance_CompletesOnceAndCapsAtGoal()
        {
            var save = new SaveData();
            var q = QuestCatalog.Get("first_blood");
            Assert.IsFalse(QuestCatalog.Advance(save, q, 30));
            Assert.IsTrue(QuestCatalog.Advance(save, q, 30));
            Assert.AreEqual(50, save.Quest(q.Id).progress);
            Assert.IsFalse(QuestCatalog.Advance(save, q, 5), "already complete");
        }

        [Test]
        public void SetProgress_TracksTotals()
        {
            var save = new SaveData();
            var q = QuestCatalog.Get("legend_seeker");
            QuestCatalog.SetProgress(save, q, 3);
            Assert.AreEqual(3, save.Quest(q.Id).progress);
            Assert.IsTrue(QuestCatalog.SetProgress(save, q, 5));
        }

        [Test]
        public void Claim_AddsPermanentBonusOnlyOnce()
        {
            var save = new SaveData();
            var q = QuestCatalog.Get("choir_breaker");
            Assert.IsFalse(QuestCatalog.Claim(save, q), "not complete yet");
            QuestCatalog.Advance(save, q, q.Goal);
            Assert.IsTrue(QuestCatalog.Claim(save, q));
            Assert.AreEqual(3, save.meta.attack);
            Assert.IsFalse(QuestCatalog.Claim(save, q));
            Assert.AreEqual(3, save.meta.attack);
        }

        [Test]
        public void MainChapters_FollowTheSave()
        {
            var save = new SaveData();
            var ch1 = QuestCatalog.Main[1];
            Assert.IsFalse(QuestCatalog.IsDone(ch1, save));
            save.UnlockCodex("bone_warden");
            Assert.IsTrue(QuestCatalog.IsDone(ch1, save));
            Assert.IsFalse(QuestCatalog.IsDone(QuestCatalog.Main.Last(), save));
            save.victories = 1;
            Assert.IsTrue(QuestCatalog.IsDone(QuestCatalog.Main.Last(), save));
        }

        [Test]
        public void MetaBonuses_SurviveSaveRoundTrip()
        {
            var save = new SaveData();
            save.meta.health = 15;
            save.meta.crit = 0.03f;
            save.heroesAtB3.Add("wren");
            var loaded = SaveSystem.FromJson(SaveSystem.ToJson(save));
            Assert.AreEqual(15, loaded.meta.health);
            Assert.AreEqual(0.03f, loaded.meta.crit, 1e-5f);
            CollectionAssert.Contains(loaded.heroesAtB3, "wren");
        }
    }
}
