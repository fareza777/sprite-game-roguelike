using NUnit.Framework;
using Oathdeep.Core;

namespace Oathdeep.Tests
{
    public class AdPolicyTests
    {
        static bool Show(int floor, int floorsSince = 9, float secondsSince = 999f, float sinceRewarded = 999f, bool removed = false) =>
            AdPolicy.InterstitialDue(floor, floorsSince, secondsSince, sinceRewarded, removed);

        [Test]
        public void NoInterstitial_BeforeTheSecondFloor()
        {
            Assert.IsFalse(Show(1));
            Assert.IsTrue(Show(2));
        }

        [Test]
        public void Interstitials_AreSpacedOut()
        {
            Assert.IsFalse(Show(5, floorsSince: 1), "at most every second floor");
            Assert.IsFalse(Show(5, secondsSince: 90f), "at least two minutes apart");
            Assert.IsTrue(Show(5, floorsSince: 2, secondsSince: 121f));
        }

        [Test]
        public void NoInterstitial_RightAfterARewardedAd()
        {
            Assert.IsFalse(Show(6, sinceRewarded: 30f));
        }

        [Test]
        public void RemoveAds_StopsInterstitialsAndBanners()
        {
            Assert.IsFalse(Show(6, removed: true));
            Assert.IsFalse(AdPolicy.BannerAllowed(onMenu: true, removed: true));
        }

        [Test]
        public void Banner_ShowsOnMenusOnly()
        {
            Assert.IsTrue(AdPolicy.BannerAllowed(onMenu: true, removed: false));
            Assert.IsFalse(AdPolicy.BannerAllowed(onMenu: false, removed: false), "never over the touch controls");
        }

        [Test]
        public void Revive_IsOfferedOncePerRun()
        {
            Assert.IsTrue(AdPolicy.ReviveOffered(usedThisRun: false, rewardedReady: true));
            Assert.IsFalse(AdPolicy.ReviveOffered(usedThisRun: true, rewardedReady: true));
            Assert.IsFalse(AdPolicy.ReviveOffered(usedThisRun: false, rewardedReady: false));
        }
    }
}
