using Oathdeep.Combat;

namespace Oathdeep.Player
{
    public enum HeroId { Aldric, Wren, Morrow, Brakka, Sable }

    public enum SkillKind { ShieldBash, FlareVolley, Requiem, Toll, Shadowstep }

    /// <summary>What a ranged combo hit fires. None = a melee swing.</summary>
    public enum ShotKind { None, Bolt, Flare, Orb }

    /// <summary>Timing + damage data for one combo hit, melee or ranged.</summary>
    public readonly struct ComboMove
    {
        public readonly string Anim;
        public readonly float Duration;
        public readonly float ActiveStart;
        public readonly float ActiveEnd;
        public readonly float LungeSpeed;
        public readonly float Reach;
        public readonly float Width;
        public readonly AttackSpec Spec;
        public readonly ShotKind Shot;
        public readonly float ShotSpeed;
        public readonly int ShotCount;
        public readonly float SpreadDegrees;
        public readonly float Homing;   // radians per second; 0 = straight

        ComboMove(string anim, float duration, float activeStart, float activeEnd, float lungeSpeed, float reach, float width,
                  AttackSpec spec, ShotKind shot, float shotSpeed, int shotCount, float spread, float homing)
        {
            Anim = anim; Duration = duration; ActiveStart = activeStart; ActiveEnd = activeEnd;
            LungeSpeed = lungeSpeed; Reach = reach; Width = width; Spec = spec;
            Shot = shot; ShotSpeed = shotSpeed; ShotCount = shotCount; SpreadDegrees = spread; Homing = homing;
        }

        public bool IsRanged => Shot != ShotKind.None;

        public static ComboMove Melee(string anim, float duration, float activeStart, float activeEnd, float lungeSpeed, float reach, float width, AttackSpec spec) =>
            new ComboMove(anim, duration, activeStart, activeEnd, lungeSpeed, reach, width, spec, ShotKind.None, 0f, 0, 0f, 0f);

        /// <summary>Fires at ActiveStart; ActiveEnd marks when recovery may be cancelled.</summary>
        public static ComboMove Ranged(string anim, float duration, float fireAt, ShotKind shot, float speed, int count, float spread, float homing, AttackSpec spec) =>
            new ComboMove(anim, duration, fireAt, fireAt + 0.04f, 0f, 0f, 0f, spec, shot, speed, count, spread, homing);
    }

    /// <summary>Everything that makes a hero play differently: combo, dodge feel and skill tuning.</summary>
    public sealed class HeroMoves
    {
        public const float ComboResetSeconds = 0.45f;
        public const float ChainFromFraction = 0.45f;
        public const float HurtStagger = 0.16f;

        public ComboMove[] Combo { get; private set; }
        public float DodgeDuration { get; private set; }
        public float DodgeSpeed { get; private set; }
        public float AutoAimRange { get; private set; }
        public float SlashScale { get; private set; }

        public SkillKind Skill { get; private set; }
        public string SkillAnim { get; private set; }
        public float SkillDuration { get; private set; }
        /// <summary>When the skill lands (seconds after it starts).</summary>
        public float SkillImpact { get; private set; }
        public float SkillSpeed { get; private set; }
        public float SkillRadius { get; private set; }
        public float SkillRange { get; private set; }
        public int SkillCount { get; private set; }
        public float SkillLifetime { get; private set; }
        public AttackSpec SkillSpec { get; private set; }

        public static HeroMoves For(HeroId id)
        {
            switch (id)
            {
                case HeroId.Wren: return Wren;
                case HeroId.Morrow: return Morrow;
                case HeroId.Brakka: return Brakka;
                case HeroId.Sable: return Sable;
                default: return Aldric;
            }
        }

        // Sword-and-board all-rounder: the Shield Bash dashes through a line of foes.
        public static readonly HeroMoves Aldric = new HeroMoves
        {
            Combo = new[]
            {
                ComboMove.Melee("attack1", 0.30f, 0.06f, 0.15f, 3.0f, 0.85f, 1.25f, new AttackSpec(1.0f, 4.5f, 0.10f, 0.045f, 0.10f)),
                ComboMove.Melee("attack2", 0.30f, 0.06f, 0.15f, 3.0f, 0.85f, 1.25f, new AttackSpec(1.1f, 4.5f, 0.10f, 0.045f, 0.12f)),
                ComboMove.Melee("attack3", 0.46f, 0.13f, 0.24f, 5.0f, 1.00f, 1.55f, new AttackSpec(1.8f, 10f, 0.35f, 0.085f, 0.28f)),
            },
            DodgeDuration = 0.28f, DodgeSpeed = 10.5f, AutoAimRange = 3.2f, SlashScale = 1f,
            Skill = SkillKind.ShieldBash, SkillAnim = "bash", SkillDuration = 0.32f, SkillSpeed = 9.5f, SkillRadius = 0.85f,
            SkillSpec = new AttackSpec(2.2f, 13f, 1.4f, 0.11f, 0.38f),
        };

        // Crossbow kiter: two bolts then a three-bolt spread; Flare Volley sprays burning bolts and hops back.
        public static readonly HeroMoves Wren = new HeroMoves
        {
            Combo = new[]
            {
                ComboMove.Ranged("attack1", 0.26f, 0.07f, ShotKind.Bolt, 14f, 1, 0f, 0f, new AttackSpec(0.9f, 2.5f, 0.05f, 0.03f, 0.05f)),
                ComboMove.Ranged("attack2", 0.26f, 0.07f, ShotKind.Bolt, 14f, 1, 0f, 0f, new AttackSpec(0.9f, 2.5f, 0.05f, 0.03f, 0.05f)),
                ComboMove.Ranged("attack3", 0.42f, 0.12f, ShotKind.Bolt, 15f, 3, 18f, 0f, new AttackSpec(0.85f, 4f, 0.12f, 0.04f, 0.10f)),
            },
            DodgeDuration = 0.26f, DodgeSpeed = 11.5f, AutoAimRange = 6.5f, SlashScale = 0f,
            Skill = SkillKind.FlareVolley, SkillAnim = "skill", SkillDuration = 0.40f, SkillImpact = 0.12f, SkillSpeed = 6f,
            SkillCount = 5, SkillRange = 50f, SkillSpec = new AttackSpec(1.4f, 6f, 0.25f, 0.05f, 0.18f, Element.Fire),
        };

        // Necromancer singer: slow homing soul orbs; Requiem raises three wisps that circle and bite.
        public static readonly HeroMoves Morrow = new HeroMoves
        {
            Combo = new[]
            {
                ComboMove.Ranged("attack1", 0.32f, 0.10f, ShotKind.Orb, 7.5f, 1, 0f, 5f, new AttackSpec(1.1f, 3f, 0.10f, 0.03f, 0.06f, Element.Frost)),
                ComboMove.Ranged("attack2", 0.32f, 0.10f, ShotKind.Orb, 7.5f, 1, 0f, 5f, new AttackSpec(1.1f, 3f, 0.10f, 0.03f, 0.06f, Element.Frost)),
                ComboMove.Ranged("attack3", 0.50f, 0.16f, ShotKind.Orb, 7f, 3, 34f, 6f, new AttackSpec(1.0f, 4f, 0.20f, 0.05f, 0.12f, Element.Frost)),
            },
            DodgeDuration = 0.30f, DodgeSpeed = 9.5f, AutoAimRange = 6f, SlashScale = 0f,
            Skill = SkillKind.Requiem, SkillAnim = "skill", SkillDuration = 0.45f, SkillImpact = 0.2f,
            SkillCount = 3, SkillRadius = 1.35f, SkillLifetime = 7f, SkillSpec = new AttackSpec(0.7f, 3f, 0.15f, 0.02f, 0.04f, Element.Frost),
        };

        // Bell-hammer bruiser: slow, wide, heavy; Toll slams a shockwave that stuns everything around him.
        public static readonly HeroMoves Brakka = new HeroMoves
        {
            Combo = new[]
            {
                ComboMove.Melee("attack1", 0.42f, 0.14f, 0.24f, 2.2f, 1.00f, 1.7f, new AttackSpec(1.25f, 7f, 0.20f, 0.06f, 0.18f)),
                ComboMove.Melee("attack2", 0.42f, 0.14f, 0.24f, 2.2f, 1.00f, 1.7f, new AttackSpec(1.25f, 7f, 0.20f, 0.06f, 0.18f)),
                ComboMove.Melee("attack3", 0.60f, 0.22f, 0.34f, 3.0f, 1.15f, 2.0f, new AttackSpec(2.3f, 14f, 0.50f, 0.10f, 0.36f)),
            },
            DodgeDuration = 0.30f, DodgeSpeed = 9f, AutoAimRange = 3.2f, SlashScale = 1.35f,
            Skill = SkillKind.Toll, SkillAnim = "skill", SkillDuration = 0.50f, SkillImpact = 0.22f, SkillRadius = 2.3f,
            SkillSpec = new AttackSpec(2.0f, 15f, 1.2f, 0.12f, 0.55f),
        };

        // Twin-dagger duelist: fast light cuts with high crit; Shadowstep blinks behind a foe and opens them up.
        public static readonly HeroMoves Sable = new HeroMoves
        {
            Combo = new[]
            {
                ComboMove.Melee("attack1", 0.22f, 0.04f, 0.11f, 4.0f, 0.75f, 1.1f, new AttackSpec(0.8f, 3f, 0.05f, 0.03f, 0.06f)),
                ComboMove.Melee("attack2", 0.22f, 0.04f, 0.11f, 4.0f, 0.75f, 1.1f, new AttackSpec(0.8f, 3f, 0.05f, 0.03f, 0.06f)),
                ComboMove.Melee("attack3", 0.36f, 0.08f, 0.20f, 7.0f, 0.90f, 1.3f, new AttackSpec(1.5f, 8f, 0.25f, 0.07f, 0.20f)),
            },
            DodgeDuration = 0.24f, DodgeSpeed = 13f, AutoAimRange = 3.6f, SlashScale = 0.8f,
            Skill = SkillKind.Shadowstep, SkillAnim = "skill", SkillDuration = 0.32f, SkillImpact = 0.12f,
            SkillRange = 4.5f, SkillRadius = 1.1f, SkillSpec = new AttackSpec(3.0f, 9f, 0.6f, 0.10f, 0.30f),
        };
    }
}
