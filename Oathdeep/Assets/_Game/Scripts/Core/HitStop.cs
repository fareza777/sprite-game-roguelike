using System.Collections;
using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>Freezes time for a few real-time milliseconds to sell impact.</summary>
    public sealed class HitStop : MonoBehaviour
    {
        static HitStop instance;
        float frozenUntil;
        bool running;

        void Awake() => instance = this;
        void OnDestroy() { if (instance == this) instance = null; }

        public static void Trigger(float seconds)
        {
            if (instance == null || seconds <= 0f || GameSettings.ReduceMotion) return;
            instance.frozenUntil = Mathf.Max(instance.frozenUntil, Time.unscaledTime + seconds);
            if (!instance.running) instance.StartCoroutine(instance.Run());
        }

        /// <summary>A beat of slow motion (a boss falls). Skipped while paused or with Reduce Motion on.</summary>
        public static void SlowMotion(float scale, float seconds)
        {
            if (instance == null || seconds <= 0f || GameSettings.ReduceMotion || Time.timeScale <= 0f) return;
            instance.StartCoroutine(instance.Slow(scale, seconds));
        }

        IEnumerator Slow(float scale, float seconds)
        {
            yield return new WaitWhile(() => running);   // let a hit stop finish first
            if (Time.timeScale <= 0f) yield break;
            Time.timeScale = scale;
            float end = Time.unscaledTime + seconds;
            while (Time.unscaledTime < end && Time.timeScale > 0f) yield return null;
            if (Mathf.Approximately(Time.timeScale, scale)) Time.timeScale = 1f;
        }

        IEnumerator Run()
        {
            running = true;
            float previous = Time.timeScale;
            if (previous <= 0f) { running = false; yield break; } // paused: skip
            Time.timeScale = 0.02f;
            while (Time.unscaledTime < frozenUntil) yield return null;
            if (Time.timeScale > 0f) Time.timeScale = previous;
            running = false;
        }
    }
}
