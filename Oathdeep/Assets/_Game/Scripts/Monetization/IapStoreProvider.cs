using System;
using System.Collections.Generic;
using System.Linq;
using Oathdeep.Core;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Oathdeep.Monetization
{
    /// <summary>
    /// Unity IAP v5 for the one non-consumable, Remove Ads. Purchases and restores both arrive as pending orders:
    /// the game records the entitlement (and saves) before the order is confirmed, so a crash never loses it.
    /// </summary>
    public sealed class IapStoreProvider : IStoreProvider
    {
        StoreController store;
        Action<bool> buyDone, restoreDone;

        public bool Ready { get; private set; }
        public string RemoveAdsPrice { get; private set; }

        public async void Initialize()
        {
            store = UnityIAPServices.StoreController();
            store.OnStoreConnected += OnConnected;
            store.OnStoreDisconnected += f => { Ready = false; Debug.LogWarning($"[IAP] store disconnected: {f.Message}"); };
            store.OnProductsFetched += OnProductsFetched;
            store.OnProductsFetchFailed += f => Debug.LogWarning($"[IAP] products failed: {f.FailureReason}");
            store.OnPurchasesFetched += OnPurchasesFetched;
            store.OnPurchasesFetchFailed += f => Debug.LogWarning($"[IAP] purchases failed: {f.Message}");
            store.OnPurchasePending += OnPurchasePending;
            store.OnPurchaseConfirmed += OnPurchaseConfirmed;
            store.OnPurchaseFailed += OnPurchaseFailed;
            store.OnPurchaseDeferred += _ => { Debug.Log("[IAP] purchase awaiting approval"); Finish(ref buyDone, false); };
            try
            {
                await store.Connect();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[IAP] connect failed: {e.Message}");
            }
        }

        void OnConnected()
        {
            store.FetchProducts(new List<ProductDefinition>
            {
                new ProductDefinition(Core.Monetization.RemoveAdsProductId, ProductType.NonConsumable)
            });
        }

        void OnProductsFetched(List<Product> products)
        {
            var removeAds = products.FirstOrDefault(IsRemoveAds);
            if (removeAds != null) RemoveAdsPrice = removeAds.metadata.localizedPriceString;
            Ready = removeAds != null;
            // A purchase made on another phone, or before a reinstall, comes back here.
            store.FetchPurchases();
            Core.Monetization.NotifyChanged();
        }

        void OnPurchasesFetched(Orders orders)
        {
            if (orders.ConfirmedOrders.Any(o => Contains(o, Core.Monetization.RemoveAdsProductId)))
                Core.Monetization.GrantRemoveAds();
        }

        void OnPurchasePending(PendingOrder order)
        {
            if (Contains(order, Core.Monetization.RemoveAdsProductId)) Core.Monetization.GrantRemoveAds();
            store.ConfirmPurchase(order);
        }

        void OnPurchaseConfirmed(Order order)
        {
            if (order is FailedOrder failed)
            {
                Debug.LogWarning($"[IAP] confirm failed: {failed.FailureReason} {failed.Details}");
                Finish(ref buyDone, Core.Monetization.AdsRemoved);
                return;
            }
            Finish(ref buyDone, true);
            Finish(ref restoreDone, true);
        }

        void OnPurchaseFailed(FailedOrder failed)
        {
            Debug.LogWarning($"[IAP] purchase failed: {failed.FailureReason} {failed.Details}");
            // Buying something already owned means the player has it: treat it as restored.
            bool owned = failed.FailureReason == PurchaseFailureReason.DuplicateTransaction;
            if (owned) Core.Monetization.GrantRemoveAds();
            Finish(ref buyDone, owned);
        }

        public void BuyRemoveAds(Action<bool> done)
        {
            if (!Ready || buyDone != null) { done?.Invoke(false); return; }
            if (Core.Monetization.AdsRemoved) { done?.Invoke(true); return; }
            buyDone = done;
            store.PurchaseProduct(Core.Monetization.RemoveAdsProductId);
        }

        public void Restore(Action<bool> done)
        {
            if (store == null || restoreDone != null) { done?.Invoke(false); return; }
            restoreDone = done;
            store.RestoreTransactions((ok, error) =>
            {
                if (!ok) Debug.LogWarning($"[IAP] restore failed: {error}");
                store.FetchPurchases();
                Finish(ref restoreDone, ok && Core.Monetization.AdsRemoved);
            });
        }

        static void Finish(ref Action<bool> callback, bool ok)
        {
            var cb = callback;
            callback = null;
            cb?.Invoke(ok);
        }

        static bool IsRemoveAds(Product p) => p != null && p.definition.id == Core.Monetization.RemoveAdsProductId;

        static bool Contains(Order order, string productId) =>
            order.CartOrdered.Items().Any(i => i.Product != null && i.Product.definition.id == productId);
    }
}
