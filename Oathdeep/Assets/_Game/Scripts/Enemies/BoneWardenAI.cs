using System.Collections.Generic;
using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;
using Oathdeep.Audio;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// Miniboss. Picks between Sweep (wide melee), Slam (AoE + bone-shard ring) and Summon (skeletons).
    /// Phase 2 below 50% HP: faster, denser rings, more adds.
    /// </summary>
    public sealed class BoneWardenAI : EnemyBase
    {
        enum S { Intro, Chase, SweepWindup, Sweep, SlamWindup, Slam, SummonCast, Recover }

        [SerializeField] Projectile shardPrefab;
        [SerializeField] EnemyBase minionPrefab;
        [SerializeField] AttackSpec sweepSpec = new AttackSpec(1.3f, 9f, 0.1f, 0.08f, 0.3f);
        [SerializeField] AttackSpec slamSpec = new AttackSpec(1.6f, 12f, 0.25f, 0.1f, 0.5f);
        [SerializeField] AttackSpec shardSpec = new AttackSpec(0.7f, 4f, 0f, 0.03f, 0.1f);

        S state;
        Vector2 lockedDir;
        bool phaseTwo;
        int attacksSinceSummon;
        readonly HashSet<Health> hit = new HashSet<Health>();

        float Pace => phaseTwo ? 0.75f : 1f;

        protected override void OnSpawned()
        {
            Go(S.Intro);
            GameEvents.RaiseBossEncountered(data.DisplayName);
            GameEvents.RaiseBossHealthChanged(Health.Current, Health.Max);
            Health.Changed += (cur, max) => GameEvents.RaiseBossHealthChanged(cur, max);
        }

        void Go(S s) { state = s; StateTime = 0f; }

        protected override void Think(float dt)
        {
            if (!phaseTwo && Health.Current <= Health.Max / 2)
            {
                phaseTwo = true;
                flash?.Flash(new Color(1f, 0.4f, 0.1f), 0.3f);
                CameraRig.Shake(0.6f);
                GameEvents.RaiseToast("The Warden rages!");
            }

            switch (state)
            {
                case S.Intro:
                    anim.Play("idle");
                    if (StateTime > 1.2f) Go(S.Chase);
                    break;

                case S.Chase:
                    anim.Play("run");
                    Face(ToTarget);
                    if (StateTime > 0.6f * Pace) ChooseAttack();
                    break;

                case S.SweepWindup:
                    if (StateTime >= 0.7f * Pace) { telegraph?.Hide(); hit.Clear(); anim.Play("attack", true); AudioManager.Play(Sfx.SwingHeavy, transform.position); Go(S.Sweep); }
                    break;

                case S.Sweep:
                    HitQuery.StrikeBox(Stats, (Vector2)transform.position + lockedDir * 1.3f, new Vector2(2.2f, 3.0f),
                        Mathf.Atan2(lockedDir.y, lockedDir.x) * Mathf.Rad2Deg, sweepSpec, hit);
                    if (StateTime >= 0.2f) Go(S.Recover);
                    break;

                case S.SlamWindup:
                    if (StateTime >= 0.9f * Pace) { telegraph?.Hide(); DoSlam(); Go(S.Slam); }
                    break;

                case S.Slam:
                    if (StateTime >= 0.35f) Go(S.Recover);
                    break;

                case S.SummonCast:
                    if (StateTime >= 0.8f * Pace) { DoSummon(); Go(S.Recover); }
                    break;

                case S.Recover:
                    anim.Play("idle");
                    if (StateTime >= 0.9f * Pace) Go(S.Chase);
                    break;
            }
        }

        void ChooseAttack()
        {
            lockedDir = ToTarget.normalized;
            Face(lockedDir);
            float d = DistanceToTarget;

            if (attacksSinceSummon >= (phaseTwo ? 2 : 3) && minionPrefab != null)
            {
                attacksSinceSummon = 0;
                anim.Play("cast", true);
                AudioManager.Play(Sfx.BossCast, transform.position);
                Go(S.SummonCast);
                return;
            }

            attacksSinceSummon++;
            if (d < 2.6f && Random.value < 0.6f)
            {
                telegraph?.Show(0.7f * Pace, lockedDir, new Vector2(2.4f, 3.0f), lockedDir * 1.3f);
                anim.Play("windup", true);
                Go(S.SweepWindup);
            }
            else
            {
                telegraph?.Show(0.9f * Pace, Vector2.right, new Vector2(4.6f, 4.6f), Vector2.zero);
                anim.Play("slam_windup", true);
                Go(S.SlamWindup);
            }
        }

        void DoSlam()
        {
            anim.Play("slam", true);
            AudioManager.Play(Sfx.BossSlam, transform.position);
            hit.Clear();
            HitQuery.StrikeCircle(Stats, transform.position, 2.2f, slamSpec, hit);
            CameraRig.Shake(0.55f);
            Haptics.Pulse(60);
            if (shardPrefab == null) return;

            int count = phaseTwo ? 16 : 10;
            float offset = Random.value * 360f;
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = Quaternion.Euler(0, 0, offset + 360f / count * i) * Vector2.right;
                var p = Pool.Spawn(shardPrefab, (Vector2)transform.position + dir * 0.6f, Quaternion.identity);
                p.Launch(Stats, dir, phaseTwo ? 6.5f : 5.5f, shardSpec);
            }
        }

        void DoSummon()
        {
            int n = phaseTwo ? 3 : 2;
            for (int i = 0; i < n; i++)
            {
                Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle.normalized * 1.8f;
                var m = Instantiate(minionPrefab, pos, Quaternion.identity, transform.parent);
                m.Spawn(Target);
            }
        }

        protected override Vector2 DesiredVelocity() => state == S.Chase && DistanceToTarget > 1.6f ? ToTarget.normalized * MoveSpeed * (phaseTwo ? 1.25f : 1f) : Vector2.zero;

        protected override void CancelAttack()
        {
            // Bosses shrug off stuns mid-windup (knockback resistance covers most of it).
        }

        protected override void OnDeath()
        {
            CameraRig.Shake(0.8f);
            HitStop.Trigger(0.25f);
            GameEvents.RaiseBossDefeated();
        }
    }
}
