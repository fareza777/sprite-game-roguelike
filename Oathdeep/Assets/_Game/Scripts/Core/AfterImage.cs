using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>A fading copy of a sprite left behind (the hero's roll). Pure visual, no collider.</summary>
    public sealed class AfterImage : MonoBehaviour
    {
        const float Life = 0.22f;

        SpriteRenderer sr;
        Color start;
        float bornAt;

        public static void Spawn(SpriteRenderer source, Color tint)
        {
            if (source == null || source.sprite == null || GameSettings.ReduceMotion) return;
            var go = new GameObject("AfterImage");
            go.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            go.transform.localScale = source.transform.lossyScale;
            var a = go.AddComponent<AfterImage>();
            a.sr = go.AddComponent<SpriteRenderer>();
            a.sr.sprite = source.sprite;
            a.sr.flipX = source.flipX;
            a.sr.sortingOrder = source.sortingOrder - 1;
            a.start = tint;
            a.bornAt = Time.time;
            a.sr.color = tint;
        }

        void Update()
        {
            float t = (Time.time - bornAt) / Life;
            if (t >= 1f) { Destroy(gameObject); return; }
            sr.color = new Color(start.r, start.g, start.b, start.a * (1f - t));
        }
    }
}
