using UnityEngine;
using Oathdeep.Audio;

namespace Oathdeep.Enemies
{
    /// <summary>Hops toward the player (contact damage); large slimes split into two small ones on death.</summary>
    public sealed class SlimeAI : EnemyBase
    {
        enum S { Rest, Squash, Hop }

        [SerializeField] float restTime = 0.7f;
        [SerializeField] float squashTime = 0.25f;
        [SerializeField] float hopTime = 0.35f;
        [SerializeField] float hopSpeed = 6f;
        [SerializeField] SlimeAI splitPrefab;
        [SerializeField] int splitCount = 2;

        S state;
        Vector2 hopDir;

        protected override void OnSpawned()
        {
            state = S.Rest;
            StateTime = Random.Range(0f, restTime);
        }

        void Go(S s) { state = s; StateTime = 0f; }

        protected override void Think(float dt)
        {
            switch (state)
            {
                case S.Rest:
                    anim.Play("idle");
                    if (StateTime >= restTime && DistanceToTarget < data.AggroRange)
                    {
                        hopDir = (ToTarget.normalized + Random.insideUnitCircle * 0.25f).normalized;
                        Face(hopDir);
                        anim.Play("windup", true);
                        Go(S.Squash);
                    }
                    break;
                case S.Squash:
                    if (StateTime >= squashTime) { anim.Play("attack", true); AudioManager.Play(Sfx.SlimeSquish, transform.position, 0.7f); Go(S.Hop); }
                    break;
                case S.Hop:
                    if (StateTime >= hopTime) Go(S.Rest);
                    break;
            }
        }

        protected override Vector2 DesiredVelocity() => state == S.Hop ? hopDir * hopSpeed * (1f - StateTime / hopTime * 0.5f) : Vector2.zero;

        protected override void OnDeath()
        {
            if (splitPrefab == null) return;
            for (int i = 0; i < splitCount; i++)
            {
                Vector2 offset = Quaternion.Euler(0, 0, 360f / splitCount * i) * Vector2.right * 0.4f;
                var child = Instantiate(splitPrefab, (Vector2)transform.position + offset, Quaternion.identity, transform.parent);
                child.Spawn(Target);
            }
        }
    }
}
