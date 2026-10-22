using System.Collections.Generic;
using Oathdeep.Audio;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// Reflections of the boss stand in the arena and the boss trades places with them. Striking a reflection shatters
    /// it into shards. The real one casts a shadow.
    /// </summary>
    public sealed class MirrorDecoys : BossMechanic
    {
        [SerializeField] int count = 2;
        [SerializeField] Color tint = new Color(0.85f, 0.92f, 1f, 0.92f);

        readonly List<BossDecoy> decoys = new List<BossDecoy>();
        float swapTimer = 5f;
        float lastHint = -99f;

        public override void OnPhase(int phase)
        {
            if (decoys.Count > 0 || Core.Model == null) return;
            foreach (var at in RingPoints(count, 7.5f, 0f))
            {
                var d = BossDecoy.Spawn(Core.HazardRoot, Core.Model, at, 99999, new Vector2(1.8f, 2.8f), tint);
                d.Shattered += OnShattered;
                decoys.Add(d);
            }
            GameEvents.RaiseToast("Reflections! The real one casts a shadow.");
        }

        public override void Tick(float dt)
        {
            if (!Every(ref swapTimer, Core.Phase >= 3 ? 4.5f : 6.5f, dt) || Core.Halted) return;
            var standing = decoys.FindAll(d => d != null && d.Visible);
            if (standing.Count == 0) return;
            var decoy = standing[Rng.Next(standing.Count)];
            Vector2 was = Core.transform.position;
            Vector2 to = decoy.transform.position;
            if (Core.TryGetComponent<Rigidbody2D>(out var body)) body.position = to;
            Core.transform.position = to;
            decoy.transform.position = was;
            AudioManager.Play(Sfx.Spawn, to, 0.7f);
        }

        void OnShattered(BossDecoy d)
        {
            Core.Shatter(d.transform.position, 8, 6f);
            if (Time.time - lastHint < 6f) return;
            lastHint = Time.time;
            GameEvents.RaiseToast("Only a reflection! Look for the shadow.");
        }

        public override void End()
        {
            foreach (var d in decoys) if (d != null) Destroy(d.gameObject);
            decoys.Clear();
        }
    }
}
