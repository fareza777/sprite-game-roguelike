using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;
using Oathdeep.Audio;

namespace Oathdeep.Enemies
{
    /// <summary>Ranged: keeps its distance, draws (telegraphed aim line), fires one shot or a fan, repositions.
    /// Stationary shooters (sentries, eyes) never walk.</summary>
    public sealed class ArcherAI : EnemyBase
    {
        enum S { Idle, Reposition, Draw, Recover }

        [SerializeField] Projectile arrowPrefab;
        [SerializeField] float preferredMin = 3.2f;
        [SerializeField] float preferredMax = 5.5f;
        [SerializeField] float drawTime = 0.7f;
        [SerializeField] float arrowSpeed = 9f;
        [SerializeField] float recover = 0.9f;
        [SerializeField] AttackSpec arrow = new AttackSpec(1f, 4f, 0f, 0.04f, 0.12f);
        [SerializeField] int shots = 1;
        [SerializeField] float spread = 0f;
        [SerializeField] bool stationary;
        [SerializeField] Sfx shotSfx = Sfx.ArrowShoot;

        S state;
        Vector2 aimDir;
        Vector2 wander;

        protected override void OnSpawned() => Go(S.Idle);

        void Go(S s)
        {
            state = s;
            StateTime = 0f;
            if (s == S.Reposition) wander = Random.insideUnitCircle.normalized;
        }

        protected override void Think(float dt)
        {
            Face(ToTarget);
            switch (state)
            {
                case S.Idle:
                    anim.Play("idle");
                    if (DistanceToTarget < data.AggroRange && StateTime > 0.4f) Go(S.Reposition);
                    break;

                case S.Reposition:
                    anim.Play("run");
                    float d = DistanceToTarget;
                    if (StateTime > 0.8f && d >= preferredMin && d <= preferredMax + 1f)
                    {
                        aimDir = ToTarget.normalized;
                        telegraph?.Show(drawTime, aimDir, new Vector2(Mathf.Min(d, 6f), 0.18f), aimDir * Mathf.Min(d, 6f) * 0.5f);
                        anim.Play("attack", true);
                        Go(S.Draw);
                    }
                    break;

                case S.Draw:
                    // Track the player during the first half of the draw, then commit.
                    if (StateTime < drawTime * 0.5f)
                    {
                        aimDir = ToTarget.normalized;
                        float len = Mathf.Min(DistanceToTarget, 6f);
                        telegraph?.Show(drawTime - StateTime, aimDir, new Vector2(len, 0.18f), aimDir * len * 0.5f);
                    }
                    if (StateTime >= drawTime)
                    {
                        telegraph?.Hide();
                        Fire();
                        Go(S.Recover);
                    }
                    break;

                case S.Recover:
                    anim.Play("idle");
                    if (StateTime >= recover) Go(S.Reposition);
                    break;
            }
        }

        void Fire()
        {
            if (arrowPrefab == null) return;
            for (int i = 0; i < shots; i++)
            {
                float a = shots == 1 ? 0f : Mathf.Lerp(-spread * 0.5f, spread * 0.5f, i / (float)(shots - 1));
                Vector2 dir = Quaternion.Euler(0, 0, a) * aimDir;
                var p = Pool.Spawn(arrowPrefab, (Vector2)transform.position + Vector2.up * 0.4f + dir * 0.5f, Quaternion.identity);
                p.Launch(Stats, dir, arrowSpeed, arrow);
            }
            AudioManager.Play(shotSfx, transform.position);
        }

        protected override Vector2 DesiredVelocity()
        {
            if (state != S.Reposition || stationary) return Vector2.zero;
            float d = DistanceToTarget;
            Vector2 dir = ToTarget.normalized;
            Vector2 v = d < preferredMin ? -dir : d > preferredMax ? dir : Vector2.Perpendicular(dir) * Mathf.Sign(wander.x);
            return (v + wander * 0.3f).normalized * MoveSpeed;
        }

        protected override void CancelAttack()
        {
            base.CancelAttack();
            if (state == S.Draw) Go(S.Recover);
        }
    }
}
