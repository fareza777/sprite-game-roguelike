using System.Collections.Generic;
using Oathdeep.Audio;
using Oathdeep.Combat;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>Hunter: circles at mid range, crouches (telegraphed line), then dashes through the player's position.</summary>
    public sealed class ChargerAI : EnemyBase
    {
        enum S { Idle, Stalk, Windup, Dash, Recover }

        [SerializeField] float stalkRange = 4.5f;
        [SerializeField] float windup = 0.55f;
        [SerializeField] float dashSpeed = 11f;
        [SerializeField] float dashTime = 0.35f;
        [SerializeField] float recover = 0.8f;
        [SerializeField] AttackSpec bite = new AttackSpec(1.2f, 7f, 0.1f, 0.06f, 0.18f);

        S state;
        Vector2 dir;
        float orbit;
        readonly HashSet<Health> hit = new HashSet<Health>();

        protected override void OnSpawned()
        {
            Go(S.Idle);
            orbit = Random.value < 0.5f ? 1f : -1f;
        }

        void Go(S s) { state = s; StateTime = 0f; }

        protected override void Think(float dt)
        {
            switch (state)
            {
                case S.Idle:
                    anim.Play("idle");
                    if (DistanceToTarget < data.AggroRange && StateTime > 0.3f) Go(S.Stalk);
                    break;
                case S.Stalk:
                    anim.Play("run");
                    Face(ToTarget);
                    if (StateTime > 0.9f && DistanceToTarget < stalkRange + 1.5f)
                    {
                        dir = ToTarget.normalized;
                        float len = Mathf.Min(dashSpeed * dashTime, 7f);
                        telegraph?.Show(windup, dir, new Vector2(len, 0.9f), dir * len * 0.5f);
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
                        AudioManager.Play(Sfx.Dodge, transform.position, 0.8f);
                        Go(S.Dash);
                    }
                    break;
                case S.Dash:
                    HitQuery.StrikeCircle(Stats, (Vector2)transform.position + dir * 0.4f, 0.65f, bite, hit);
                    if (StateTime >= dashTime) Go(S.Recover);
                    break;
                case S.Recover:
                    anim.Play("idle");
                    if (StateTime >= recover) Go(S.Stalk);
                    break;
            }
        }

        protected override Vector2 DesiredVelocity()
        {
            switch (state)
            {
                case S.Stalk:
                    Vector2 to = ToTarget.normalized;
                    float d = DistanceToTarget;
                    Vector2 radial = d > stalkRange ? to : d < stalkRange - 1.5f ? -to : Vector2.zero;
                    return (radial + Vector2.Perpendicular(to) * orbit * 0.8f).normalized * MoveSpeed;
                case S.Dash:
                    return dir * dashSpeed * Mathf.Lerp(1.1f, 0.6f, StateTime / dashTime);
                default:
                    return Vector2.zero;
            }
        }

        protected override void CancelAttack()
        {
            base.CancelAttack();
            if (state == S.Windup || state == S.Dash) Go(S.Recover);
        }
    }
}
