using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>The Pharaoh's sandstorm: a soft dust vignette around the screen edges; the play area around the hero stays clear.</summary>
    public sealed class SandFog : MonoBehaviour
    {
        const float WorldSize = 40f;   // the 640 px veil at 16 px per unit

        Transform follow;
        SpriteRenderer sr;
        float alpha, wanted;

        public static SandFog Spawn(Transform parent, Sprite sprite, Transform follow)
        {
            var go = new GameObject("Sandstorm");
            go.transform.SetParent(parent, false);
            var f = go.AddComponent<SandFog>();
            f.follow = follow;
            f.sr = go.AddComponent<SpriteRenderer>();
            f.sr.sprite = sprite;
            f.sr.sortingOrder = 400;
            float native = sprite != null ? sprite.bounds.size.x : 1f;
            go.transform.localScale = Vector3.one * (WorldSize / Mathf.Max(0.01f, native));
            f.sr.color = new Color(1f, 1f, 1f, 0f);
            return f;
        }

        public void Set(bool on) => wanted = on ? 0.55f : 0f;   // a soft vignette: the play area stays clear

        void LateUpdate()
        {
            if (follow != null) transform.position = follow.position;
            alpha = Mathf.MoveTowards(alpha, wanted, Time.deltaTime * 0.8f);
            sr.color = new Color(1f, 1f, 1f, alpha);
        }
    }
}
