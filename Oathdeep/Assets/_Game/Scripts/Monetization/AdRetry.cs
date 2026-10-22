using System;
using System.Collections;
using UnityEngine;

namespace Oathdeep.Monetization
{
    /// <summary>Runs a callback after a real-time delay (ads load and close while the game may be paused).</summary>
    sealed class AdRetry : MonoBehaviour
    {
        static AdRetry instance;

        public static void After(float seconds, Action action)
        {
            if (instance == null)
            {
                var go = new GameObject("AdRetry") { hideFlags = HideFlags.HideAndDontSave };
                DontDestroyOnLoad(go);
                instance = go.AddComponent<AdRetry>();
            }
            instance.StartCoroutine(Run(seconds, action));
        }

        static IEnumerator Run(float seconds, Action action)
        {
            yield return new WaitForSecondsRealtime(seconds);
            action?.Invoke();
        }
    }
}
