using Oathdeep.Combat;

namespace Oathdeep.Player
{
    /// <summary>Timing + damage data for one melee move.</summary>
    public readonly struct MeleeMove
    {
        public readonly string Anim;
        public readonly float Duration;
        public readonly float ActiveStart;
        public readonly float ActiveEnd;
        public readonly float LungeSpeed;
        public readonly float Reach;
        public readonly float Width;
        public readonly AttackSpec Spec;

        public MeleeMove(string anim, float duration, float activeStart, float activeEnd, float lungeSpeed, float reach, float width, AttackSpec spec)
        {
            Anim = anim; Duration = duration; ActiveStart = activeStart; ActiveEnd = activeEnd;
            LungeSpeed = lungeSpeed; Reach = reach; Width = width; Spec = spec;
        }
    }

    /// <summary>Knight move list. Tuned for snappy mobile combat.</summary>
    public static class KnightMoves
    {
        public static readonly MeleeMove[] Combo =
        {
            new MeleeMove("attack1", 0.30f, 0.06f, 0.15f, 3.0f, 0.85f, 1.25f, new AttackSpec(1.0f, 4.5f, 0.10f, 0.045f, 0.10f)),
            new MeleeMove("attack2", 0.30f, 0.06f, 0.15f, 3.0f, 0.85f, 1.25f, new AttackSpec(1.1f, 4.5f, 0.10f, 0.045f, 0.12f)),
            new MeleeMove("attack3", 0.46f, 0.13f, 0.24f, 5.0f, 1.00f, 1.55f, new AttackSpec(1.8f, 10f, 0.35f, 0.085f, 0.28f)),
        };

        public const float ComboResetSeconds = 0.45f;
        public const float ChainFromFraction = 0.45f;

        public const float DodgeDuration = 0.28f;
        public const float DodgeSpeed = 10.5f;

        public const float BashDuration = 0.32f;
        public const float BashSpeed = 9.5f;
        public const float BashRadius = 0.85f;
        public static readonly AttackSpec Bash = new AttackSpec(2.2f, 13f, 1.4f, 0.11f, 0.38f);

        public const float HurtStagger = 0.16f;
        public const float AutoAimRange = 3.2f;
    }
}
