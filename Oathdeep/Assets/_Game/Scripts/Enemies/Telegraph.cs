using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>Warning marker shown before an enemy attack lands (blinking, growing).</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Telegraph : MonoBehaviour
    {
        [SerializeField] Color color = new Color(1f, 0.25f, 0.2f, 0.55f);

        SpriteRenderer sr;
        float shownAt;
        float duration;
        Vector3 fullScale = Vector3.one;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            gameObject.SetActive(false);
        }

        /// <summary>Show for `seconds`, pointing along `direction`, scaled to `size` (world units).</summary>
        public void Show(float seconds, Vector2 direction, Vector2 size, Vector2 localOffset)
        {
            gameObject.SetActive(true);
            shownAt = Time.time;
            duration = Mathf.Max(0.05f, seconds);
            fullScale = new Vector3(size.x, size.y, 1f);
            transform.localPosition = localOffset;
            transform.right = direction.sqrMagnitude > 0f ? (Vector3)direction : Vector3.right;
            Update();
        }

        public void Hide() => gameObject.SetActive(false);

        void Update()
        {
            float t = Mathf.Clamp01((Time.time - shownAt) / duration);
            transform.localScale = Vector3.Lerp(fullScale * 0.4f, fullScale, t);
            float blink = t > 0.7f ? (Mathf.Sin(Time.time * 60f) > 0f ? 1f : 0.35f) : 1f;
            sr.color = new Color(color.r, color.g, color.b, color.a * blink * Mathf.Lerp(0.5f, 1f, t));
        }
    }
}
