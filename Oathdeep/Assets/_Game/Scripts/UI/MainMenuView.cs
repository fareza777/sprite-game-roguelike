using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Oathdeep.UI
{
    /// <summary>
    /// Title screen: the chasm backdrop with drifting embers, the bobbing UNDERSUN logo, DESCEND, the
    /// collection pages and a footer of settings / about / share / rate. Actions left null hide their button.
    /// </summary>
    public sealed class MainMenuView : MonoBehaviour
    {
        public sealed class Actions
        {
            public Action Descend, Heroes, Codex, Quests, Settings, About, Share, Rate;
        }

        const int EmberCount = 22;

        CanvasGroup group;
        RectTransform logo;
        readonly List<(RectTransform rt, float speed, float sway, float phase)> embers = new List<(RectTransform, float, float, float)>();
        RectTransform emberLayer;

        public static MainMenuView Create(RectTransform parent, UISkin skin, Actions actions, string version)
        {
            var root = UIFactory.Stretch("MainMenu", parent);
            var view = root.gameObject.AddComponent<MainMenuView>();
            view.group = root.gameObject.AddComponent<CanvasGroup>();

            var bg = UIFactory.Stretch("Backdrop", root);
            var bgImg = UIFactory.Image(bg, skin.menuBackground);
            bgImg.preserveAspect = false;
            var fitter = bg.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = 180f / 320f;

            view.emberLayer = UIFactory.Stretch("Embers", root);
            for (int i = 0; i < EmberCount; i++)
            {
                var e = UIFactory.Rect("Ember", view.emberLayer, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2, 2));
                var img = UIFactory.Image(e, skin.white, i % 3 == 0 ? skin.critColor : skin.accentColor);
                img.preserveAspect = false;
                view.embers.Add((e, UnityEngine.Random.Range(14f, 34f), UnityEngine.Random.Range(4f, 14f), UnityEngine.Random.value * 10f));
            }

            view.logo = UIFactory.Rect("Logo", root, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(288, 144));
            UIFactory.Image(view.logo, skin.logo);
            var tag = UIFactory.Rect("Tagline", root, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -200), new Vector2(300, 30));
            var tagText = UIFactory.Text(tag, skin, "The sun was buried.\nYou are what it sings for.", 11, TextAlignmentOptions.Center, skin.mutedColor);
            tagText.lineSpacing = 8f;

            var descend = UIFactory.Rect("Descend", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -36), new Vector2(230, 56));
            UIFactory.Button(descend, skin, "DESCEND", 22, actions.Descend);

            var middle = new List<(Sprite, string, Action)>();
            if (actions.Heroes != null) middle.Add((skin.iconHeroes, "HEROES", actions.Heroes));
            if (actions.Codex != null) middle.Add((skin.iconCodex, "CODEX", actions.Codex));
            if (actions.Quests != null) middle.Add((skin.iconQuest, "QUESTS", actions.Quests));
            Row(root, skin, middle, -118, new Vector2(70, 62));

            var footer = new List<(Sprite, string, Action)>();
            if (actions.Settings != null) footer.Add((skin.iconSettings, "SETTINGS", actions.Settings));
            if (actions.About != null) footer.Add((skin.iconInfo, "ABOUT", actions.About));
            if (actions.Share != null) footer.Add((skin.iconShare, "SHARE", actions.Share));
            if (actions.Rate != null) footer.Add((skin.iconRate, "RATE", actions.Rate));
            Row(root, skin, footer, -216, new Vector2(66, 58));

            var ver = UIFactory.Rect("Version", root, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-8, 6), new Vector2(160, 14));
            UIFactory.Text(ver, skin, version, 9, TextAlignmentOptions.BottomRight, skin.mutedColor);

            view.Hide();
            return view;
        }

        static void Row(RectTransform root, UISkin skin, List<(Sprite icon, string caption, Action action)> items, float y, Vector2 size)
        {
            float gap = 8f;
            float total = items.Count * size.x + (items.Count - 1) * gap;
            for (int i = 0; i < items.Count; i++)
            {
                float x = -total / 2 + size.x / 2 + i * (size.x + gap);
                var rt = UIFactory.Rect(items[i].caption, root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), size);
                UIWidgets.IconButton(rt, skin, items[i].icon, items[i].action, items[i].caption);
            }
        }

        void Update()
        {
            float t = Time.unscaledTime;
            if (logo != null) logo.anchoredPosition = new Vector2(0, -40 + Mathf.Sin(t * 1.3f) * 3f);
            var size = emberLayer.rect.size;
            foreach (var (rt, speed, sway, phase) in embers)
            {
                float y = Mathf.Repeat(phase * 97f + t * speed, size.y + 20f) - 10f;
                float x = Mathf.Repeat(phase * 53f, size.x) + Mathf.Sin(t * 0.8f + phase) * sway;
                rt.anchoredPosition = new Vector2(x, y);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            group.alpha = 1f;
            group.blocksRaycasts = true;
            transform.SetAsLastSibling();
        }

        public void Hide()
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
    }
}
