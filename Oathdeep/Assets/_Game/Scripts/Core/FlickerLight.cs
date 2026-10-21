using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Oathdeep.Core
{
    /// <summary>Torch / candle flicker for a 2D light. Cheap Perlin noise, desynced per instance.</summary>
    [RequireComponent(typeof(Light2D))]
    public sealed class FlickerLight : MonoBehaviour
    {
        [SerializeField] float baseIntensity = 1.1f;
        [SerializeField] float intensityJitter = 0.25f;
        [SerializeField] float radiusJitter = 0.3f;
        [SerializeField] float speed = 7f;

        Light2D light2D;
        float baseRadius;
        float seed;

        void Awake()
        {
            light2D = GetComponent<Light2D>();
            baseRadius = light2D.pointLightOuterRadius;
            seed = Random.value * 100f;
        }

        void Update()
        {
            float n = Mathf.PerlinNoise(seed, Time.time * speed) - 0.5f;
            light2D.intensity = baseIntensity + n * 2f * intensityJitter;
            light2D.pointLightOuterRadius = baseRadius + n * 2f * radiusJitter;
        }
    }
}
