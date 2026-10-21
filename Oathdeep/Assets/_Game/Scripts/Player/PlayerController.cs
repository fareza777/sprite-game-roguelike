using System.Collections.Generic;
using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;
using Oathdeep.Audio;

namespace Oathdeep.Player
{
    /// <summary>Knight movement + combat state machine: move, 3-hit combo, dodge roll, shield bash, hurt, death.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(CombatStats))]
    public sealed class PlayerController : MonoBehaviour
    {
        enum State { Move, Attack, Dodge, Bash, Hurt, Dead }

        [SerializeField] PlayerInputReader input;
        [SerializeField] AnimDriver anim;
        [SerializeField] HitFlash flash;
        [SerializeField] GameObject slashVfx;
        [SerializeField] GameObject dodgeDustVfx;
        [SerializeField] GameObject bashVfx;
        [SerializeField] float acceleration = 60f;

        Rigidbody2D body;
        Health health;
        CombatStats stats;
        State state;
        float stateTime;
        int comboIndex;
        float comboResetAt;
        bool chainQueued;
        Vector2 aim = Vector2.down;
        Vector2 dashDir;
        float dodgeReadyAt;
        float skillReadyAt;
        float dodgeCooldown = 0.45f;
        float skillCooldown = 5f;
        readonly HashSet<Health> hitThisSwing = new HashSet<Health>();
        Vector2 knockVelocity;

        public Vector2 Aim => aim;
        public bool IsDead => state == State.Dead;
        public float SkillCooldownFraction => skillCooldown <= 0f ? 0f : Mathf.Clamp01((skillReadyAt - Time.time) / skillCooldown);
        public float DodgeCooldownFraction => dodgeCooldown <= 0f ? 0f : Mathf.Clamp01((dodgeReadyAt - Time.time) / dodgeCooldown);

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            stats = GetComponent<CombatStats>();
            if (input == null) input = GetComponent<PlayerInputReader>();
            if (anim == null) anim = GetComponentInChildren<AnimDriver>();
            if (flash == null) flash = GetComponentInChildren<HitFlash>();
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

        public void SetCooldowns(float dodge, float skill)
        {
            dodgeCooldown = dodge;
            skillCooldown = skill;
        }

        void Update()
        {
            if (state == State.Dead || Time.timeScale <= 0f) return;
            stateTime += Time.deltaTime;

            Vector2 move = input.Move;
            if (move.sqrMagnitude > 0.01f && (state == State.Move)) aim = move.normalized;

            switch (state)
            {
                case State.Move: UpdateMove(move); break;
                case State.Attack: UpdateAttack(); break;
                case State.Dodge: if (stateTime >= KnightMoves.DodgeDuration) EndDodge(); break;
                case State.Bash: UpdateBash(); break;
                case State.Hurt: if (stateTime >= KnightMoves.HurtStagger) Enter(State.Move); break;
            }
        }

        void FixedUpdate()
        {
            if (state == State.Dead) { body.linearVelocity = Vector2.zero; return; }

            Vector2 target = Vector2.zero;
            switch (state)
            {
                case State.Move:
                    target = input.Move * stats.Stats.MoveSpeed;
                    break;
                case State.Attack:
                    var mv = KnightMoves.Combo[comboIndex];
                    target = stateTime < mv.ActiveStart + 0.02f ? aim * mv.LungeSpeed : Vector2.zero;
                    break;
                case State.Dodge:
                    float t = stateTime / KnightMoves.DodgeDuration;
                    target = dashDir * KnightMoves.DodgeSpeed * Mathf.Lerp(1.15f, 0.35f, t);
                    break;
                case State.Bash:
                    target = dashDir * KnightMoves.BashSpeed * (stateTime < KnightMoves.BashDuration * 0.6f ? 1f : 0.2f);
                    break;
            }

            knockVelocity = Vector2.MoveTowards(knockVelocity, Vector2.zero, 40f * Time.fixedDeltaTime);
            Vector2 desired = target + knockVelocity;
            bool instant = state == State.Dodge || state == State.Bash;
            body.linearVelocity = instant ? desired : Vector2.MoveTowards(body.linearVelocity, desired, acceleration * Time.fixedDeltaTime);
        }

        void UpdateMove(Vector2 move)
        {
            anim.Face(aim.x);
            anim.Play(move.sqrMagnitude > 0.01f ? "run" : "idle");

            if (input.Consume(PlayerAction.Dodge) && Time.time >= dodgeReadyAt) { StartDodge(); return; }
            if (input.Consume(PlayerAction.Skill) && Time.time >= skillReadyAt) { StartBash(); return; }
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
            hitThisSwing.Clear();
            AutoAim();
            Enter(State.Attack);
            anim.Face(aim.x);
            anim.Play(KnightMoves.Combo[index].Anim, true);
            if (slashVfx != null)
            {
                var angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
                var vfx = Pool.Spawn(slashVfx, (Vector2)transform.position + aim * 0.6f, Quaternion.Euler(0, 0, angle));
                vfx.transform.localScale = new Vector3(1f, index == 1 ? -1f : 1f, 1f) * (index == 2 ? 1.25f : 1f);
            }
            AudioManager.Play(index == 2 ? Sfx.SwingHeavy : Sfx.Swing);
        }

        void UpdateAttack()
        {
            var mv = KnightMoves.Combo[comboIndex];

            if (stateTime >= mv.ActiveStart && stateTime <= mv.ActiveEnd)
            {
                Vector2 center = (Vector2)transform.position + aim * mv.Reach;
                float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
                HitQuery.StrikeBox(stats, center, new Vector2(mv.Reach * 1.3f, mv.Width), angle, mv.Spec, hitThisSwing);
            }

            if (stateTime >= mv.Duration * KnightMoves.ChainFromFraction && input.Consume(PlayerAction.Attack)) chainQueued = true;

            // Recovery can be cancelled into dodge or skill for responsiveness.
            if (stateTime > mv.ActiveEnd)
            {
                if (input.IsBuffered(PlayerAction.Dodge) && Time.time >= dodgeReadyAt) { input.Consume(PlayerAction.Dodge); StartDodge(); return; }
                if (input.IsBuffered(PlayerAction.Skill) && Time.time >= skillReadyAt) { input.Consume(PlayerAction.Skill); StartBash(); return; }
            }

            if (stateTime >= mv.Duration)
            {
                comboResetAt = Time.time + KnightMoves.ComboResetSeconds;
                if (chainQueued && comboIndex < KnightMoves.Combo.Length - 1)
                {
                    if (input.Move.sqrMagnitude > 0.01f) aim = input.Move.normalized;
                    StartAttack(comboIndex + 1);
                }
                else
                {
                    // A tap shortly after recovery continues the combo; after the finisher it wraps to hit 1.
                    comboIndex = (comboIndex + 1) % KnightMoves.Combo.Length;
                    Enter(State.Move);
                }
            }
        }

        void StartDodge()
        {
            dashDir = input.Move.sqrMagnitude > 0.01f ? input.Move.normalized : aim;
            aim = dashDir;
            dodgeReadyAt = Time.time + dodgeCooldown + KnightMoves.DodgeDuration;
            health.PushInvulnerable();
            Enter(State.Dodge);
            anim.Face(dashDir.x);
            anim.Play("dodge", true);
            if (dodgeDustVfx != null) Pool.Spawn(dodgeDustVfx, transform.position, Quaternion.identity);
            AudioManager.Play(Sfx.Dodge);
            Haptics.Pulse(12);
        }

        void EndDodge()
        {
            health.PopInvulnerable();
            Enter(State.Move);
        }

        void StartBash()
        {
            AutoAim();
            dashDir = aim;
            skillReadyAt = Time.time + skillCooldown;
            hitThisSwing.Clear();
            health.PushInvulnerable();
            Enter(State.Bash);
            anim.Face(aim.x);
            anim.Play("bash", true);
            if (bashVfx != null) Pool.Spawn(bashVfx, transform.position, Quaternion.Euler(0, 0, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg));
            AudioManager.Play(Sfx.Bash);
            Haptics.Pulse(25);
        }

        void UpdateBash()
        {
            if (stateTime <= KnightMoves.BashDuration * 0.7f)
            {
                int hits = HitQuery.StrikeCircle(stats, (Vector2)transform.position + dashDir * 0.55f, KnightMoves.BashRadius, KnightMoves.Bash, hitThisSwing);
                if (hits > 0) Haptics.Pulse(40);
            }
            if (stateTime >= KnightMoves.BashDuration)
            {
                health.PopInvulnerable();
                Enter(State.Move);
            }
        }

        void AutoAim()
        {
            if (!GameSettings.AutoAim) return;
            var target = TargetRegistry.Nearest(transform.position, KnightMoves.AutoAimRange);
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
            state = State.Move;
            stateTime = 0f;
            comboIndex = 0;
            knockVelocity = Vector2.zero;
            dodgeReadyAt = skillReadyAt = 0f;
        }
    }
}
