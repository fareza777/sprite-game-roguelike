using Oathdeep.Audio;
using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Player
{
    /// <summary>The five hero skills. Each skill is invulnerable while it plays and lands once at HeroMoves.SkillImpact.</summary>
    public sealed partial class PlayerController
    {
        static readonly RaycastHit2D[] StepHits = new RaycastHit2D[8];

        [Header("Skill effects")]
        [SerializeField] GameObject bashVfx;
        [SerializeField] Projectile boltPrefab;
        [SerializeField] Projectile flarePrefab;
        [SerializeField] Projectile orbPrefab;
        [SerializeField] GameObject wispPrefab;
        [SerializeField] GameObject tollVfx;
        [SerializeField] GameObject shadowVfx;

        bool skillLanded;
        Vector2 stepTarget;

        Projectile ShotPrefab(ShotKind kind)
        {
            switch (kind)
            {
                case ShotKind.Flare: return flarePrefab;
                case ShotKind.Orb: return orbPrefab;
                default: return boltPrefab;
            }
        }

        void StartSkill()
        {
            AutoAim();
            dashDir = aim;
            skillReadyAt = Time.time + skillCooldown;
            skillLanded = false;
            hitThisSwing.Clear();
            health.PushInvulnerable();
            Enter(State.Skill);
            anim.Face(aim.x);
            anim.Play(moves.SkillAnim, true);
            Performed?.Invoke(PlayerAction.Skill);
            Haptics.Pulse(25);

            switch (moves.Skill)
            {
                case SkillKind.ShieldBash:
                    if (bashVfx != null) Pool.Spawn(bashVfx, transform.position, Quaternion.Euler(0, 0, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg));
                    AudioManager.Play(Sfx.Bash);
                    break;
                case SkillKind.Shadowstep:
                    stepTarget = FindStepTarget();
                    if (shadowVfx != null) Pool.Spawn(shadowVfx, transform.position + Vector3.up * 0.4f, Quaternion.identity);
                    if (flash != null) flash.Flash(new Color(0.3f, 0.2f, 0.5f), moves.SkillImpact);
                    AudioManager.Play(Sfx.Dodge);
                    break;
                case SkillKind.Toll:
                case SkillKind.Requiem:
                    AudioManager.Play(Sfx.BossCast);
                    break;
            }
        }

        Vector2 SkillVelocity()
        {
            switch (moves.Skill)
            {
                case SkillKind.ShieldBash:
                    return dashDir * moves.SkillSpeed * (stateTime < moves.SkillDuration * 0.6f ? 1f : 0.2f);
                case SkillKind.FlareVolley:
                    return stateTime < moves.SkillImpact + 0.1f ? -dashDir * moves.SkillSpeed : Vector2.zero;   // hop back while firing
                default:
                    return Vector2.zero;
            }
        }

        void UpdateSkill()
        {
            if (moves.Skill == SkillKind.ShieldBash)
            {
                // The bash hits continuously along its dash rather than at one moment.
                if (stateTime <= moves.SkillDuration * 0.7f &&
                    HitQuery.StrikeCircle(stats, (Vector2)transform.position + dashDir * 0.55f, moves.SkillRadius, moves.SkillSpec, hitThisSwing) > 0)
                    Haptics.Pulse(40);
            }
            else if (!skillLanded && stateTime >= moves.SkillImpact)
            {
                skillLanded = true;
                LandSkill();
            }

            if (stateTime >= moves.SkillDuration)
            {
                EndSkillEffects();
                Enter(State.Move);
            }
        }

        void LandSkill()
        {
            switch (moves.Skill)
            {
                case SkillKind.FlareVolley:
                    FireVolley(ShotKind.Flare, moves.SkillCount, moves.SkillRange, 12f, 0f, moves.SkillSpec);
                    CameraRig.Shake(0.12f);
                    break;

                case SkillKind.Requiem:
                    SoulWisps.Summon(gameObject, stats, wispPrefab, moves.SkillCount, moves.SkillRadius, moves.SkillLifetime, moves.SkillSpec);
                    break;

                case SkillKind.Toll:
                    if (tollVfx != null)
                        Pool.Spawn(tollVfx, transform.position, Quaternion.identity).transform.localScale = Vector3.one * moves.SkillRadius * 0.95f;
                    HitQuery.StrikeCircle(stats, transform.position, moves.SkillRadius, moves.SkillSpec, hitThisSwing);
                    CameraRig.Shake(0.55f);
                    AudioManager.Play(Sfx.BossSlam);
                    Haptics.Pulse(60);
                    break;

                case SkillKind.Shadowstep:
                    body.position = stepTarget;
                    transform.position = stepTarget;
                    if (shadowVfx != null) Pool.Spawn(shadowVfx, (Vector3)stepTarget + Vector3.up * 0.4f, Quaternion.identity);
                    var victim = TargetRegistry.Nearest(stepTarget, moves.SkillRange);
                    if (victim != null) aim = ((Vector2)victim.transform.position - stepTarget).normalized;
                    anim.Face(aim.x);
                    HitQuery.StrikeCircle(stats, stepTarget + aim * 0.5f, moves.SkillRadius, moves.SkillSpec, hitThisSwing);
                    AudioManager.Play(Sfx.SwingHeavy);
                    Haptics.Pulse(50);
                    break;
            }
        }

        void EndSkillEffects() => health.PopInvulnerable();

        /// <summary>Behind the nearest enemy in range, else a short blink forward; never through a wall.</summary>
        Vector2 FindStepTarget()
        {
            Vector2 from = transform.position;
            var target = TargetRegistry.Nearest(from, moves.SkillRange);
            Vector2 want = target != null
                ? (Vector2)target.transform.position + ((Vector2)target.transform.position - from).normalized * 0.8f
                : from + aim * 3f;

            Vector2 delta = want - from;
            float distance = delta.magnitude;
            if (distance < 0.01f) return from;
            int count = Physics2D.CircleCast(from, 0.28f, delta / distance, new ContactFilter2D { useTriggers = false }, StepHits, distance);
            for (int i = 0; i < count; i++)
            {
                var hit = StepHits[i];
                if (hit.rigidbody == body || hit.collider.GetComponentInParent<Health>() != null) continue;   // bodies don't block a shadow
                return from + delta / distance * Mathf.Max(0f, hit.distance - 0.1f);
            }
            return want;
        }
    }
}
