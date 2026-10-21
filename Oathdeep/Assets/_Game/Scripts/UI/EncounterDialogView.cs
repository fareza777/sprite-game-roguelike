using System;
using System.Collections.Generic;
using Oathdeep.Dungeon;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Oathdeep.UI
{
    /// <summary>
    /// Bottom-sheet conversation box: portrait, speaker, typewriter text and stacked answer buttons sized for thumbs.
    /// Tapping the text finishes the typewriter. Picking an answer closes the box first, then runs the answer.
    /// </summary>
    public sealed class EncounterDialogView : MonoBehaviour, IPointerClickHandler
    {
        const float CharsPerSecond = 60f;

        UISkin skin;
        CanvasGroup group;
        UnityEngine.UI.Image portrait;
        TextMeshProUGUI speaker, body;
        RectTransform choiceRoot;
        readonly List<GameObject> buttons = new List<GameObject>();
        Action onClosed;
        float revealStart;
        bool revealing;

        public bool IsOpen => gameObject.activeSelf;

        public static EncounterDialogView Create(RectTransform parent, UISkin skin)
        {
            var root = UIFactory.Stretch("EncounterDialog", parent);
            var view = root.gameObject.AddComponent<EncounterDialogView>();
            view.skin = skin;
            view.group = root.gameObject.AddComponent<CanvasGroup>();
            var dim = UIFactory.Image(root, skin.white, new Color(0.02f, 0.03f, 0.06f, 0.55f), false, true);
            dim.preserveAspect = false;

            var panel = UIFactory.Rect("Panel", root, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 12), new Vector2(-20, 300));
            UIFactory.Image(panel, skin.panel, null, true, true);

            var art = UIFactory.Rect("Portrait", panel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -10), new Vector2(72, 72));
            UIFactory.Image(art, skin.panel, new Color(1, 1, 1, 0.6f), true);
            var artInner = UIFactory.Stretch("Art", art);
            artInner.offsetMin = new Vector2(4, 4); artInner.offsetMax = new Vector2(-4, -4);
            view.portrait = UIFactory.Image(artInner, null);

            var name = UIFactory.Rect("Speaker", panel, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(92, -12), new Vector2(-102, 20));
            view.speaker = UIFactory.Text(name, skin, "", 15, TextAlignmentOptions.TopLeft, skin.accentColor);

            var text = UIFactory.Rect("Body", panel, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(92, -36), new Vector2(-104, 110));
            view.body = UIFactory.Text(text, skin, "", 11, TextAlignmentOptions.TopLeft);
            view.body.textWrappingMode = TextWrappingModes.Normal;
            view.body.lineSpacing = 8f;

            view.choiceRoot = UIFactory.Rect("Choices", panel, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(-20, 150));
            root.gameObject.SetActive(false);
            return view;
        }

        public void Show(EncounterDialog dialog, Action closed)
        {
            onClosed = closed;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            group.alpha = 1f;
            portrait.sprite = dialog.Portrait;
            portrait.enabled = dialog.Portrait != null;
            speaker.text = dialog.Speaker;
            body.text = dialog.Text;
            body.maxVisibleCharacters = 0;
            revealStart = Time.unscaledTime;
            revealing = true;

            foreach (var b in buttons) Destroy(b);
            buttons.Clear();
            var choices = dialog.Choices.Count > 0 ? dialog.Choices : new List<EncounterChoice> { new EncounterChoice("OK", null) };
            float h = Mathf.Min(40f, 144f / choices.Count);
            for (int i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                var rt = UIFactory.Rect("Choice" + i, choiceRoot, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0),
                                        new Vector2(0, (choices.Count - 1 - i) * (h + 4)), new Vector2(0, h));
                var button = UIFactory.Button(rt, skin, choice.Label, choice.Label.Length > 28 ? 10 : 13, () => Pick(choice));
                button.interactable = choice.Enabled;
                if (!choice.Enabled) rt.GetComponentInChildren<TextMeshProUGUI>().color = skin.mutedColor;
                buttons.Add(rt.gameObject);
            }
        }

        void Pick(EncounterChoice choice)
        {
            Close();
            choice.Pick?.Invoke();
        }

        public void Close()
        {
            if (!gameObject.activeSelf) return;
            gameObject.SetActive(false);
            var closed = onClosed;
            onClosed = null;
            closed?.Invoke();
        }

        void Update()
        {
            if (!revealing) return;
            int n = Mathf.FloorToInt((Time.unscaledTime - revealStart) * CharsPerSecond);
            body.maxVisibleCharacters = n;
            if (n >= body.text.Length) revealing = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            revealing = false;
            body.maxVisibleCharacters = body.text.Length;
        }
    }
}
