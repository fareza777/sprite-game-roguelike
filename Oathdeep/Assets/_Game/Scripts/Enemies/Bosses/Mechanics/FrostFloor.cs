using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>The Pale Matriarch freezes the arena: four sheets of ice where the hero slides, leaving only the cross lanes firm.</summary>
    public sealed class FrostFloor : BossMechanic
    {
        static readonly Color Tint = new Color(1f, 1f, 1f, 0.5f);
        bool frozen;

        public override void OnPhase(int phase)
        {
            if (frozen || Fx == null) return;
            frozen = true;
            var a = Arena;
            const float lane = 2.5f, margin = 1f;
            float w = a.width / 2f - lane - margin, h = a.height / 2f - lane - margin;
            Ice(new Rect(a.xMin + margin, a.yMin + margin, w, h));
            Ice(new Rect(a.center.x + lane, a.yMin + margin, w, h));
            Ice(new Rect(a.xMin + margin, a.center.y + lane, w, h));
            Ice(new Rect(a.center.x + lane, a.center.y + lane, w, h));
            GameEvents.RaiseToast("The floor freezes over! Mind your footing.");
        }

        void Ice(Rect r) => Zone(Fx.ice, Tint, HazardKind.Ice, 0.07f, HazardShape.Box(r), 1.2f, 9999f);
    }
}
