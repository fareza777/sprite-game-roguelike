using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oathdeep.Dungeon
{
    /// <summary>
    /// Something the player can use by standing close and tapping the context button (TALK, TRADE, PRAY...).
    /// Shows a bubble over itself while in reach. One-handed friendly: no precise tapping in the world.
    /// </summary>
    public sealed class Interactable : MonoBehaviour
    {
        static readonly List<Interactable> Active = new List<Interactable>();

        [SerializeField] string prompt = "TALK";
        [SerializeField] float reach = 1.6f;
        [SerializeField] GameObject bubble;

        Action onUse;
        bool spent;

        public string Prompt => prompt;

        public void Setup(string label, Action use)
        {
            prompt = label;
            onUse = use;
            spent = false;
        }

        /// <summary>Removes the prompt after a one-time encounter; the NPC stays as scenery.</summary>
        public void Spend()
        {
            spent = true;
            if (bubble != null) bubble.SetActive(false);
        }

        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);

        public void Use()
        {
            if (spent) return;
            onUse?.Invoke();
        }

        /// <summary>The closest usable interactable within reach of <paramref name="from"/>, or null.</summary>
        public static Interactable Nearest(Vector2 from)
        {
            Interactable best = null;
            float bestSq = float.MaxValue;
            foreach (var i in Active)
            {
                if (i.spent) continue;
                float sq = ((Vector2)i.transform.position - from).sqrMagnitude;
                bool inReach = sq <= i.reach * i.reach;
                if (i.bubble != null && i.bubble.activeSelf != inReach) i.bubble.SetActive(inReach);
                if (inReach && sq < bestSq) { bestSq = sq; best = i; }
            }
            return best;
        }

#if UNITY_EDITOR
        public void EditorInit(string label, float range, GameObject bubbleObject) { prompt = label; reach = range; bubble = bubbleObject; }
#endif
    }
}
