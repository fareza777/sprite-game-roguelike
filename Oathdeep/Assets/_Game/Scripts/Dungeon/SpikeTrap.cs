using System.Collections.Generic;
using Oathdeep.Combat;
using Oathdeep.Data;
using UnityEngine;
using Oathdeep.Audio;

namespace Oathdeep.Dungeon
{
    /// <summary>Floor spikes on a readable cycle: hidden → warning → up (hurts everyone) → hidden.</summary>
    [RequireComponent(typeof(CombatStats))]
    public sealed class SpikeTrap : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite hiddenSprite;
        [SerializeField] Sprite warnSprite;
        [SerializeField] Sprite upSprite;
        [SerializeField] float hiddenSeconds = 1.8f;
        [SerializeField] float warnSeconds = 0.5f;
        [SerializeField] float upSeconds = 0.6f;
        [SerializeField] int damage = 12;
        [SerializeField] AttackSpec spec = new AttackSpec(1f, 3f, 0f, 0.03f, 0.1f);

        CombatStats stats;
        float timer;
        int phase;
        readonly HashSet<Health> hitThisCycle = new HashSet<Health>();

        void Awake()
        {
            stats = GetComponent<CombatStats>();
            stats.Set(new StatBlock(1, damage, 0, 0f, 1f, 1f), Team.Hazard);
            timer = Random.Range(0f, hiddenSeconds); // desync neighbouring traps
            Apply();
        }

        void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                phase = (phase + 1) % 3;
                timer = phase == 0 ? hiddenSeconds : phase == 1 ? warnSeconds : upSeconds;
                if (phase == 2) { hitThisCycle.Clear(); AudioManager.Play(Sfx.Spike, transform.position, 0.6f); }
                Apply();
            }
            if (phase == 2) HitQuery.StrikeBox(stats, transform.position, new Vector2(0.8f, 0.8f), 0f, spec, hitThisCycle);
        }

        void Apply()
        {
            if (spriteRenderer == null) return;
            spriteRenderer.sprite = phase == 0 ? hiddenSprite : phase == 1 ? warnSprite : upSprite;
        }
    }
}
