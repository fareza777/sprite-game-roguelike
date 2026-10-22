using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Player
{
    /// <summary>
    /// Conditions bosses put on the hero: held by roots (tap roll to tear free faster), and the ground's effect on
    /// movement (water and quicksand slow, ice slides), sampled from the live hazard zones.
    /// </summary>
    public sealed class PlayerStatus : MonoBehaviour, IRootable
    {
        const float StruggleSeconds = 0.35f;

        /// <summary>Drawn under the hero while rooted; set by whichever boss uses roots.</summary>
        public static Sprite RootSprite;

        float rootedUntil;
        SpriteRenderer rootVisual;

        public bool Rooted => Time.time < rootedUntil;

        public MoveModifier Ground => HazardZone.SampleMove(transform.position);

        public void Root(float seconds)
        {
            bool fresh = !Rooted;
            rootedUntil = Mathf.Max(rootedUntil, Time.time + seconds);
            if (!fresh) return;
            GameEvents.RaiseToast("Rooted! Tap roll to tear free");
            Haptics.Pulse(40);
            ShowRoots(true);
        }

        /// <summary>Each roll press while rooted tears a little free.</summary>
        public void Struggle()
        {
            rootedUntil -= StruggleSeconds;
            Haptics.Pulse(10);
        }

        public void Clear()
        {
            rootedUntil = 0f;
            ShowRoots(false);
        }

        void Update()
        {
            if (rootVisual != null && rootVisual.enabled && !Rooted) ShowRoots(false);
        }

        void ShowRoots(bool on)
        {
            if (on && rootVisual == null && RootSprite != null)
            {
                var go = new GameObject("Roots");
                go.transform.SetParent(transform, false);
                rootVisual = go.AddComponent<SpriteRenderer>();
                rootVisual.sprite = RootSprite;
                rootVisual.sortingOrder = 1;
            }
            if (rootVisual != null) rootVisual.enabled = on;
        }
    }
}
