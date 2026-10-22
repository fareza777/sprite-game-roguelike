using System.Collections;
using Oathdeep.Audio;
using Oathdeep.Combat;
using Oathdeep.Core;
using Oathdeep.Player;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// Shared Chorister fight rules on top of the boss's own AI: three phases with a roaring transition, a break gauge
    /// that staggers the boss when the player keeps up the pressure, damage multipliers (shields, exposed vents), and the
    /// boss's signature mechanics (the <see cref="BossMechanic"/> components on the same object).
    /// </summary>
    [RequireComponent(typeof(Health))]
    public sealed class BossCore : MonoBehaviour
    {
        const float IntroSeconds = 1.2f;
        const float PhaseHaltSeconds = 1.4f;
        const float StaggerDamageBonus = 1.5f;
        const float BreakCapacity = 0.14f;   // fraction of max health that breaks the guard the first time

        [SerializeField] BossFx fx;
        [SerializeField] Projectile shot;
        [SerializeField] AttackSpec shardSpec = new AttackSpec(0.8f, 4f, 0f, 0.03f, 0.08f);

        EnemyBase enemy;
        HitFlash flash;
        BossMechanic[] mechanics = new BossMechanic[0];
        BreakGauge gauge;
        float haltedUntil;
        float startAt = float.MaxValue;
        float lastBlockedToast = -99f;
        float lastFraction = -1f;
        System.Func<Vector2Int, bool> isFloor;
        bool lastStagger, started, ended;

        public BossFx Fx => fx;
        public Health Health { get; private set; }
        public CombatStats Stats { get; private set; }
        public Transform Target { get; private set; }
        public Health TargetHealth { get; private set; }
        public Rect Arena { get; private set; }
        public int Phase { get; private set; } = 1;
        public Transform HazardRoot { get; private set; }
        public SpriteRenderer Model { get; private set; }
        public bool Staggered => gauge != null && gauge.Broken;
        public bool Halted => Staggered || Time.time < haltedUntil;
        public string DisplayName => enemy != null && enemy.Data != null ? enemy.Data.DisplayName : name;

        void Awake()
        {
            Health = GetComponent<Health>();
            Stats = GetComponent<CombatStats>();
            enemy = GetComponent<EnemyBase>();
            flash = GetComponentInChildren<HitFlash>();
            var driver = GetComponentInChildren<AnimDriver>();
            Model = driver != null ? driver.Renderer : GetComponentInChildren<SpriteRenderer>();
            mechanics = GetComponents<BossMechanic>();
            foreach (var m in mechanics) m.Attach(this);
        }

        /// <summary>Called by the boss room once the boss stands in it.</summary>
        public void Begin(Transform target, Rect arena, System.Func<Vector2Int, bool> floorTest = null)
        {
            isFloor = floorTest;
            Target = target;
            TargetHealth = target != null ? target.GetComponent<Health>() : null;
            Arena = arena;
            HazardRoot = new GameObject(name + " Arena").transform;
            gauge = new BreakGauge(Health.Max * BreakCapacity);
            Health.Damaged += OnDamaged;
            Health.Blocked += OnBlocked;
            Health.Died += End;
            startAt = Time.time + IntroSeconds;
            if (fx != null && fx.rootSnare != null) PlayerStatus.RootSprite = fx.rootSnare;
            GameEvents.RaiseBossBreakChanged(0f, false);
        }

        void Update()
        {
            if (ended || Target == null || gauge == null) return;
            if (!started)
            {
                if (Time.time < startAt) return;
                started = true;
                EnterPhase(1, true);
                return;
            }
            gauge.Tick(Time.deltaTime);
            int want = BossPhases.PhaseFor(Health.Current, Health.Max);
            if (want > Phase) EnterPhase(want, false);
            foreach (var m in mechanics)
                if (m.enabled && m.ActiveIn(Phase)) m.Tick(Time.deltaTime);
            ApplyMultiplier();
            RaiseBreak();
        }

        void EnterPhase(int phase, bool quiet)
        {
            int before = Phase;
            Phase = phase;
            foreach (var m in mechanics)   // a mechanic whose phases are over cleans up (the Cantor's opening anchors)
                if (!quiet && m.ActiveIn(before) && !m.ActiveIn(phase)) m.End();
            if (!quiet)
            {
                haltedUntil = Time.time + PhaseHaltSeconds;
                StartCoroutine(Invulnerable(PhaseHaltSeconds));
                flash?.Flash(new Color(1f, 0.45f, 0.15f), 0.35f);
                CameraRig.Shake(0.7f);
                HitStop.Trigger(0.12f);
                AudioManager.Play(Sfx.BossRoar, transform.position);
                GameEvents.RaiseToast(phase >= BossPhases.Count ? $"{DisplayName} is desperate!" : $"{DisplayName} grows stronger!");
            }
            GameEvents.RaiseBossPhaseChanged(phase);
            foreach (var m in mechanics)
                if (m.enabled && m.ActiveIn(phase)) m.OnPhase(phase);
        }

        IEnumerator Invulnerable(float seconds)
        {
            Health.PushInvulnerable();
            yield return new WaitForSeconds(seconds);
            Health.PopInvulnerable();
        }

        /// <summary>True when the cell under `p` and its neighbours are open floor (not a pillar or wall).</summary>
        public bool IsOpenFloor(Vector2 p)
        {
            if (isFloor == null) return Arena.Contains(p);
            var c = new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if (!isFloor(c + new Vector2Int(dx, dy))) return false;
            return true;
        }

        /// <summary>The AI tells the core which attack it just used (Overheat counts slams).</summary>
        public void ReportMove(BossMove move)
        {
            foreach (var m in mechanics)
                if (m.enabled && m.ActiveIn(Phase)) m.OnMove(move);
        }

        void OnDamaged(DamageInfo info)
        {
            if (gauge == null || !gauge.AddDamage(info.Amount)) return;
            flash?.Flash(new Color(1f, 0.95f, 0.5f), 0.4f);
            CameraRig.Shake(0.45f);
            HitStop.Trigger(0.15f);
            AudioManager.Play(Sfx.BossSlam, transform.position, 0.7f);
            GameEvents.RaiseToast("Guard broken! Strike now!");
        }

        void OnBlocked(DamageInfo info)
        {
            flash?.Flash(new Color(0.55f, 0.6f, 0.7f), 0.12f);
            if (Time.time - lastBlockedToast < 4f) return;
            lastBlockedToast = Time.time;
            string hint = null;
            foreach (var m in mechanics)
                if (m.enabled && m.ActiveIn(Phase) && m.BlockedHint != null) { hint = m.BlockedHint; break; }
            GameEvents.RaiseToast(hint ?? $"{DisplayName} is shielded!");
        }

        void ApplyMultiplier()
        {
            float mult = Staggered ? StaggerDamageBonus : 1f;
            foreach (var m in mechanics)
                if (m.enabled && m.ActiveIn(Phase)) mult *= m.DamageMultiplier;
            Health.DamageTakenMultiplier = mult;
        }

        void RaiseBreak()
        {
            float f = gauge.Fraction;
            if (Mathf.Abs(f - lastFraction) < 0.005f && Staggered == lastStagger) return;
            lastFraction = f;
            lastStagger = Staggered;
            GameEvents.RaiseBossBreakChanged(f, Staggered);
        }

        /// <summary>A ring of shards from `from` (a shattered decoy or a broken anchor), fired with the boss's stats.</summary>
        public void Shatter(Vector2 from, int count, float speed)
        {
            if (shot == null) return;
            float offset = Random.value * 360f;
            for (int i = 0; i < count; i++)
            {
                var dir = (Vector2)(Quaternion.Euler(0, 0, offset + 360f / count * i) * Vector2.right);
                Pool.Spawn(shot, from + dir * 0.5f, Quaternion.identity).Launch(Stats, dir, speed, shardSpec);
            }
        }

        void End()
        {
            if (ended) return;
            ended = true;
            foreach (var m in mechanics) m.End();
            HazardZone.ClearAll();
            if (HazardRoot != null) Destroy(HazardRoot.gameObject);
            if (Target != null && Target.TryGetComponent<PlayerStatus>(out var status)) status.Clear();
            GameEvents.RaiseBossBreakChanged(0f, false);
        }

        void OnDestroy()
        {
            if (!ended && HazardRoot != null) Destroy(HazardRoot.gameObject);
        }

#if UNITY_EDITOR
        public void EditorInit(BossFx library, Projectile shard) { fx = library; shot = shard; }
#endif
    }
}
