using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Oathdeep.UI
{
    /// <summary>Composite controls (icon buttons, sliders, toggles, labelled rows) built from the skin.</summary>
    public static class UIWidgets
    {
        public static Button IconButton(RectTransform rt, UISkin skin, Sprite icon, Action onClick, string caption = null)
        {
            var b = UIFactory.Button(rt, skin, "", 1, onClick);
            var iconRect = UIFactory.Rect("Icon", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                caption == null ? Vector2.zero : new Vector2(0, 6), new Vector2(24, 24));
            UIFactory.Image(iconRect, icon);
            if (caption != null)
            {
                var capRect = UIFactory.Rect("Caption", rt, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 4), new Vector2(0, 12));
                UIFactory.Text(capRect, skin, caption, 9, TextAlignmentOptions.Center, skin.mutedColor);
            }
            return b;
        }

        public static Slider Slider(RectTransform rt, UISkin skin, float value, Action<float> onChange)
        {
            var bg = UIFactory.Image(rt, skin.barFrame, null, true, true);
            var slider = rt.gameObject.AddComponent<Slider>();
            var fillArea = UIFactory.Stretch("FillArea", rt);
            fillArea.offsetMin = new Vector2(3, 3);
            fillArea.offsetMax = new Vector2(-3, -3);
            var fill = UIFactory.Rect("Fill", fillArea, Vector2.zero, new Vector2(0, 1), new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
            var fillImg = UIFactory.Image(fill, skin.white, skin.accentColor);
            fillImg.preserveAspect = false;
            var handleArea = UIFactory.Stretch("HandleArea", rt);
            var handle = UIFactory.Rect("Handle", handleArea, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(20, 6));
            var handleImg = UIFactory.Image(handle, skin.joystickKnob, null, false, true);
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImg;
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(v => onChange?.Invoke(v));
            bg.raycastTarget = true;
            return slider;
        }

        /// <summary>Label on the left, a button on the right that shows ON/OFF (or any two states).</summary>
        public static void ToggleRow(RectTransform row, UISkin skin, string label, Func<bool> get, Action<bool> set,
                                     string onText = "ON", string offText = "OFF")
        {
            Label(row, skin, label);
            var btnRect = UIFactory.Rect("Toggle", row, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), Vector2.zero, new Vector2(96, 30));
            TextMeshProUGUI text = null;
            UIFactory.Button(btnRect, skin, "", 13, () =>
            {
                set(!get());
                Refresh();
            });
            text = btnRect.GetComponentInChildren<TextMeshProUGUI>();
            Refresh();

            void Refresh()
            {
                bool on = get();
                text.text = on ? onText : offText;
                text.color = on ? skin.accentColor : skin.mutedColor;
            }
        }

        public static void SliderRow(RectTransform row, UISkin skin, string label, float value, Action<float> onChange)
        {
            Label(row, skin, label);
            var sRect = UIFactory.Rect("Slider", row, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), Vector2.zero, new Vector2(150, 16));
            Slider(sRect, skin, value, onChange);
        }

        public static TextMeshProUGUI Label(RectTransform row, UISkin skin, string label)
        {
            var lRect = UIFactory.Rect("Label", row, new Vector2(0, 0), new Vector2(0.55f, 1), new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
            return UIFactory.Text(lRect, skin, label, 13, TextAlignmentOptions.Left);
        }
    }
}
