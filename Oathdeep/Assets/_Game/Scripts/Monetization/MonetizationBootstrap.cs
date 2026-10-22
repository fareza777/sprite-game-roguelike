using UnityEngine;

namespace Oathdeep.Monetization
{
    /// <summary>Plugs AdMob and Unity IAP into the game on Android devices. The editor and tests keep the no-op providers.</summary>
    static class MonetizationBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (Application.isEditor || Application.platform != RuntimePlatform.Android) return;
            Core.Monetization.Use(new AdMobProvider(), new IapStoreProvider());
        }
    }
}
