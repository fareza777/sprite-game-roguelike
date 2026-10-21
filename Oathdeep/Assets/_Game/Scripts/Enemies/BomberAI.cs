using Oathdeep.Audio;
using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>Scurries at the player, hisses for a moment (telegraphed ring), then bursts. Killing it first is safe.</summary>
    public sealed class BomberAI : EnemyBase
    {
        enum S { Idle, Rush, Fuse }

        [SerializeField] float fuse = 0.75f;
        [SerializeField] float triggerRange = 1.3f;
        [SerializeField] float radius = 1.6f;
        [SerializeField] GameObject burstVfx;
        [SerializeField] AttackSpec burst = new AttackSpec(2f, 9f, 0.3f, 0.08f, 0.35f);

        S state;
        bool exploded;

        protected override void OnSpawned()
        {
            state = S.Idle;
            exploded = false;
        }

        void Go(S s) { state = s; StateTime = 0f; }

        protected override void Think(float dt)
        {
            switch (state)
            {
                case S.Idle:
                    anim.Play("idle");
                    if (DistanceToTarget < data.AggroRange && StateTime > 0.3f) Go(S.Rush);
                    break;
                case S.Rush:
                    anim.Play("run");
                    Face(ToTarget);
                    if (DistanceToTarget <= triggerRange)
                    {
                        telegraph?.Show(fuse, Vector2.right, new Vector2(radius * 2f, radius * 2f), Vector2.zero);
                        anim.Play("windup", true);
                        AudioManager.Play(Sfx.Spike, transform.position, 0.8f);
                        Go(S.Fuse);
                    }
                    break;
                case S.Fuse:
                    if (StateTime >= fuse) Explode();
                    break;
            }
        }

        void Explode()
        {
            if (exploded) return;
            exploded = true;
            telegraph?.Hide();
            HitQuery.StrikeCircle(Stats, transform.position, radius, burst, null);
            if (burstVfx != null) Pool.Spawn(burstVfx, transform.position, Quaternion.identity).transform.localScale = Vector3.one * radius * 0.9f;
            AudioManager.Play(Sfx.Explosion, transform.position, 0.8f);
            CameraRig.Shake(0.25f);
            // The blast consumes the bomber; it still counts as a kill and drops loot.
            Health.TryTakeDamage(new DamageInfo(99999, false, transform.position, 0f, 0f, Team.Hazard));
        }

        protected override Vector2 DesiredVelocity() => state == S.Rush ? ToTarget.normalized * MoveSpeed * 1.25f : Vector2.zero;

        protected override void CancelAttack()
        {
            // A stunned fuse keeps burning.
        }
    }
}
