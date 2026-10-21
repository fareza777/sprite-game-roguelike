using System;
using TMPro;
using UnityEngine;

namespace Oathdeep.UI
{
    /// <summary>Modal yes/no prompt for destructive actions (reset progress, abandon run).</summary>
    public sealed class ConfirmDialog : MonoBehaviour
    {
        TextMeshProUGUI message;
        Action onYes;

        public static ConfirmDialog Create(RectTransform parent, UISkin skin)
        {
            var root = UIFactory.Stretch("Confirm", parent);
            var dim = UIFactory.Image(root, skin.white, new Color(0, 0, 0, 0.7f), false, true);
            dim.preserveAspect = false;
            var dialog = root.gameObject.AddComponent<ConfirmDialog>();
            var box = UIFactory.Rect("Box", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(290, 170));
            UIFactory.Image(box, skin.panel, null, true, true);
            var msgRect = UIFactory.Rect("Message", box, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(-28, 90));
            dialog.message = UIFactory.Text(msgRect, skin, "", 13, TextAlignmentOptions.Top);
            dialog.message.textWrappingMode = TextWrappingModes.Normal;
            var yes = UIFactory.Rect("Yes", box, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-66, 16), new Vector2(116, 40));
            UIFactory.Button(yes, skin, "YES", 15, () => { root.gameObject.SetActive(false); dialog.onYes?.Invoke(); });
            var no = UIFactory.Rect("No", box, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(66, 16), new Vector2(116, 40));
            UIFactory.Button(no, skin, "NO", 15, () => root.gameObject.SetActive(false));
            root.gameObject.SetActive(false);
            return dialog;
        }

        public void Ask(string text, Action yes)
        {
            message.text = text;
            onYes = yes;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }
    }
}
