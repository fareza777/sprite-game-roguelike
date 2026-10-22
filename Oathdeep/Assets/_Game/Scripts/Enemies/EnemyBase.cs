using System;
using Oathdeep.Combat;
using Oathdeep.Core;
using Oathdeep.Data;
using Oathdeep.Loot;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// Shared enemy plumbing: stats, target tracking, knockback, stun, hit flash, death + loot.
    /// Subclasses implement Think() as a small state machine.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(CombatStats))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [SerializeField] protected EnemyData data;
        [SerializeField] protected AnimDriver anim;
        [SerializeField] protected HitFlash flash;
        [SerializeField] protected Telegraph telegraph;
        [SerializeField] GameObject deathVfx;
        [SerializeField] float knockbackResistance = 0f;
        [SerializeField] GameObject eliteAura;

        const float EliteHealthMultiplier = 2.2f;
        const float EliteAttackMultiplier = 1.35f;
        const float EliteSizeMultiplier = 1.2f;

        protected Rigidbody2D Body { get; private set; }
        protected Health Health { get; private set; }
        protected CombatStats Stats { get; private set; }
        protected Transform Target { get; private set; }
        protected float StateTime { get; set; }
        protected bool Stunned => Time.time < stunnedUntil;

        public EnemyData Data => data;
        public event Action<EnemyBase> Died;

        Vector2 knockVelocity;
        float stunnedUntil;
        BossCore bossCore;
        bool halted;

        /// <summary>The Chorister rules on a boss (phases, break gauge); null on regular enemies.</summary>
        protected BossCore BossRules => bossCore;
        float statScale = 1f;

        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            Health = GetComponent<Health>();
            Stats = GetComponent<CombatStats>();
            if (anim == null) anim = GetComponentInChildren<AnimDriver>();
            if (flash == null) flash = GetComponentInChildren<HitFlash>();
            if (telegraph == null) telegraph = GetComponentInChildren<Telegraph>(true);
            bossCore = GetComponent<BossCore>();
        }

        public bool IsElite { get; private set; }

        /// <summary>Activates the enemy; statScale lets later floors reuse the same data. Elites are tougher and richer.</summary>
        public virtual void Spawn(Transform target, float scale = 1f, bool elite = false)
        {
            Target = target;
            statScale = scale;
            IsElite = elite;
            var s = data.Stats;
            float hp = scale * (elite ? EliteHealthMultiplier : 1f);
            float atk = scale * (elite ? EliteAttackMultiplier : 1f);
            var scaled = new StatBlock(Mathf.RoundToInt(s.MaxHealth * hp), Mathf.RoundToInt(s.Attack * atk), s.Defense, s.CritChance, s.CritMultiplier, s.MoveSpeed);
            if (elite)
            {
                transform.localScale = Vector3.one * EliteSizeMultiplier;
                if (eliteAura != null) eliteAura.SetActive(true);
            }
            Stats.Set(scaled, Team.Enemy);
            Health.Init(scaled.MaxHealth, Team.Enemy);
            Health.Damaged += OnDamaged;
            Health.Died += OnDied;
            TargetRegistry.Register(Health);
            StateTime = 0f;
            OnSpawned();
        }

        protected virtual void OnSpawned() { }
        protected abstract void Think(float dt);
        protected virtual Vector2 DesiredVelocity() => Vector2.zero;

        void Update()
        {
            if (Health.IsDead || Target == null) return;
            StateTime += Time.deltaTime;
            if (Stunned) { anim.Play("hurt"); return; }
            if (bossCore != null && bossCore.Halted)
            {
                // Staggered or roaring into a new phase: the AI waits, then picks up from a recovery.
                if (!halted) { halted = true; telegraph?.Hide(); }
                anim.Play(bossCore.Staggered ? "hurt" : "cast");
                return;
            }
            if (halted) { halted = false; OnResume(); }
            Think(Time.deltaTime);
        }

        void FixedUpdate()
        {
            if (Health.IsDead) { Body.linearVelocity = Vector2.zero; return; }
            knockVelocity = Vector2.MoveTowards(knockVelocity, Vector2.zero, 30f * Time.fixedDeltaTime);
            Vector2 move = Stunned || (bossCore != null && bossCore.Halted) ? Vector2.zero : DesiredVelocity();
            Body.linearVelocity = move + knockVelocity;
        }

        protected Vector2 ToTarget => Target == null ? Vector2.zero : (Vector2)(Target.position - transform.position);
        protected float DistanceToTarget => ToTarget.magnitude;
        protected float MoveSpeed => Stats.Stats.MoveSpeed;

        protected void Face(Vector2 dir) => anim.Face(dir.x);

        void OnDamaged(DamageInfo info)
        {
            Vector2 away = ((Vector2)transform.position - info.SourcePosition).normalized;
            knockVelocity = away * info.Knockback * (1f - knockbackResistance);
            if (info.StunSeconds > 0f) stunnedUntil = Mathf.Max(stunnedUntil, Time.time + info.StunSeconds * (1f - knockbackResistance));
            if (flash != null) flash.Flash(Color.white);
            if (Stunned) CancelAttack();
            OnHurt(info);
        }

        protected virtual void OnHurt(DamageInfo info) { }

        /// <summary>A boss coming out of a stagger or phase roar; reset to a safe state.</summary>
        protected virtual void OnResume() { }

        /// <summary>Interrupts telegraphs when stunned. Override to reset subclass state.</summary>
        protected virtual void CancelAttack()
        {
            if (telegraph != null) telegraph.Hide();
        }

        void OnDied()
        {
            Health.Damaged -= OnDamaged;
            Health.Died -= OnDied;
            TargetRegistry.Unregister(Health);
            if (telegraph != null) telegraph.Hide();
            foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
            anim.Play("death", true);
            if (deathVfx != null) Pool.Spawn(deathVfx, transform.position, Quaternion.identity);
            LootDropper.DropFor(data, transform.position, IsElite);
            GameEvents.RaiseEnemyKilled(transform.position, data.IsBoss);
            if (IsElite) GameEvents.RaiseEliteKilled();
            if (SaveSystem.Current.UnlockCodex(data.Id)) GameEvents.RaiseToast($"Codex: {data.DisplayName}");
            OnDeath();
            Died?.Invoke(this);
            Destroy(gameObject, 0.9f);
        }

        protected virtual void OnDeath() { }

        void OnDestroy()
        {
            if (Health != null) TargetRegistry.Unregister(Health);
        }
    }
}
