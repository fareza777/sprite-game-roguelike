using System.Collections.Generic;
using Oathdeep.Audio;
using Oathdeep.Combat;
using Oathdeep.Core;
using Oathdeep.Data;
using UnityEngine;

namespace Oathdeep.Player
{
    /// <summary>Hero movement + combat state machine: move, 3-hit combo (melee or ranged), dodge roll, hero skill, hurt, death.
    /// The hero's feel comes from its <see cref="HeroMoves"/>; skills live in PlayerController.Skills.cs.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(CombatStats))]
    public sealed partial class PlayerController : MonoBehaviour
    {
        enum State { Move, Attack, Dodge, Skill, Hurt, Dead }

        [SerializeField] HeroId heroId;
        [SerializeField] PlayerInputReader input;
        [SerializeField] AnimDriver anim;
        [SerializeField] HitFlash flash;
        [SerializeField] GameObject slashVfx;
        [SerializeField] GameObject dodgeDustVfx;
        [SerializeField] float acceleration = 60f;

        Rigidbody2D body;
        Health health;
        CombatStats stats;
        PlayerStatus status;
        float nextAfterImage;
        static readonly Color AfterImageTint = new Color(0.65f, 0.85f, 1f, 0.55f);
        HeroMoves moves;
        State state;
        float stateTime;
        int comboIndex;
        float comboResetAt;
        bool chainQueued;
        bool shotFired;
        Vector2 aim = Vector2.down;
        Vector2 dashDir;
        float dodgeReadyAt;
        float skillReadyAt;
        float dodgeCooldown = 0.45f;
        float skillCooldown = 5f;
        readonly HashSet<Health> hitThisSwing = new HashSet<Health>();
        Vector2 knockVelocity;

        public HeroId Hero => heroId;
        public Vector2 Aim => aim;
        public bool IsDead => state == State.Dead;
        /// <summary>Raised when an attack, dodge or skill actually starts (used by onboarding and quests).</summary>
        public event System.Action<PlayerAction> Performed;
        public float SkillCooldownFraction => activeSkillCooldown <= 0f ? 0f : Mathf.Clamp01((skillReadyAt - Time.time) / activeSkillCooldown);
        public float DodgeCooldownFraction => dodgeCooldown <= 0f ? 0f : Mathf.Clamp01((dodgeReadyAt - Time.time) / dodgeCooldown);

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            stats = GetComponent<CombatStats>();
            moves = HeroMoves.For(heroId);
            if (input == null) input = GetComponent<PlayerInputReader>();
            if (anim == null) anim = GetComponentInChildren<AnimDriver>();
            if (flash == null) flash = GetComponentInChildren<HitFlash>();
            status = GetComponent<PlayerStatus>();
            if (status == null) status = gameObject.AddComponent<PlayerStatus>();
        }

        void OnEnable()
        {
            health.Damaged += OnDamaged;
            health.Died += OnDied;
        }

        void OnDisable()
        {
            health.Damaged -= OnDamaged;
            health.Died -= OnDied;
        }

        /// <summary>Applies the hero's cooldowns and move set. Called once when the run starts.</summary>
        public void Configure(HeroData hero)
        {
            heroId = hero.Id;
            moves = hero.Moves;
            dodgeCooldown = hero.DodgeCooldown;
            skillCooldown = hero.SkillCooldown;
        }

        void Update()
        {
            if (state == State.Dead || Time.timeScale <= 0f) return;
            stateTime += Time.deltaTime;

            Vector2 move = input.Move;
            if (move.sqrMagnitude > 0.01f && (state == State.Move)) aim = move.normalized;
            // Rooted: roll presses tear the hero free instead of rolling.
            if (status.Rooted && input.Consume(PlayerAction.Dodge)) status.Struggle();

            switch (state)
            {
                case State.Move: UpdateMove(move); break;
                case State.Attack: UpdateAttack(); break;
                case State.Dodge:
                    if (Time.time >= nextAfterImage)
                    {
                        nextAfterImage = Time.time + moves.DodgeDuration / 3f;   // three ghosts trail each roll
                        AfterImage.Spawn(anim.Renderer, AfterImageTint);
                    }
                    if (stateTime >= moves.DodgeDuration) EndDodge();
                    break;
                case State.Skill: UpdateSkill(); break;
                case State.Hurt: if (stateTime >= HeroMoves.HurtStagger) Enter(State.Move); break;
            }
        }

        void FixedUpdate()
        {
            if (state == State.Dead) { body.linearVelocity = Vector2.zero; return; }

            Vector2 target = Vector2.zero;
            var ground = status.Ground;   // water and quicksand slow, ice slides
            switch (state)
            {
                case State.Move:
                    target = status.Rooted ? Vector2.zero : input.Move * stats.Stats.MoveSpeed * ground.SpeedMultiplier;
                    break;
                case State.Attack:
                    var mv = moves.Combo[comboIndex];
                    target = stateTime < mv.ActiveStart + 0.02f ? aim * mv.LungeSpeed : Vector2.zero;
                    break;
                case State.Dodge:
                    float t = stateTime / moves.DodgeDuration;
                    target = dashDir * moves.DodgeSpeed * Mathf.Lerp(1.15f, 0.35f, t);
                    break;
                case State.Skill:
                    target = SkillVelocity();
                    break;
            }

            knockVelocity = Vector2.MoveTowards(knockVelocity, Vector2.zero, 40f * Time.fixedDeltaTime);
            Vector2 desired = target + knockVelocity;
            bool instant = state == State.Dodge || state == State.Skill;
            body.linearVelocity = instant ? desired : Vector2.MoveTowards(body.linearVelocity, desired, acceleration * ground.Traction * Time.fixedDeltaTime);
        }

        void UpdateMove(Vector2 move)
        {
            anim.Face(aim.x);
            anim.Play(move.sqrMagnitude > 0.01f ? "run" : "idle");

            if (input.Consume(PlayerAction.Dodge) && Time.time >= dodgeReadyAt) { StartDodge(); return; }
            if (input.Consume(PlayerAction.Skill) && Time.time >= skillReadyAt) { StartSkill(); return; }
            if (input.Consume(PlayerAction.Attack))
            {
                if (Time.time > comboResetAt) comboIndex = 0;
                StartAttack(comboIndex);
            }
        }

        void StartAttack(int index)
        {
            comboIndex = index;
            chainQueued = false;
            shotFired = false;
            hitThisSwing.Clear();
            AutoAim();
            Enter(State.Attack);
            anim.Face(aim.x);
            var mv = moves.Combo[index];
            anim.Play(mv.Anim, true);
            Performed?.Invoke(PlayerAction.Attack);
            if (mv.IsRanged) return;   // ranged hits make their sound when they fire

            if (slashVfx != null && moves.SlashScale > 0f)
            {
                var angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
                var vfx = Pool.Spawn(slashVfx, (Vector2)transform.position + aim * 0.6f, Quaternion.Euler(0, 0, angle));
                vfx.transform.localScale = new Vector3(1f, index == 1 ? -1f : 1f, 1f) * (index == 2 ? 1.25f : 1f) * moves.SlashScale;
            }
            AudioManager.Play(index == 2 ? Sfx.SwingHeavy : Sfx.Swing);
        }

        void UpdateAttack()
        {
            var mv = moves.Combo[comboIndex];

            if (mv.IsRanged)
            {
                if (!shotFired && stateTime >= mv.ActiveStart)
                {
                    shotFired = true;
                    AutoAim();
                    anim.Face(aim.x);
                    FireVolley(mv.Shot, mv.ShotCount, mv.SpreadDegrees, mv.ShotSpeed, mv.Homing, mv.Spec);
                }
            }
            else if (stateTime >= mv.ActiveStart && stateTime <= mv.ActiveEnd)
            {
                Vector2 center = (Vector2)transform.position + aim * mv.Reach;
                float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
                HitQuery.StrikeBox(stats, center, new Vector2(mv.Reach * 1.3f, mv.Width), angle, mv.Spec, hitThisSwing);
            }

            if (stateTime >= mv.Duration * HeroMoves.ChainFromFraction && input.Consume(PlayerAction.Attack)) chainQueued = true;

            // Recovery can be cancelled into dodge or skill for responsiveness.
            if (stateTime > mv.ActiveEnd)
            {
                if (input.IsBuffered(PlayerAction.Dodge) && Time.time >= dodgeReadyAt) { input.Consume(PlayerAction.Dodge); StartDodge(); return; }
                if (input.IsBuffered(PlayerAction.Skill) && Time.time >= skillReadyAt) { input.Consume(PlayerAction.Skill); StartSkill(); return; }
            }

            if (stateTime >= mv.Duration)
            {
                comboResetAt = Time.time + HeroMoves.ComboResetSeconds;
                if (chainQueued && comboIndex < moves.Combo.Length - 1)
                {
                    if (input.Move.sqrMagnitude > 0.01f) aim = input.Move.normalized;
                    StartAttack(comboIndex + 1);
                }
                else
                {
                    // A tap shortly after recovery continues the combo; after the finisher it wraps to hit 1.
                    comboIndex = (comboIndex + 1) % moves.Combo.Length;
                    Enter(State.Move);
                }
            }
        }

        /// <summary>Fires <paramref name="count"/> shots fanned evenly across <paramref name="spread"/> degrees around the aim.</summary>
        void FireVolley(ShotKind kind, int count, float spread, float speed, float homing, AttackSpec spec)
        {
            var prefab = ShotPrefab(kind);
            if (prefab == null) return;
            Vector2 muzzle = (Vector2)transform.position + Vector2.up * 0.45f + aim * 0.45f;
            for (int i = 0; i < count; i++)
            {
                float offset = count == 1 ? 0f : Mathf.Lerp(-spread * 0.5f, spread * 0.5f, i / (float)(count - 1));
                Vector2 dir = Quaternion.Euler(0, 0, offset) * aim;
                Pool.Spawn(prefab, muzzle, Quaternion.identity).Launch(stats, dir, speed, spec, homing);
            }
            AudioManager.Play(kind == ShotKind.Orb ? Sfx.SoulCast : kind == ShotKind.Flare ? Sfx.Flare : Sfx.Crossbow);
        }

        void StartDodge()
        {
            dashDir = input.Move.sqrMagnitude > 0.01f ? input.Move.normalized : aim;
            aim = dashDir;
            dodgeReadyAt = Time.time + dodgeCooldown + moves.DodgeDuration;
            health.PushInvulnerable();
            Enter(State.Dodge);
            anim.Face(dashDir.x);
            anim.Play("dodge", true);
            Performed?.Invoke(PlayerAction.Dodge);
            if (dodgeDustVfx != null) Pool.Spawn(dodgeDustVfx, transform.position, Quaternion.identity);
            AudioManager.Play(Sfx.Dodge);
            Haptics.Pulse(12);
        }

        void EndDodge()
        {
            health.PopInvulnerable();
            Enter(State.Move);
        }

        void AutoAim()
        {
            if (!GameSettings.AutoAim) return;
            var target = TargetRegistry.Nearest(transform.position, moves.AutoAimRange);
            if (target != null) aim = ((Vector2)(target.transform.position - transform.position)).normalized;
        }

        void OnDamaged(DamageInfo info)
        {
            Vector2 away = ((Vector2)transform.position - info.SourcePosition).normalized;
            knockVelocity = away * info.Knockback;
            if (flash != null) flash.Flash(new Color(1f, 0.25f, 0.25f), 0.1f);
            CameraRig.Shake(0.3f);
            Haptics.Pulse(info.IsCrit ? 70 : 45);
            if (state == State.Move || state == State.Attack)
            {
                Enter(State.Hurt);
                anim.Play("hurt", true);
            }
        }

        void OnDied()
        {
            if (state == State.Skill) EndSkillEffects();
            Enter(State.Dead);
            anim.Play("death", true);
            body.linearVelocity = Vector2.zero;
            Haptics.Pulse(120);
            GameEvents.RaisePlayerDied();
        }

        void Enter(State s)
        {
            state = s;
            stateTime = 0f;
        }

        /// <summary>Called when a run restarts or the player enters a new floor.</summary>
        public void ResetState()
        {
            if (state == State.Skill) EndSkillEffects();
            state = State.Move;
            stateTime = 0f;
            comboIndex = 0;
            knockVelocity = Vector2.zero;
            dodgeReadyAt = skillReadyAt = 0f;
            status.Clear();
        }
    }
}
