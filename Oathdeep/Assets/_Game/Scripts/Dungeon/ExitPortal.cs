using System;
using Oathdeep.Player;
using UnityEngine;

namespace Oathdeep.Dungeon
{
    /// <summary>Stairway that appears when the floor's boss falls. Touch it to finish the floor.</summary>
    public sealed class ExitPortal : MonoBehaviour
    {
        public static event Action Reached;

        bool used;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (used || other.GetComponentInParent<PlayerController>() == null) return;
            used = true;
            Reached?.Invoke();
        }

        public static void ClearListeners() => Reached = null;
    }
}
