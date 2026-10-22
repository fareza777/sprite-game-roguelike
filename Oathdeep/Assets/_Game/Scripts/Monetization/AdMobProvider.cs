using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Monetization
{
    /// <summary>
    /// Google AdMob: consent first (UMP), then an adaptive banner kept hidden until a menu asks for it,
    /// and one interstitial and one rewarded ad always preloaded so they show instantly when the game wants them.
    /// </summary>
    public sealed class AdMobProvider : IAdProvider
    {
        // Google's sample ad units: always serve test ads. Swap for the real units from the AdMob console before release.
        const string BannerId = "ca-app-pub-3940256099942544/9214589741";
        const string InterstitialId = "ca-app-pub-3940256099942544/1033173712";
        const string RewardedId = "ca-app-pub-3940256099942544/5224354917";
        const float RetrySeconds = 30f;
        const float ConsentPollSeconds = 1f, ConsentWaitSeconds = 120f;

        BannerView banner;
        InterstitialAd interstitial;
        RewardedAd rewarded;
        bool ready, bannerWanted, bannerLoaded, loadingInterstitial, loadingRewarded;

        public bool InterstitialReady => interstitial != null && interstitial.CanShowAd();
        public bool RewardedReady => rewarded != null && rewarded.CanShowAd();

        public void Initialize()
        {
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            ConsentInformation.Update(new ConsentRequestParameters(), updateError =>
            {
                if (updateError != null) Debug.LogWarning($"[Ads] consent info: {updateError.Message}");
                ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                {
                    if (formError != null) Debug.LogWarning($"[Ads] consent form: {formError.Message}");
                });
            });
            // The consent callbacks can arrive off the main thread (or not at all on a first launch), so the main
            // thread checks for itself. Consent from an earlier session counts straight away.
            AdRetry.Until(ConsentPollSeconds, ConsentWaitSeconds, () =>
            {
                if (!ConsentInformation.CanRequestAds()) return false;
                StartAds();
                return true;
            });
        }

        void StartAds()
        {
            if (ready) return;
            ready = true;
            MobileAds.Initialize(_ =>
            {
                LoadInterstitial();
                LoadRewarded();
                if (bannerWanted) ShowBanner();
            });
        }

        public void SetBannerVisible(bool visible)
        {
            bannerWanted = visible;
            if (!ready) return;
            if (visible) ShowBanner();
            else banner?.Hide();
        }

        void ShowBanner()
        {
            if (banner == null)
            {
                var size = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                banner = new BannerView(BannerId, size, AdPosition.Bottom);
                banner.OnBannerAdLoaded += () => { bannerLoaded = true; if (!bannerWanted) banner.Hide(); };
                banner.OnBannerAdLoadFailed += e => Debug.LogWarning($"[Ads] banner: {e.GetMessage()}");
                banner.LoadAd(new AdRequest());
                return;
            }
            if (bannerLoaded) banner.Show();
        }

        void LoadInterstitial()
        {
            if (loadingInterstitial) return;
            loadingInterstitial = true;
            interstitial?.Destroy();
            interstitial = null;
            InterstitialAd.Load(InterstitialId, new AdRequest(), (ad, error) =>
            {
                loadingInterstitial = false;
                if (error != null || ad == null) { Retry(LoadInterstitial, "interstitial", error); return; }
                interstitial = ad;
            });
        }

        void LoadRewarded()
        {
            if (loadingRewarded) return;
            loadingRewarded = true;
            rewarded?.Destroy();
            rewarded = null;
            RewardedAd.Load(RewardedId, new AdRequest(), (ad, error) =>
            {
                loadingRewarded = false;
                if (error != null || ad == null) { Retry(LoadRewarded, "rewarded", error); return; }
                rewarded = ad;
            });
        }

        static void Retry(Action load, string what, LoadAdError error)
        {
            Debug.LogWarning($"[Ads] {what} failed to load: {error?.GetMessage()}");
            AdRetry.After(RetrySeconds, load);
        }

        public void ShowInterstitial(Action closed)
        {
            if (!InterstitialReady) { closed?.Invoke(); return; }
            var ad = interstitial;
            bool finished = false;
            void Finish()
            {
                if (finished) return;
                finished = true;
                LoadInterstitial();
                closed?.Invoke();
            }
            ad.OnAdFullScreenContentClosed += Finish;
            ad.OnAdFullScreenContentFailed += e => { Debug.LogWarning($"[Ads] interstitial show: {e.GetMessage()}"); Finish(); };
            ad.Show();
        }

        public void ShowRewarded(Action<bool> done)
        {
            if (!RewardedReady) { done?.Invoke(false); return; }
            var ad = rewarded;
            bool earned = false, finished = false;
            void Finish()
            {
                if (finished) return;
                finished = true;
                // On some phones the reward callback lands just after the close; give it a moment before deciding.
                AdRetry.After(0.25f, () => { LoadRewarded(); done?.Invoke(earned); });
            }
            ad.OnAdFullScreenContentClosed += Finish;
            ad.OnAdFullScreenContentFailed += e => { Debug.LogWarning($"[Ads] rewarded show: {e.GetMessage()}"); Finish(); };
            ad.Show(_ => earned = true);
        }
    }
}
