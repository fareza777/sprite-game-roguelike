using System.Collections.Generic;
using Oathdeep.Audio;
using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// A Chorister: keeps a fighting distance and cycles through its moves from <see cref="BossPatterns"/>.
    /// Half health unlocks phase two (faster, extra moves); the Cantor also gets a third phase at a quarter.
    /// </summary>
    public sealed class PatternBossAI : EnemyBase
    {
        enum S { Intro, Chase, Windup, Act, Recover }

        [SerializeField] string patternId;
        [SerializeField] Projectile shot;
        [SerializeField] EnemyBase minion;
        [SerializeField] GroundBlast blast;
        [SerializeField] AttackSpec shotSpec = new AttackSpec(0.8f, 4f, 0f, 0.03f, 0.08f);
        [SerializeField] AttackSpec slamSpec = new AttackSpec(1.7f, 12f, 0.25f, 0.1f, 0.5f);
        [SerializeField] AttackSpec chargeSpec = new AttackSpec(1.5f, 12f, 0.3f, 0.08f, 0.35f);
        [SerializeField] AttackSpec blastSpec = new AttackSpec(1.4f, 6f, 0.2f, 0.06f, 0.2f);

        IReadOnlyList<BossMoveSpec> moves;
        BossMoveSpec current;
        S state;
        int phase = 1;
        Vector2 lockedDir;
        float spiralAngle;
        float nextSpiralShot;
        int fired;
        BossMove? lastMove;
        readonly HashSet<Health> hit = new HashSet<Health>();
        readonly System.Random rng = new System.Random();

        float Pace => phase == 1 ? 1f : phase == 2 ? 0.78f : 0.62f;

        protected override void OnSpawned()
        {
            moves = BossPatterns.For(patternId);
            Go(S.Intro);
            GameEvents.RaiseBossEncountered(data.DisplayName);
            GameEvents.RaiseBossHealthChanged(Health.Current, Health.Max);
            Health.Changed += (cur, max) => GameEvents.RaiseBossHealthChanged(cur, max);
        }

        void Go(S s) { state = s; StateTime = 0f; }

        protected override void Think(float dt)
        {
            UpdatePhase();
            switch (state)
            {
                case S.Intro:
                    anim.Play("idle");
                    if (StateTime > 1.2f) Go(S.Chase);
                    break;
                case S.Chase:
                    anim.Play("run");
                    Face(ToTarget);
                    if (StateTime > 0.7f * Pace) Begin();
                    break;
                case S.Windup:
                    if (StateTime >= current.Windup * Pace) { telegraph?.Hide(); fired = 0; Go(S.Act); Execute(); }
                    break;
                case S.Act:
                    Continue();
                    break;
                case S.Recover:
                    anim.Play("idle");
                    if (StateTime >= 0.8f * Pace) Go(S.Chase);
                    break;
            }
        }

        void UpdatePhase()
        {
            int want = Health.Current <= Health.Max / 2 ? 2 : 1;
            if (patternId == "the_cantor" && Health.Current <= Health.Max / 4) want = 3;
            if (want <= phase) return;
            phase = want;
            flash?.Flash(new Color(1f, 0.45f, 0.15f), 0.35f);
            CameraRig.Shake(0.6f);
            AudioManager.Play(Sfx.BossRoar, transform.position);
            GameEvents.RaiseToast(phase == 3 ? $"{data.DisplayName} sings the last verse!" : $"{data.DisplayName} rages!");
        }

        void Begin()
        {
            current = BossPatterns.Pick(moves, phase >= 2, rng, lastMove);
            lastMove = current.Move;
            lockedDir = ToTarget.normalized;
            Face(lockedDir);
            switch (current.Move)
            {
                case BossMove.Charge:
                    telegraph?.Show(current.Windup * Pace, lockedDir, new Vector2(7f, 1.4f), lockedDir * 3.5f);
                    anim.Play("windup", true);
                    break;
                case BossMove.Slam:
                    telegraph?.Show(current.Windup * Pace, Vector2.right, Vector2.one * current.Radius * 2f, Vector2.zero);
                    anim.Play("windup", true);
                    break;
                default:
                    anim.Play("cast", true);
                    AudioManager.Play(Sfx.BossCast, transform.position);
                    break;
            }
            Go(S.Windup);
        }

        void Execute()
        {
            switch (current.Move)
            {
                case BossMove.Radial:
                    Burst(current.Count, rng.Next(360));
                    break;
                case BossMove.AimedFan:
                    Fan(ToTarget.normalized, current.Count, current.Spread);
                    break;
                case BossMove.Slam:
                    anim.Play("attack", true);
                    hit.Clear();
                    HitQuery.StrikeCircle(Stats, transform.position, current.Radius, slamSpec, hit);
                    AudioManager.Play(Sfx.BossSlam, transform.position);
                    CameraRig.Shake(0.55f);
                    Haptics.Pulse(60);
                    Burst(current.Count, rng.Next(360));
                    break;
                case BossMove.Rain:
                    Vector2 at = Target.position;
                    int n = current.Count + (phase >= 3 ? 3 : 0);
                    for (int i = 0; i < n; i++)
                    {
                        Vector2 offset = i == 0 ? Vector2.zero : (Vector2)(Random.insideUnitCircle * 4f);
                        GroundBlast.Spawn(blast, at + offset, current.Radius, 0.9f * Pace + i * 0.08f, Stats, blastSpec);
                    }
                    break;
                case BossMove.Summon:
                    if (minion == null) break;
                    for (int i = 0; i < current.Count + (phase - 1); i++)
                    {
                        Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle.normalized * 2f;
                        Instantiate(minion, pos, Quaternion.identity, transform.parent).Spawn(Target);
                    }
                    break;
                case BossMove.Charge:
                    anim.Play("attack", true);
                    hit.Clear();
                    AudioManager.Play(Sfx.SwingHeavy, transform.position);
                    break;
                case BossMove.Spiral:
                    spiralAngle = rng.Next(360);
                    nextSpiralShot = 0f;
                    break;
            }
        }

        /// <summary>Moves that last over time (spiral, charge); everything else finishes on the first tick.</summary>
        void Continue()
        {
            switch (current.Move)
            {
                case BossMove.Spiral:
                    float duration = 2.2f;
                    if (StateTime >= nextSpiralShot && fired < current.Count)
                    {
                        nextSpiralShot += duration / current.Count;
                        spiralAngle += current.Spread * duration / current.Count;
                        Shoot(Quaternion.Euler(0, 0, spiralAngle) * Vector2.right, current.Speed);
                        Shoot(Quaternion.Euler(0, 0, spiralAngle + 180f) * Vector2.right, current.Speed);
                        fired++;
                    }
                    if (StateTime >= duration) Go(S.Recover);
                    break;
                case BossMove.Charge:
                    HitQuery.StrikeCircle(Stats, transform.position, 1.2f, chargeSpec, hit);
                    if (StateTime >= 0.55f) Go(S.Recover);
                    break;
                default:
                    if (StateTime >= 0.35f) Go(S.Recover);
                    break;
            }
        }

        void Burst(int count, float offset)
        {
            count += (phase - 1) * 4;
            for (int i = 0; i < count; i++)
                Shoot(Quaternion.Euler(0, 0, offset + 360f / count * i) * Vector2.right, current.Speed > 0 ? current.Speed : 5f);
        }

        void Fan(Vector2 aim, int count, float spread)
        {
            count += phase - 1;
            for (int i = 0; i < count; i++)
            {
                float a = count == 1 ? 0f : Mathf.Lerp(-spread * 0.5f, spread * 0.5f, i / (float)(count - 1));
                Shoot(Quaternion.Euler(0, 0, a) * aim, current.Speed);
            }
        }

        void Shoot(Vector2 dir, float speed)
        {
            if (shot == null) return;
            var p = Pool.Spawn(shot, (Vector2)transform.position + Vector2.up * 0.8f + dir * 0.8f, Quaternion.identity);
            p.Launch(Stats, dir, speed, shotSpec);
            AudioManager.Play(Sfx.EnemyShot, transform.position, 0.5f);
        }

        protected override Vector2 DesiredVelocity()
        {
            if (state == S.Act && current.Move == BossMove.Charge) return lockedDir * current.Speed * Mathf.Lerp(1.2f, 0.5f, StateTime / 0.55f);
            if (state != S.Chase) return Vector2.zero;
            float d = DistanceToTarget;
            Vector2 to = ToTarget.normalized;
            Vector2 v = d > 4.5f ? to : d < 2.5f ? -to : Vector2.Perpendicular(to) * 0.6f;
            return v * MoveSpeed * (phase >= 2 ? 1.2f : 1f);
        }

        protected override void CancelAttack()
        {
            // Choristers do not flinch out of their hymns.
        }

        protected override void OnDeath()
        {
            CameraRig.Shake(0.8f);
            HitStop.Trigger(0.25f);
            GameEvents.RaiseBossDefeated();
        }
    }
}
