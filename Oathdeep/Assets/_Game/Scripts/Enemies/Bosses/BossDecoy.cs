using System;
using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// A mirror image of the boss: it copies the boss's current frame, has no shadow (the tell), and shatters into
    /// shards when struck, then reforms. The boss can swap places with it.
    /// </summary>
    public sealed class BossDecoy : MonoBehaviour
    {
        SpriteRenderer mine;
        SpriteRenderer source;
        Health health;
        Collider2D hurtbox;
        Color tint;
        int maxHp;
        float hiddenUntil;
        bool registered;

        public event Action<BossDecoy> Shattered;
        public bool Visible => Time.time >= hiddenUntil;

        public static BossDecoy Spawn(Transform parent, SpriteRenderer source, Vector2 at, int hp, Vector2 hitbox, Color tint)
        {
            var go = new GameObject("Decoy");
            go.transform.SetParent(parent, false);
            go.transform.position = at;
            var d = go.AddComponent<BossDecoy>();
            d.source = source;
            d.tint = tint;
            d.maxHp = hp;
            d.mine = go.AddComponent<SpriteRenderer>();
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = hitbox;
            box.offset = new Vector2(0f, hitbox.y / 2f);
            d.hurtbox = box;
            d.health = go.AddComponent<Health>();
            d.health.Init(hp, Team.Enemy);
            d.health.Damaged += d.OnHit;
            return d;
        }

        void LateUpdate()
        {
            bool visible = Visible && source != null;
            mine.enabled = visible;
            hurtbox.enabled = visible;
            if (visible != registered)
            {
                registered = visible;   // auto-aim only sees a decoy that is standing there
                if (visible) TargetRegistry.Register(health); else TargetRegistry.Unregister(health);
            }
            if (!visible) return;
            mine.sprite = source.sprite;
            mine.flipX = source.flipX;
            mine.sortingOrder = source.sortingOrder;
            transform.localScale = source.transform.lossyScale;
            mine.color = tint;
        }

        void OnHit(DamageInfo info)
        {
            Hide(6f);
            Shattered?.Invoke(this);
        }

        /// <summary>Gone for `seconds`, then reforms whole.</summary>
        public void Hide(float seconds)
        {
            hiddenUntil = Time.time + seconds;
            health.Init(maxHp, Team.Enemy);
        }

        void OnDestroy()
        {
            if (health != null) TargetRegistry.Unregister(health);
        }
    }
}
