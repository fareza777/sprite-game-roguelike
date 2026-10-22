using System.Collections.Generic;
using Oathdeep.Combat;
using UnityEngine;

namespace Oathdeep.Player
{
    /// <summary>A small patch of flame left by an Ember Trail roll: it burns foes standing in it, then gutters out.</summary>
    public sealed class FireTrail : MonoBehaviour
    {
        const float Life = 2f;
        const float Radius = 0.7f;
        static readonly AttackSpec Burn = new AttackSpec(0.25f, 0f, 0f, 0f, 0f);

        CombatStats owner;
        SpriteRenderer sr;
        Sprite[] frames;
        float bornAt, nextBurn;
        readonly HashSet<Health> burned = new HashSet<Health>();

        public static void Spawn(Vector2 at, CombatStats owner, Sprite[] frames, Material glow)
        {
            var go = new GameObject("FireTrail");
            go.transform.position = at;
            var f = go.AddComponent<FireTrail>();
            f.owner = owner;
            f.frames = frames;
            f.bornAt = Time.time;
            f.sr = go.AddComponent<SpriteRenderer>();
            f.sr.sortingOrder = -60;
            if (glow != null) f.sr.sharedMaterial = glow;
            if (frames != null && frames.Length > 0) f.sr.sprite = frames[0];
        }

        void Update()
        {
            float age = Time.time - bornAt;
            if (age >= Life) { Destroy(gameObject); return; }
            if (frames != null && frames.Length > 0) sr.sprite = frames[(int)(age * 10f) % frames.Length];
            sr.color = new Color(1f, 1f, 1f, Mathf.Clamp01((Life - age) / 0.5f));
            if (Time.time < nextBurn || owner == null) return;
            nextBurn = Time.time + 0.4f;
            burned.Clear();   // each pulse may hit again
            HitQuery.StrikeCircle(owner, transform.position, Radius, Burn, burned);
        }
    }
}
