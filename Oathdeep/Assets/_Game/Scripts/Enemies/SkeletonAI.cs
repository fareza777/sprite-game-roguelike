using System.Collections.Generic;
using Oathdeep.Combat;
using UnityEngine;
using Oathdeep.Audio;

namespace Oathdeep.Enemies
{
    /// <summary>Melee grunt: chase, wind up (telegraphed), lunge slash, recover.</summary>
    public sealed class SkeletonAI : EnemyBase
    {
        enum S { Idle, Chase, Windup, Lunge, Recover }

        [SerializeField] float windup = 0.5f;
        [SerializeField] float lungeTime = 0.18f;
        [SerializeField] float lungeSpeed = 8f;
        [SerializeField] float recover = 0.65f;
        [SerializeField] AttackSpec slash = new AttackSpec(1f, 6f, 0f, 0.05f, 0.15f);

        S state;
        Vector2 lockedDir;
        Vector2 strafe;
        readonly HashSet<Health> hit = new HashSet<Health>();

        protected override void OnSpawned()
        {
            state = S.Idle;
            strafe = Random.value < 0.5f ? Vector2.left : Vector2.right;
        }

        void Go(S s) { state = s; StateTime = 0f; }

        protected override void Think(float dt)
        {
            switch (state)
            {
                case S.Idle:
                    anim.Play("idle");
                    if (DistanceToTarget < data.AggroRange && StateTime > 0.3f) Go(S.Chase);
                    break;

                case S.Chase:
                    anim.Play("run");
                    Face(ToTarget);
                    if (DistanceToTarget <= data.AttackRange + 0.4f)
                    {
                        lockedDir = ToTarget.normalized;
                        telegraph?.Show(windup, lockedDir, new Vector2(1.6f, 1.1f), lockedDir * 0.9f);
                        anim.Play("windup", true);
                        Go(S.Windup);
                    }
                    break;

                case S.Windup:
                    if (StateTime >= windup)
                    {
                        telegraph?.Hide();
                        hit.Clear();
                        anim.Play("attack", true);
                        AudioManager.Play(Sfx.Swing, transform.position, 0.6f);
                        Go(S.Lunge);
                    }
                    break;

                case S.Lunge:
                    HitQuery.StrikeBox(Stats, (Vector2)transform.position + lockedDir * 0.85f, new Vector2(1.3f, 1.0f),
                        Mathf.Atan2(lockedDir.y, lockedDir.x) * Mathf.Rad2Deg, slash, hit);
                    if (StateTime >= lungeTime) Go(S.Recover);
                    break;

                case S.Recover:
                    anim.Play("idle");
                    if (StateTime >= recover) Go(S.Chase);
                    break;
            }
        }

        protected override Vector2 DesiredVelocity()
        {
            switch (state)
            {
                case S.Chase:
                    // Slight sideways drift so groups spread out instead of stacking.
                    Vector2 dir = ToTarget.normalized;
                    return (dir + Vector2.Perpendicular(dir) * strafe.x * 0.25f).normalized * MoveSpeed;
                case S.Lunge:
                    return lockedDir * lungeSpeed;
                default:
                    return Vector2.zero;
            }
        }

        protected override void CancelAttack()
        {
            base.CancelAttack();
            if (state == S.Windup || state == S.Lunge) Go(S.Recover);
        }
    }
}
