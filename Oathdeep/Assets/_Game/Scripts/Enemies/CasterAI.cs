using Oathdeep.Audio;
using Oathdeep.Combat;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>Keeps its distance and calls ground blasts on and around the player. Weak up close.</summary>
    public sealed class CasterAI : EnemyBase
    {
        enum S { Idle, Drift, Cast, Recover }

        [SerializeField] GroundBlast blastPrefab;
        [SerializeField] float preferred = 4.5f;
        [SerializeField] float castTime = 0.6f;
        [SerializeField] float blastDelay = 0.9f;
        [SerializeField] float blastRadius = 1.2f;
        [SerializeField] int blasts = 1;
        [SerializeField] float recover = 1.4f;
        [SerializeField] AttackSpec blast = new AttackSpec(1.3f, 5f, 0.2f, 0.05f, 0.15f);

        S state;
        Vector2 wander;

        protected override void OnSpawned() => Go(S.Idle);

        void Go(S s)
        {
            state = s;
            StateTime = 0f;
            if (s == S.Drift) wander = Random.insideUnitCircle.normalized;
        }

        protected override void Think(float dt)
        {
            Face(ToTarget);
            switch (state)
            {
                case S.Idle:
                    anim.Play("idle");
                    if (DistanceToTarget < data.AggroRange && StateTime > 0.4f) Go(S.Drift);
                    break;
                case S.Drift:
                    anim.Play("run");
                    if (StateTime > 1.1f) { anim.Play("windup", true); Go(S.Cast); }
                    break;
                case S.Cast:
                    if (StateTime >= castTime)
                    {
                        anim.Play("attack", true);
                        AudioManager.Play(Sfx.BossCast, transform.position, 0.6f);
                        Vector2 at = Target.position;
                        for (int i = 0; i < blasts; i++)
                        {
                            Vector2 offset = i == 0 ? Vector2.zero : Random.insideUnitCircle.normalized * blastRadius * 1.6f;
                            GroundBlast.Spawn(blastPrefab, at + offset, blastRadius, blastDelay, Stats, blast);
                        }
                        Go(S.Recover);
                    }
                    break;
                case S.Recover:
                    anim.Play("idle");
                    if (StateTime >= recover) Go(S.Drift);
                    break;
            }
        }

        protected override Vector2 DesiredVelocity()
        {
            if (state != S.Drift) return Vector2.zero;
            Vector2 to = ToTarget.normalized;
            float d = DistanceToTarget;
            Vector2 v = d < preferred - 1f ? -to : d > preferred + 1.5f ? to : Vector2.Perpendicular(to) * Mathf.Sign(wander.x);
            return (v + wander * 0.25f).normalized * MoveSpeed;
        }

        protected override void CancelAttack()
        {
            base.CancelAttack();
            if (state == S.Cast) Go(S.Recover);
        }
    }
}
