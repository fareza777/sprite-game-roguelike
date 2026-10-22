using System.Collections.Generic;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// The boss raises breakable anchors on entering a phase and cannot be hurt until every one is broken.
    /// Bone Warden: bone totems. Verdant Hierophant: root cores. The Cantor: dusk anchors.
    /// </summary>
    public sealed class TotemShield : BossMechanic
    {
        public enum Style { Bone, Root, Dusk }

        [SerializeField] Style style;
        [SerializeField] int count = 3;
        [SerializeField] float healthShare = 0.06f;   // each anchor's health, as a share of the boss's

        readonly List<BossAnchor> anchors = new List<BossAnchor>();

        public override float DamageMultiplier => anchors.Count > 0 ? 0f : 1f;
        public override string BlockedHint => anchors.Count > 0 ? $"Shielded! Break the {Label}!" : null;

        string Label => style == Style.Bone ? "bone totems" : style == Style.Root ? "root cores" : "dusk anchors";

        Sprite Art => style == Style.Bone ? Fx.boneTotem : style == Style.Root ? Fx.rootCore : Fx.duskAnchor;

        public override void OnPhase(int phase)
        {
            if (Fx == null) return;
            int hp = Mathf.Max(30, Mathf.RoundToInt(Core.Health.Max * healthShare));
            foreach (var at in RingPoints(count, 7f, 30f + phase * 50f))
            {
                var a = BossAnchor.Spawn(Core.HazardRoot, Art, at, hp, new Vector2(1.2f, 1.8f));
                a.Broken += OnBroken;
                anchors.Add(a);
            }
            GameEvents.RaiseToast($"{Core.DisplayName} raises {Label}. Break them!");
        }

        void OnBroken(BossAnchor a)
        {
            anchors.Remove(a);
            if (anchors.Count == 0) GameEvents.RaiseToast($"{Core.DisplayName}'s shield shatters!");
        }

        public override void End()
        {
            foreach (var a in anchors) if (a != null) Destroy(a.gameObject);
            anchors.Clear();
        }

#if UNITY_EDITOR
        public void EditorInit(Style s, int n, float share) { style = s; count = n; healthShare = share; }
#endif
    }
}
