using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Oathdeep.UI
{
    /// <summary>Small helpers so screens can be laid out in code with consistent pixel styling.</summary>
    public static class UIFactory
    {
        public static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Stretch(string name, Transform parent)
        {
            var rt = Rect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            return rt;
        }

        public static Image Image(RectTransform rt, Sprite sprite, Color? color = null, bool sliced = false, bool raycast = false)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color ?? Color.white;
            img.type = sliced ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            img.raycastTarget = raycast;
            img.preserveAspect = !sliced && sprite != null;
            return img;
        }

        public static TextMeshProUGUI Text(RectTransform rt, UISkin skin, string text, float size, TextAlignmentOptions align, Color? color = null)
        {
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (skin.font != null) t.font = skin.font;
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = color ?? skin.textColor;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Overflow;
            return t;
        }

        public static Button Button(RectTransform rt, UISkin skin, string label, float fontSize, System.Action onClick)
        {
            var img = Image(rt, skin.button, null, true, true);
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            if (skin.buttonPressed != null)
            {
                b.transition = Selectable.Transition.SpriteSwap;
                b.spriteState = new SpriteState { pressedSprite = skin.buttonPressed, highlightedSprite = skin.button, selectedSprite = skin.button };
            }
            b.onClick.AddListener(() => { Core.Haptics.Pulse(8); Audio.AudioManager.Play(Audio.Sfx.UiClick); onClick?.Invoke(); });
            var labelRect = Stretch("Label", rt);
            labelRect.offsetMin = new Vector2(4, 2);
            labelRect.offsetMax = new Vector2(-4, -2);
            Text(labelRect, skin, label, fontSize, TextAlignmentOptions.Center);
            return b;
        }
    }
}
