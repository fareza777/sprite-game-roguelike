using System.Collections.Generic;
using Oathdeep.Audio;
using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Player
{
    /// <summary>
    /// The hero's run-long talents and their effects. Bursts and shockwaves are queued and resolved on the next
    /// frame: they are triggered from inside another hit, and resolving them there would reuse HitQuery's buffer.
    /// </summary>
    public sealed class PlayerTalents : MonoBehaviour
    {
        static readonly AttackSpec ThornSpec = new AttackSpec(0.6f, 3f, 0f, 0.02f, 0.05f);
        static readonly AttackSpec SparkSpec = new AttackSpec(0.5f, 1f, 0f, 0.02f, 0.05f);
        static readonly AttackSpec BurstSpec = new AttackSpec(0.4f, 4f, 0f, 0.03f, 0.1f);
        static readonly AttackSpec ShockSpec = new AttackSpec(0.6f, 9f, 0.2f, 0.05f, 0.2f);

        [SerializeField] Sprite[] flameFrames;
        [SerializeField] Material glowMaterial;
        [SerializeField] GameObject sparkVfx;
        [SerializeField] GameObject burstVfx;
        [SerializeField] GameObject ringVfx;

        readonly HashSet<Talent> owned = new HashSet<Talent>();
        readonly HashSet<Health> rollHit = new HashSet<Health>();
        readonly Queue<Vector2> bursts = new Queue<Vector2>();
        Health health;
        CombatStats stats;
        int hits;
        bool chaining, shockPending, secondWindUsed;
        float nextTrail, nextShock;

        public IReadOnlyCollection<Talent> Owned => owned;
        public bool Has(Talent t) => owned.Contains(t);

        void Awake()
        {
            health = GetComponent<Health>();
            stats = GetComponent<CombatStats>();
        }

        void Start()
        {
            GameEvents.DamageDealt += OnDealt;
            GameEvents.EnemyKilled += OnKilled;
            health.Damaged += OnHurt;
            health.Changed += OnHealthChanged;
        }

        void OnDestroy()
        {
            GameEvents.DamageDealt -= OnDealt;
            GameEvents.EnemyKilled -= OnKilled;
            if (health != null) { health.Damaged -= OnHurt; health.Changed -= OnHealthChanged; }
        }

        public void Add(Talent t)
        {
            if (!owned.Add(t)) return;
            GameEvents.RaiseToast("Talent: " + TalentCatalog.Info(t).Name);
        }

        /// <summary>A new floor: Second Wind is ready again.</summary>
        public void OnFloorStart() => secondWindUsed = false;

        public void RollStarted() => rollHit.Clear();

        /// <summary>Called every frame of a roll with the hero's position.</summary>
        public void WhileRolling(Vector2 at)
        {
            if (Has(Talent.ThornRoll)) HitQuery.StrikeCircle(stats, at, 0.8f, ThornSpec, rollHit);
            if (Has(Talent.EmberTrail) && Time.time >= nextTrail)
            {
                nextTrail = Time.time + 0.09f;
                FireTrail.Spawn(at, stats, flameFrames, glowMaterial);
            }
        }

        void OnDealt(DamageInfo info, Vector3 at)
        {
            if (chaining || info.SourceTeam != Team.Player || !Has(Talent.ChainSpark)) return;
            if (!TalentCatalog.ChainSparkFires(++hits)) return;
            chaining = true;   // spark hits raise DamageDealt too; they must not spark again
            int jumped = 0;
            foreach (var enemy in new List<Health>(TargetRegistry.All))
            {
                if (jumped >= 2) break;
                if (enemy == null || enemy.IsDead) continue;
                float d = Vector2.Distance(enemy.transform.position, at);
                if (d < 0.3f || d > 4.5f) continue;
                if (HitQuery.TryHit(stats, enemy, SparkSpec, at))
                {
                    jumped++;
                    if (sparkVfx != null) Pool.Spawn(sparkVfx, enemy.transform.position + Vector3.up * 0.5f, Quaternion.identity);
                }
            }
            chaining = false;
        }

        void OnKilled(Vector3 at, bool boss)
        {
            if (!boss && Has(Talent.DeathBurst)) bursts.Enqueue(at);
        }

        void OnHurt(DamageInfo info)
        {
            if (Has(Talent.Retaliate) && Time.time >= nextShock) shockPending = true;
        }

        void OnHealthChanged(int current, int max)
        {
            if (!Has(Talent.SecondWind) || !TalentCatalog.SecondWindDue(current, max, secondWindUsed)) return;
            secondWindUsed = true;
            health.Heal(Mathf.RoundToInt(max * 0.3f));
            AudioManager.Play(Sfx.Boon);
            GameEvents.RaiseToast("Second Wind!");
        }

        void Update()
        {
            int n = bursts.Count;   // bursts that kill queue more for next frame: chains ripple outward
            for (int i = 0; i < n; i++)
            {
                var at = bursts.Dequeue();
                HitQuery.StrikeCircle(stats, at, 1.8f, BurstSpec, null);
                if (burstVfx != null) Pool.Spawn(burstVfx, at, Quaternion.identity);
            }
            if (shockPending)
            {
                shockPending = false;
                nextShock = Time.time + 5f;
                HitQuery.StrikeCircle(stats, transform.position, 2.4f, ShockSpec, null);
                if (ringVfx != null) Pool.Spawn(ringVfx, transform.position, Quaternion.identity);
                CameraRig.Shake(0.2f);
            }
        }

#if UNITY_EDITOR
        public void EditorInit(Sprite[] flames, Material glow, GameObject spark, GameObject burst, GameObject ring)
        {
            flameFrames = flames; glowMaterial = glow; sparkVfx = spark; burstVfx = burst; ringVfx = ring;
        }
#endif
    }
}
