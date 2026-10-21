using System.Collections.Generic;
using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Player
{
    /// <summary>Morrow's Requiem: wisps that circle the singer and bite whatever they pass through.
    /// Re-casting replaces the current choir instead of stacking it.</summary>
    public sealed class SoulWisps : MonoBehaviour
    {
        const float OrbitSpeed = 3.2f;     // radians per second
        const float BiteRadius = 0.45f;
        const float BiteInterval = 0.4f;
        const float Height = 0.45f;        // orbit around the chest, not the feet

        readonly List<GameObject> wisps = new List<GameObject>();
        readonly HashSet<Health> bitten = new HashSet<Health>();
        CombatStats owner;
        AttackSpec spec;
        float radius;
        float endAt;
        float nextBite;
        float angle;

        public static void Summon(GameObject host, CombatStats owner, GameObject wispPrefab, int count, float radius, float lifetime, AttackSpec spec)
        {
            var choir = host.GetComponent<SoulWisps>();
            if (choir == null) choir = host.AddComponent<SoulWisps>();
            choir.Begin(owner, wispPrefab, count, radius, lifetime, spec);
        }

        void Begin(CombatStats shooter, GameObject wispPrefab, int count, float orbit, float lifetime, AttackSpec attack)
        {
            Release();
            owner = shooter;
            spec = attack;
            radius = orbit;
            endAt = Time.time + lifetime;
            nextBite = Time.time;
            enabled = true;
            if (wispPrefab == null) return;
            for (int i = 0; i < count; i++) wisps.Add(Pool.Spawn(wispPrefab, transform.position, Quaternion.identity));
        }

        void Update()
        {
            if (Time.time >= endAt || owner == null) { Release(); enabled = false; return; }

            angle += OrbitSpeed * Time.deltaTime;
            Vector2 centre = (Vector2)transform.position + Vector2.up * Height;
            bool bite = Time.time >= nextBite;
            if (bite) { nextBite = Time.time + BiteInterval; bitten.Clear(); }

            for (int i = 0; i < wisps.Count; i++)
            {
                float a = angle + i * Mathf.PI * 2f / wisps.Count;
                Vector2 at = centre + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.7f) * radius;
                wisps[i].transform.position = at;
                if (bite) HitQuery.StrikeCircle(owner, at, BiteRadius, spec, bitten);
            }
        }

        void OnDisable() => Release();

        void Release()
        {
            foreach (var w in wisps) if (w != null) Pool.Despawn(w);
            wisps.Clear();
        }
    }
}
