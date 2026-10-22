using System;
using Oathdeep.Audio;
using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// A breakable arena object tied to the boss: bone totems, root cores, spore pods, dusk anchors. It rises out of the
    /// floor, can be hit by anything that hits enemies (auto-aim included), and reports when it breaks.
    /// </summary>
    public sealed class BossAnchor : MonoBehaviour
    {
        SpriteRenderer sr;
        Health health;
        float bornAt;
        float flashUntil;

        public event Action<BossAnchor> Broken;
        public bool Alive => health != null && !health.IsDead;

        public static BossAnchor Spawn(Transform parent, Sprite sprite, Vector2 at, int hp, Vector2 hitbox)
        {
            var go = new GameObject("Anchor");
            go.transform.SetParent(parent, false);
            go.transform.position = at;
            var a = go.AddComponent<BossAnchor>();
            a.sr = go.AddComponent<SpriteRenderer>();
            a.sr.sprite = sprite;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = hitbox;
            box.offset = new Vector2(0f, hitbox.y / 2f);
            a.health = go.AddComponent<Health>();
            a.health.Init(hp, Team.Enemy);
            a.health.Damaged += a.OnHit;
            a.health.Died += a.OnBroken;
            TargetRegistry.Register(a.health);
            a.bornAt = Time.time;
            AudioManager.Play(Sfx.Spawn, at, 0.6f);
            return a;
        }

        void Update()
        {
            float rise = Mathf.Clamp01((Time.time - bornAt) / 0.35f);
            transform.localScale = new Vector3(1f, Mathf.SmoothStep(0.2f, 1f, rise), 1f);
            sr.color = Time.time < flashUntil ? new Color(1f, 1f, 1f, 1f) * 1.6f : Color.white;
        }

        void OnHit(DamageInfo info)
        {
            flashUntil = Time.time + 0.08f;
            CameraRig.Shake(0.08f);
        }

        void OnBroken()
        {
            TargetRegistry.Unregister(health);
            CameraRig.Shake(0.3f);
            AudioManager.Play(Sfx.BossSlam, transform.position, 0.5f);
            Broken?.Invoke(this);
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (health != null) TargetRegistry.Unregister(health);
        }
    }
}
