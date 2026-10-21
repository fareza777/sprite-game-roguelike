using System.Collections.Generic;
using Oathdeep.Combat;
using Oathdeep.Core;
using TMPro;
using UnityEngine;

namespace Oathdeep.UI
{
    /// <summary>Pooled world-space numbers that pop, rise and fade. Crits are bigger and gold.</summary>
    public sealed class DamageNumbers : MonoBehaviour
    {
        const float Lifetime = 0.7f;
        const int PoolSize = 40;

        sealed class Entry
        {
            public TextMeshPro Text;
            public float Born;
            public Vector3 Origin;
            public Vector3 Drift;
            public float Scale;
        }

        UISkin skin;
        readonly List<Entry> active = new List<Entry>();
        readonly Stack<Entry> free = new Stack<Entry>();

        public void Init(UISkin uiSkin)
        {
            skin = uiSkin;
            for (int i = 0; i < PoolSize; i++) free.Push(Create());
            GameEvents.DamageDealt += Show;
        }

        void OnDestroy() => GameEvents.DamageDealt -= Show;

        Entry Create()
        {
            var go = new GameObject("DamageNumber");
            go.transform.SetParent(transform, false);
            var t = go.AddComponent<TextMeshPro>();
            if (skin.font != null) t.font = skin.font;
            t.alignment = TextAlignmentOptions.Center;
            t.fontSize = 4f;
            t.sortingOrder = 500;
            t.outlineWidth = 0.25f;
            t.outlineColor = new Color32(0x09, 0x0A, 0x14, 0xFF);
            go.SetActive(false);
            return new Entry { Text = t };
        }

        void Show(DamageInfo info, Vector3 at)
        {
            var e = free.Count > 0 ? free.Pop() : (active.Count > 0 ? Recycle() : Create());
            e.Text.gameObject.SetActive(true);
            e.Text.text = info.IsCrit ? $"{info.Amount}!" : info.Amount.ToString();
            e.Text.color = info.SourceTeam == Team.Player
                ? (info.IsCrit ? skin.critColor : skin.textColor)
                : skin.dangerColor;
            e.Scale = info.IsCrit ? 1.5f : 1f;
            e.Born = Time.time;
            e.Origin = at + new Vector3(Random.Range(-0.2f, 0.2f), 0.6f, 0f);
            e.Drift = new Vector3(Random.Range(-0.4f, 0.4f), 1.2f, 0f);
            active.Add(e);
        }

        Entry Recycle()
        {
            var oldest = active[0];
            active.RemoveAt(0);
            return oldest;
        }

        void LateUpdate()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var e = active[i];
                float t = (Time.time - e.Born) / Lifetime;
                if (t >= 1f)
                {
                    e.Text.gameObject.SetActive(false);
                    active.RemoveAt(i);
                    free.Push(e);
                    continue;
                }
                // Pop: overshoot scale in the first 15%, then ease out upward.
                float pop = t < 0.15f ? Mathf.Lerp(1.6f, 1f, t / 0.15f) : 1f;
                e.Text.transform.position = e.Origin + e.Drift * Mathf.Sqrt(t);
                e.Text.transform.localScale = Vector3.one * e.Scale * pop;
                var c = e.Text.color;
                c.a = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
                e.Text.color = c;
            }
        }
    }
}
