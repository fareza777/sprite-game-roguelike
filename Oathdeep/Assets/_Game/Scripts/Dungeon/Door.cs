using UnityEngine;
using Oathdeep.Audio;

namespace Oathdeep.Dungeon
{
    /// <summary>Portcullis that seals a room during combat. Collider + sprite swap.</summary>
    public sealed class Door : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] BoxCollider2D blocker;
        [SerializeField] Sprite openSprite;
        [SerializeField] Sprite closedSprite;

        public bool IsClosed { get; private set; }

        public void Setup(Sprite open, Sprite closed, Vector2 size, bool vertical)
        {
            if (open != null) openSprite = open;
            if (closed != null) closedSprite = closed;
            if (blocker == null) blocker = gameObject.AddComponent<BoxCollider2D>();
            blocker.size = size;
            if (spriteRenderer == null)
            {
                var go = new GameObject("Gate");
                go.transform.SetParent(transform, false);
                spriteRenderer = go.AddComponent<SpriteRenderer>();
            }
            spriteRenderer.drawMode = SpriteDrawMode.Tiled;
            spriteRenderer.size = vertical ? new Vector2(1f, size.y) : new Vector2(size.x, 1f);
            SetClosed(false);
        }

        public void SetClosed(bool closed)
        {
            if (closed != IsClosed) AudioManager.Play(closed ? Sfx.DoorSlam : Sfx.DoorOpen, transform.position);
            IsClosed = closed;
            if (blocker != null) blocker.enabled = closed;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = closed ? closedSprite : openSprite;
                spriteRenderer.enabled = spriteRenderer.sprite != null;
            }
        }
    }
}
