using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Oathdeep.UI
{
    /// <summary>Full-screen overlay (title, pause, game over, victory) built from a title, body and buttons.</summary>
    public sealed class ScreenView : MonoBehaviour
    {
        CanvasGroup group;
        TextMeshProUGUI body;

        public static ScreenView Create(string name, RectTransform parent, UISkin skin, string title, string bodyText,
                                        (string label, Action action)[] buttons, bool showLogo = false)
        {
            var root = UIFactory.Stretch(name, parent);
            var dim = UIFactory.Image(root, skin.white, new Color(0.035f, 0.04f, 0.08f, 0.82f), false, true);
            dim.preserveAspect = false;
            var view = root.gameObject.AddComponent<ScreenView>();
            view.group = root.gameObject.AddComponent<CanvasGroup>();

            float y = 120f;
            if (showLogo && skin.logo != null)
            {
                var logo = UIFactory.Rect("Logo", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(300, 120));
                UIFactory.Image(logo, skin.logo);
                y = 60f;
            }
            else
            {
                var titleRect = UIFactory.Rect("Title", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, y + 40), new Vector2(320, 48));
                UIFactory.Text(titleRect, skin, title, 32, TextAlignmentOptions.Center, skin.accentColor);
            }

            var bodyRect = UIFactory.Rect("Body", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0, y - 10), new Vector2(300, 140));
            view.body = UIFactory.Text(bodyRect, skin, bodyText, 13, TextAlignmentOptions.Top);
            view.body.textWrappingMode = TextWrappingModes.Normal;
            view.body.lineSpacing = 12f;

            for (int i = 0; i < buttons.Length; i++)
            {
                var b = UIFactory.Rect(buttons[i].label, root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -110 - i * 52), new Vector2(200, 42));
                UIFactory.Button(b, skin, buttons[i].label, 16, buttons[i].action);
            }

            view.Hide();
            return view;
        }

        public void SetBody(string text) => body.text = text;

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
