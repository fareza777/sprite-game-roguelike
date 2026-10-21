using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>Smooth follow clamped to the current room, plus trauma-based shake.</summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraRig : MonoBehaviour
    {
        [SerializeField] float followSmoothTime = 0.12f;
        [SerializeField] float maxShakeOffset = 0.35f;
        [SerializeField] float traumaDecay = 1.8f;
        [SerializeField] float lookAhead = 0.6f;

        static CameraRig instance;
        Camera cam;
        Transform target;
        Rigidbody2D targetBody;
        readonly CameraBounds bounds = new CameraBounds();
        Vector3 velocity;
        Vector3 basePosition;
        float trauma;

        void Awake()
        {
            instance = this;
            cam = GetComponent<Camera>();
            basePosition = transform.position;
        }

        void OnDestroy() { if (instance == this) instance = null; }

        public static void Follow(Transform t)
        {
            if (instance == null) return;
            instance.target = t;
            instance.targetBody = t != null ? t.GetComponent<Rigidbody2D>() : null;
            if (t != null) instance.SnapTo(t.position);
        }

        public static void SetBounds(Rect worldRect)
        {
            if (instance == null) return;
            instance.bounds.Set(worldRect);
        }

        /// <summary>Called when the player leaves a room so the camera follows them down the corridor.</summary>
        public static void ReleaseBounds(Rect worldRect)
        {
            if (instance == null) return;
            instance.bounds.Release(worldRect);
        }

        public static void Shake(float amount)
        {
            if (instance == null || GameSettings.ReduceMotion) return;
            instance.trauma = Mathf.Clamp01(instance.trauma + amount * GameSettings.ScreenShake);
        }

        void SnapTo(Vector3 p)
        {
            basePosition = Clamp(new Vector3(p.x, p.y, transform.position.z));
            velocity = Vector3.zero;
        }

        void LateUpdate()
        {
            if (target != null)
            {
                Vector3 desired = target.position;
                if (targetBody != null) desired += (Vector3)(targetBody.linearVelocity.normalized * lookAhead);
                desired.z = transform.position.z;
                basePosition = Vector3.SmoothDamp(basePosition, Clamp(desired), ref velocity, followSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            }

            trauma = Mathf.Max(0f, trauma - traumaDecay * Time.unscaledDeltaTime);
            float s = trauma * trauma * maxShakeOffset;
            float t = Time.unscaledTime * 30f;
            var offset = new Vector3((Mathf.PerlinNoise(t, 0.1f) - 0.5f) * 2f * s, (Mathf.PerlinNoise(0.7f, t) - 0.5f) * 2f * s, 0f);
            transform.position = basePosition + offset;
        }

        Vector3 Clamp(Vector3 p)
        {
            float halfH = cam.orthographicSize;
            Vector2 c = bounds.Clamp(p, halfH * cam.aspect, halfH);
            return new Vector3(c.x, c.y, p.z);
        }
    }
}
