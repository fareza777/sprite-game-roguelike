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
            Instance().StartCoroutine(Run(seconds, action));
        }

        static AdRetry Instance()
        {
            if (instance == null)
            {
                var go = new GameObject("AdRetry") { hideFlags = HideFlags.HideAndDontSave };
                DontDestroyOnLoad(go);
                instance = go.AddComponent<AdRetry>();
            }
            return instance;
        }

        /// <summary>Calls `done` every `interval` seconds (real time) until it returns true or `timeout` passes.</summary>
        public static void Until(float interval, float timeout, Func<bool> done)
        {
            Instance().StartCoroutine(Poll(interval, timeout, done));
        }

        static IEnumerator Poll(float interval, float timeout, Func<bool> done)
        {
            for (float waited = 0f; waited <= timeout; waited += interval)
            {
                if (done()) yield break;
                yield return new WaitForSecondsRealtime(interval);
            }
        }

        static IEnumerator Run(float seconds, Action action)
        {
            yield return new WaitForSecondsRealtime(seconds);
            action?.Invoke();
        }
    }
}
