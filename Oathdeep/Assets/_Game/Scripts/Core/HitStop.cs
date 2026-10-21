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
