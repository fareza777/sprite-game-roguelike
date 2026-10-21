using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>
    /// Flipbook for effects and animated props. One-shots return to the pool when done;
    /// looping ones run forever unless given a lifetime (e.g. spawn markers).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteVfx : MonoBehaviour
    {
        [SerializeField] Sprite[] frames;
        [SerializeField] float fps = 16f;
        [SerializeField] bool loop;
        [SerializeField] float lifetime;          // seconds; 0 = one pass (one-shot) or forever (loop)
        [SerializeField] bool randomStartFrame;   // desyncs rows of torches

        SpriteRenderer spriteRenderer;
        float startedAt;
        float offset;

        void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

        void OnEnable()
        {
            startedAt = Time.time;
            offset = randomStartFrame && frames != null ? Random.Range(0, frames.Length) / Mathf.Max(fps, 0.01f) : 0f;
            if (frames != null && frames.Length > 0) spriteRenderer.sprite = frames[0];
        }

        void Update()
        {
            if (frames == null || frames.Length == 0) return;
            float t = Time.time - startedAt;
            float life = lifetime > 0f ? lifetime : loop ? float.PositiveInfinity : frames.Length / fps;
            if (t >= life)
            {
                Pool.Despawn(gameObject);
                return;
            }
            int i = (int)((t + offset) * fps);
            spriteRenderer.sprite = frames[loop ? i % frames.Length : Mathf.Min(i, frames.Length - 1)];
        }

#if UNITY_EDITOR
        public void EditorInit(Sprite[] sequence, float rate, bool looping, float seconds, bool randomStart)
        {
            frames = sequence; fps = rate; loop = looping; lifetime = seconds; randomStartFrame = randomStart;
        }
#endif
    }
}
