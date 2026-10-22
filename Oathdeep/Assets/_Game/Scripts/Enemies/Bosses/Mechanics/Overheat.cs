using Oathdeep.Audio;
using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>
    /// The Furnace Saint overheats: every third slam bursts its vents open and it takes double damage for a few
    /// seconds. From phase two each slam also cracks the floor into lines of lava.
    /// </summary>
    public sealed class Overheat : BossMechanic
    {
        const float VentSeconds = 4.5f;
        static readonly Color LavaTint = new Color(1f, 1f, 1f, 0.95f);

        int slams;
        float ventUntil;

        public override float DamageMultiplier => Time.time < ventUntil ? 2f : 1f;

        public override void OnMove(BossMove move)
        {
            if (move != BossMove.Slam) return;
            if (Core.Phase >= 2 && Fx != null) Cracks();
            if (++slams < 3) return;
            slams = 0;
            ventUntil = Time.time + VentSeconds;
            Steam();
            AudioManager.Play(Sfx.BossRoar, Core.transform.position, 0.6f);
            GameEvents.RaiseToast("Its vents burst open! Hit it now!");
        }

        void Cracks()
        {
            Vector2 c = Core.transform.position;
            const float len = 6f, w = 1f;
            Lava(new Rect(c.x + 0.8f, c.y - w / 2f, len, w));
            Lava(new Rect(c.x - 0.8f - len, c.y - w / 2f, len, w));
            Lava(new Rect(c.x - w / 2f, c.y + 0.8f, w, len));
            Lava(new Rect(c.x - w / 2f, c.y - 0.8f - len, w, len));
        }

        void Lava(Rect r) => Zone(Fx.lava, LavaTint, HazardKind.Burn, 0f, HazardShape.Box(Rect.MinMaxRect(
            Mathf.Max(r.xMin, Arena.xMin), Mathf.Max(r.yMin, Arena.yMin), Mathf.Min(r.xMax, Arena.xMax), Mathf.Min(r.yMax, Arena.yMax))), 0.5f, 4f);

        void Steam()
        {
            if (Fx == null || Fx.steam == null) return;
            for (int i = 0; i < 3; i++)
            {
                var go = new GameObject("Steam");
                go.transform.SetParent(Core.transform, false);
                go.transform.localPosition = new Vector3(-0.9f + i * 0.9f, 2.4f + (i % 2) * 0.3f, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Fx.steam;
                sr.sortingOrder = 60;
                Destroy(go, VentSeconds);
            }
        }
    }
}
