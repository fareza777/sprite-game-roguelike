using System.Collections.Generic;
using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>
    /// The floor's air: a few dozen faint motes drifting through the camera view (dust, spores, embers, snow...).
    /// Drawn just above the floor and below every character so they never hide the fight.
    /// </summary>
    public sealed class AmbientMotes : MonoBehaviour
    {
        const int SortingOrder = -80;   // above floor decals and hazards, below characters

        [SerializeField] Camera view;
        [SerializeField] Material glowMaterial;

        readonly List<SpriteRenderer> motes = new List<SpriteRenderer>();
        readonly List<Vector2> positions = new List<Vector2>();
        readonly List<float> phases = new List<float>();
        Material litMaterial;
        Sprite[] frames;
        AmbientStyle style;

        public void Show(Sprite[] sprites, AmbientStyle s)
        {
            frames = sprites;
            style = s;
            bool on = sprites != null && sprites.Length > 0 && view != null;
            while (motes.Count < s.Count) AddMote();
            var area = ViewRect();
            for (int i = 0; i < motes.Count; i++)
            {
                bool used = on && i < s.Count;
                motes[i].enabled = used;
                if (!used) continue;
                positions[i] = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));
                motes[i].sprite = sprites[i % sprites.Length];
                motes[i].sharedMaterial = s.Glow && glowMaterial != null ? glowMaterial : litMaterial;
            }
        }

        void AddMote()
        {
            var go = new GameObject("Mote");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = SortingOrder;
            if (litMaterial == null) litMaterial = sr.sharedMaterial;
            sr.enabled = false;
            motes.Add(sr);
            positions.Add(Vector2.zero);
            phases.Add(Random.value * 10f);
        }

        Rect ViewRect()
        {
            if (view == null) return new Rect(-8f, -12f, 16f, 24f);
            float h = view.orthographicSize * 2f, w = h * view.aspect;
            Vector2 c = view.transform.position;
            return new Rect(c.x - w / 2f - 1f, c.y - h / 2f - 1f, w + 2f, h + 2f);
        }

        void LateUpdate()
        {
            if (frames == null || frames.Length == 0) return;
            var area = ViewRect();
            float t = Time.time;
            for (int i = 0; i < style.Count && i < motes.Count; i++)
            {
                var p = AmbientStyles.Wrap(positions[i] + style.Velocity * Time.deltaTime, area);
                positions[i] = p;
                float wobble = Mathf.Sin(t * 1.3f + phases[i]) * style.Sway;
                motes[i].transform.position = new Vector3(p.x + wobble, p.y, 0f);
                float twinkle = 0.6f + 0.4f * Mathf.Sin(t * 2.1f + phases[i] * 3f);
                motes[i].color = new Color(1f, 1f, 1f, style.Alpha * twinkle);
                if (frames.Length > 1) motes[i].sprite = frames[(i + (int)(t * 2.5f + phases[i])) % frames.Length];
            }
        }

#if UNITY_EDITOR
        public void EditorInit(Camera cam, Material glow) { view = cam; glowMaterial = glow; }
#endif
    }
}
