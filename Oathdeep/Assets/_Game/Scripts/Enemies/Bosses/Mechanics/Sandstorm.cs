using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// The Dune Pharaoh: quicksand pits open near the hero, and from `fogFromPhase` a sandstorm cuts sight down to a
    /// wide clear circle around the hero (a dusty vignette, never a blindfold).
    /// </summary>
    public sealed class Sandstorm : BossMechanic
    {
        static readonly Color SandTint = new Color(1f, 1f, 1f, 0.9f);

        [SerializeField, Range(1, 3)] int fogFromPhase = 2;
        [SerializeField] bool quicksand = true;

        SandFog fog;
        float timer = 3f;

        public override void OnPhase(int phase)
        {
            if (phase < fogFromPhase || Fx == null || Fx.sandFog == null || Core.Target == null) return;
            if (fog == null)
            {
                fog = SandFog.Spawn(Core.HazardRoot, Fx.sandFog, Core.Target);
                GameEvents.RaiseToast("A sandstorm rises!");
            }
            fog.Set(true);
        }

        public override void Tick(float dt)
        {
            if (!quicksand || Fx == null || !Every(ref timer, Core.Phase >= 3 ? 5.5f : 8f, dt)) return;
            int count = Core.Phase >= 3 ? 2 : 1;
            for (int i = 0; i < count; i++)
                Zone(Fx.quicksand, SandTint, HazardKind.Slow, 0.3f, HazardShape.Circle(PointNear(TargetPos, 2.5f), 2.5f), 1f, 7f);
        }

        public override void End()
        {
            if (fog != null) Destroy(fog.gameObject);
        }

#if UNITY_EDITOR
        public void EditorInit(int fogPhase, bool pits) { fogFromPhase = fogPhase; quicksand = pits; }
#endif
    }
}
