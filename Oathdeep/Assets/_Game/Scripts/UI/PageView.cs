using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Oathdeep.UI
{
    /// <summary>
    /// Full-screen menu page: dimmed backdrop, stone panel, header with title + back button, and a vertical
    /// scrolling content column. Settings, About, Codex, Quests and Inventory are all built on it.
    /// </summary>
    public sealed class PageView : MonoBehaviour
    {
        CanvasGroup group;
        Action onClosed;

        public RectTransform Content { get; private set; }
        public UISkin Skin { get; private set; }
        public TextMeshProUGUI Title { get; private set; }

        public static PageView Create(string name, RectTransform parent, UISkin skin, string title, Action onBack)
        {
            var root = UIFactory.Stretch(name, parent);
            var dim = UIFactory.Image(root, skin.white, new Color(0.035f, 0.04f, 0.08f, 0.9f), false, true);
            dim.preserveAspect = false;
            var page = root.gameObject.AddComponent<PageView>();
            page.Skin = skin;
            page.group = root.gameObject.AddComponent<CanvasGroup>();
            page.onClosed = onBack;

            var panel = UIFactory.Stretch("Panel", root);
            panel.offsetMin = new Vector2(10, 14);
            panel.offsetMax = new Vector2(-10, -14);
            UIFactory.Image(panel, skin.panel, null, true, true);

            var header = UIFactory.Rect("Header", panel, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(-16, 40));
            var back = UIFactory.Rect("Back", header, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(40, 36));
            UIWidgets.IconButton(back, skin, skin.iconBack, page.Close);
            var titleRect = UIFactory.Rect("Title", header, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            page.Title = UIFactory.Text(titleRect, skin, title, 20, TextAlignmentOptions.Center, skin.accentColor);

            var view = UIFactory.Stretch("Scroll", panel);
            view.offsetMin = new Vector2(10, 10);
            view.offsetMax = new Vector2(-10, -54);
            var scrollImg = UIFactory.Image(view, skin.white, new Color(0, 0, 0, 0), false, true);
            scrollImg.preserveAspect = false;
            view.gameObject.AddComponent<RectMask2D>();
            var scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            var content = UIFactory.Rect("Content", view, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(4, 4, 4, 12);
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;
            scroll.viewport = view;
            page.Content = content;

            page.Hide();
            return page;
        }

        /// <summary>Adds a fixed-height row to the content column.</summary>
        public RectTransform Row(string name, float height)
        {
            var row = UIFactory.Rect(name, Content, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var le = row.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            return row;
        }

        /// <summary>A paragraph that grows with its text.</summary>
        public TextMeshProUGUI Paragraph(string text, float size = 12, Color? color = null, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
        {
            var row = UIFactory.Rect("Text", Content, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var t = UIFactory.Text(row, Skin, text, size, align, color);
            t.textWrappingMode = TextWrappingModes.Normal;
            t.lineSpacing = 10f;
            var fitter = row.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return t;
        }

        public RectTransform Heading(string text)
        {
            var row = Row("Heading", 24);
            UIFactory.Text(row, Skin, text, 14, TextAlignmentOptions.BottomLeft, Skin.accentColor);
            return row;
        }

        public void Clear()
        {
            for (int i = Content.childCount - 1; i >= 0; i--) Destroy(Content.GetChild(i).gameObject);
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

        public void Close()
        {
            Hide();
            onClosed?.Invoke();
        }
    }
}
