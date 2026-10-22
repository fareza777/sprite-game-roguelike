using System;
using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>
    /// When ads may appear. Banners only on menus (never over the touch controls); interstitials between floors,
    /// spaced out and never straight after a rewarded ad; rewarded ads are always the player's choice.
    /// Remove Ads ends banners and interstitials; rewarded ads stay available because the player asks for them.
    /// </summary>
    public static class AdPolicy
    {
        public const int FirstInterstitialFloor = 2;
        public const int FloorsBetweenInterstitials = 2;
        public const float SecondsBetweenInterstitials = 120f;
        public const float QuietAfterRewarded = 90f;

        public static bool InterstitialDue(int floor, int floorsSinceLast, float secondsSinceLast, float secondsSinceRewarded, bool adsRemoved) =>
            !adsRemoved && floor >= FirstInterstitialFloor && floorsSinceLast >= FloorsBetweenInterstitials &&
            secondsSinceLast >= SecondsBetweenInterstitials && secondsSinceRewarded >= QuietAfterRewarded;

        public static bool BannerAllowed(bool onMenu, bool removed) => onMenu && !removed;

        public static bool ReviveOffered(bool usedThisRun, bool rewardedReady) => !usedThisRun && rewardedReady;
    }

    /// <summary>An ad network (AdMob in builds; nothing in the editor or tests).</summary>
    public interface IAdProvider
    {
        void Initialize();
        void SetBannerVisible(bool visible);
        bool InterstitialReady { get; }
        void ShowInterstitial(Action closed);
        bool RewardedReady { get; }
        /// <summary>`done(true)` only when the player watched to the reward.</summary>
        void ShowRewarded(Action<bool> done);
    }

    /// <summary>The store for the Remove Ads purchase (Unity IAP in builds).</summary>
    public interface IStoreProvider
    {
        void Initialize();
        bool Ready { get; }
        string RemoveAdsPrice { get; }
        void BuyRemoveAds(Action<bool> done);
        void Restore(Action<bool> done);
    }

    /// <summary>
    /// The game's single entry point for ads and the Remove Ads purchase. The SDK-specific providers live in the
    /// Oathdeep.Monetization assembly and register themselves at startup; until then everything is a quiet no-op.
    /// </summary>
    public static class Monetization
    {
        public const string RemoveAdsProductId = "com.fareza777.undersun.removeads";
        public const string RemoveAdsFallbackPrice = "$4.99";

        static IAdProvider ads = new NoAds();
        static IStoreProvider store = new NoStore();
        static bool onMenu;
        static int floorsSinceInterstitial = AdPolicy.FloorsBetweenInterstitials;
        static float lastInterstitialAt = -999f, lastRewardedAt = -999f;

        public static event Action Changed;

        public static bool AdsRemoved => SaveSystem.Current.adsRemoved;
        /// <summary>A full-screen ad is up: the app going to the background is the ad, not the player leaving.</summary>
        public static bool ShowingFullScreenAd { get; private set; }
        public static bool RewardedReady => ads.RewardedReady;
        public static bool StoreReady => store.Ready;
        public static string RemoveAdsPrice => string.IsNullOrEmpty(store.RemoveAdsPrice) ? RemoveAdsFallbackPrice : store.RemoveAdsPrice;

        public static void Use(IAdProvider adProvider, IStoreProvider storeProvider)
        {
            if (adProvider != null) ads = adProvider;
            if (storeProvider != null) store = storeProvider;
            ads.Initialize();
            store.Initialize();
            RefreshBanner();
        }

        /// <summary>The player owns Remove Ads (a purchase, or a restore on a new phone).</summary>
        public static void GrantRemoveAds()
        {
            if (SaveSystem.Current.adsRemoved) return;
            SaveSystem.Current.adsRemoved = true;
            SaveSystem.Save();
            RefreshBanner();
            Changed?.Invoke();
        }

        /// <summary>The store learned something the menus show (the localized price, availability).</summary>
        public static void NotifyChanged() => Changed?.Invoke();

        public static void OnMenu() { onMenu = true; RefreshBanner(); }
        public static void OnGameplay() { onMenu = false; RefreshBanner(); }

        static void RefreshBanner() => ads.SetBannerVisible(AdPolicy.BannerAllowed(onMenu, AdsRemoved));

        /// <summary>Between floors: maybe an interstitial, then `next` (always called exactly once).</summary>
        public static void BetweenFloors(int nextFloor, Action next)
        {
            floorsSinceInterstitial++;
            bool due = AdPolicy.InterstitialDue(nextFloor, floorsSinceInterstitial, Time.unscaledTime - lastInterstitialAt,
                Time.unscaledTime - lastRewardedAt, AdsRemoved);
            if (!due || !ads.InterstitialReady) { next?.Invoke(); return; }
            floorsSinceInterstitial = 0;
            lastInterstitialAt = Time.unscaledTime;
            ShowingFullScreenAd = true;
            ads.ShowInterstitial(() => { ShowingFullScreenAd = false; next?.Invoke(); });
        }

        public static void ShowRewarded(Action<bool> done)
        {
            if (!ads.RewardedReady) { done?.Invoke(false); return; }
            ShowingFullScreenAd = true;
            ads.ShowRewarded(ok =>
            {
                ShowingFullScreenAd = false;
                lastRewardedAt = Time.unscaledTime;
                done?.Invoke(ok);
            });
        }

        public static void BuyRemoveAds(Action<bool> done) => store.BuyRemoveAds(done);
        public static void RestorePurchases(Action<bool> done) => store.Restore(done);

        sealed class NoAds : IAdProvider
        {
            public void Initialize() { }
            public void SetBannerVisible(bool visible) { }
            public bool InterstitialReady => false;
            public void ShowInterstitial(Action closed) => closed?.Invoke();
            public bool RewardedReady => false;
            public void ShowRewarded(Action<bool> done) => done?.Invoke(false);
        }

        sealed class NoStore : IStoreProvider
        {
            public void Initialize() { }
            public bool Ready => false;
            public string RemoveAdsPrice => null;
            public void BuyRemoveAds(Action<bool> done) => done?.Invoke(false);
            public void Restore(Action<bool> done) => done?.Invoke(false);
        }
    }
}
