using UnityEngine;

namespace Oathdeep.Combat
{
    /// <summary>
    /// Draws a solid-colour silhouette over the sprite for a few frames when hit.
    /// Uses a child SpriteRenderer with the Oathdeep/SpriteFlash shader so 2D lighting on the base sprite is untouched.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class HitFlash : MonoBehaviour
    {
        static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");

        [SerializeField] Material flashMaterial;
        [SerializeField] float duration = 0.08f;

        SpriteRenderer source;
        SpriteRenderer overlay;
        MaterialPropertyBlock block;
        float until;

        void Awake()
        {
            source = GetComponent<SpriteRenderer>();
            block = new MaterialPropertyBlock();
            var go = new GameObject("Flash");
            go.transform.SetParent(transform, false);
            overlay = go.AddComponent<SpriteRenderer>();
            overlay.sharedMaterial = flashMaterial;
            overlay.sortingLayerID = source.sortingLayerID;
            overlay.sortingOrder = source.sortingOrder + 1;
            overlay.enabled = false;
        }

        public void SetMaterial(Material m)
        {
            flashMaterial = m;
            if (overlay != null) overlay.sharedMaterial = m;
        }

        public void Flash(Color color, float seconds = -1f)
        {
            if (flashMaterial == null) return;
            until = Time.time + (seconds > 0f ? seconds : duration);
            overlay.GetPropertyBlock(block);
            block.SetColor(FlashColorId, color);
            overlay.SetPropertyBlock(block);
            overlay.enabled = true;
        }

        void LateUpdate()
        {
            if (!overlay.enabled) return;
            overlay.sprite = source.sprite;
            overlay.flipX = source.flipX;
            overlay.sortingOrder = source.sortingOrder + 1;
            if (Time.time >= until) overlay.enabled = false;
        }
    }
}
