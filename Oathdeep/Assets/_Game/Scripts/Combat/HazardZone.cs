using System.Collections.Generic;
using UnityEngine;

namespace Oathdeep.Combat
{
    /// <summary>
    /// A patch of hostile ground laid by a boss: water, ice, spores, quicksand, lava, roots. It flashes as a warning,
    /// then acts while the hero stands in it. The hero's movement samples every live zone through <see cref="SampleMove"/>.
    /// </summary>
    public sealed class HazardZone : MonoBehaviour
    {
        public const int SortingOrder = -85;   // above floor decals (-90), below characters (0)

        static readonly List<HazardZone> Live = new List<HazardZone>();

        HazardKind kind;
        float strength;          // Slow: speed multiplier, Ice: traction, Root: seconds held
        HazardShape shape;
        float activeAt, endAt, fadeFrom = float.MaxValue;
        SpriteRenderer sr;
        Color tint;
        Health victim;
        CombatStats owner;
        AttackSpec burn;
        float nextBurn;
        bool rootSprung;

        public HazardKind Kind => kind;
        public bool Active => Time.time >= activeAt && Time.time < endAt;
        public HazardShape Shape => shape;

        /// <summary>
        /// Lays a hazard. `warn` seconds of flashing first, then `life` seconds active. Burn zones hurt `victim` with
        /// `owner`'s stats; Root zones grab the hero once.
        /// </summary>
        public static HazardZone Spawn(Transform parent, Sprite sprite, Color tint, HazardKind kind, float strength, HazardShape shape,
            float warn, float life, Health victim = null, CombatStats owner = null, AttackSpec burn = default)
        {
            var go = new GameObject("Hazard_" + kind);
            go.transform.SetParent(parent, false);
            var z = go.AddComponent<HazardZone>();
            z.kind = kind;
            z.strength = strength;
            z.shape = shape;
            z.tint = tint;
            z.victim = victim;
            z.owner = owner;
            z.burn = burn;
            z.activeAt = Time.time + warn;
            z.endAt = z.activeAt + life;
            z.nextBurn = z.activeAt;

            z.sr = go.AddComponent<SpriteRenderer>();
            z.sr.sprite = sprite;
            z.sr.sortingOrder = SortingOrder;
            if (shape.IsBox)
            {
                go.transform.position = shape.Bounds.center;
                z.sr.drawMode = SpriteDrawMode.Tiled;
                z.sr.size = shape.Bounds.size;
            }
            else
            {
                go.transform.position = shape.Centre;
                float native = sprite != null ? sprite.bounds.size.x : 1f;
                go.transform.localScale = Vector3.one * (shape.Radius * 2f / Mathf.Max(0.01f, native));
            }
            z.Paint();
            return z;
        }

        /// <summary>Ends a zone early with a short fade (e.g. a spore pod destroyed nearby).</summary>
        public void Dismiss(float fade = 0.4f)
        {
            if (Time.time < activeAt) { endAt = Time.time; return; }   // a warning simply vanishes, it never fires
            endAt = Mathf.Min(endAt, Time.time + fade);
        }

        void OnEnable() => Live.Add(this);
        void OnDisable() => Live.Remove(this);

        void Update()
        {
            if (Time.time >= endAt) { Destroy(gameObject); return; }
            Paint();
            if (!Active || victim == null || victim.IsDead || !shape.Contains(victim.transform.position)) return;

            if (kind == HazardKind.Burn && Time.time >= nextBurn && owner != null)
            {
                nextBurn = Time.time + 0.6f;
                HitQuery.TryHit(owner, victim, burn, transform.position);
            }
            else if (kind == HazardKind.Root && !rootSprung)
            {
                rootSprung = true;
                victim.GetComponent<IRootable>()?.Root(strength);
                Dismiss(0.6f);
            }
        }

        void Paint()
        {
            float a;
            if (Time.time < activeAt)
                a = Mathf.Lerp(0.2f, 0.55f, 0.5f + 0.5f * Mathf.Sin(Time.time * 18f));   // warning flash
            else
                a = 0.85f * Mathf.Clamp01((endAt - Time.time) / 0.5f);                    // fades at the end
            sr.color = new Color(tint.r, tint.g, tint.b, tint.a * a);
        }

        /// <summary>The combined effect of every active zone under `point`.</summary>
        public static MoveModifier SampleMove(Vector2 point)
        {
            if (Live.Count == 0) return MoveModifier.None;
            var under = new List<(HazardKind, float)>();
            foreach (var z in Live)
                if (z.Active && z.shape.Contains(point)) under.Add((z.kind, z.strength));
            return MoveModifier.Combine(under);
        }

        /// <summary>Removes every hazard (the boss died or the run ended).</summary>
        public static void ClearAll()
        {
            foreach (var z in Live.ToArray()) if (z != null) Destroy(z.gameObject);
            Live.Clear();
        }
    }

    /// <summary>Something a Root hazard can hold in place (the hero).</summary>
    public interface IRootable
    {
        void Root(float seconds);
    }
}
