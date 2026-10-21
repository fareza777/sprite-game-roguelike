using System;
using Oathdeep.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Oathdeep.UI
{
    /// <summary>
    /// Settings > Customize layout: drag the stick and the action buttons where the thumbs like them, then save.
    /// Positions are stored as fractions of the safe area, the same rectangle the HUD lays its controls out in.
    /// </summary>
    public sealed class ControlLayoutEditor : MonoBehaviour
    {
        static readonly Control[] Order = { Control.Stick, Control.Attack, Control.Dodge, Control.Skill };

        RectTransform board;
        readonly RectTransform[] handles = new RectTransform[4];
        bool resetChosen;
        Action onSaved;

        public static ControlLayoutEditor Create(RectTransform parent, UISkin skin)
        {
            var root = UIFactory.Stretch("ControlEditor", parent);
            var shade = UIFactory.Image(root, skin.white, new Color(0.02f, 0.03f, 0.06f, 0.97f), false, true);
            shade.preserveAspect = false;
            var editor = root.gameObject.AddComponent<ControlLayoutEditor>();
            editor.board = root;

            var hint = UIFactory.Text(UIFactory.Rect("Hint", root, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -56), new Vector2(-32, 44)),
                skin, "Drag each control to where your thumbs like it.", 13, TextAlignmentOptions.Center);
            hint.textWrappingMode = TextWrappingModes.Normal;
            Button(root, skin, "RESET", -104, editor.ResetToDefault);
            Button(root, skin, "CANCEL", 0, editor.Close);
            Button(root, skin, "SAVE", 104, editor.Save);

            var layout = ControlLayout.Compute(false);
            editor.handles[0] = editor.Handle(skin, "Stick", skin.joystickBase, null, ControlLayout.StickSize);
            editor.handles[1] = editor.Handle(skin, "Attack", skin.actionRing, skin.iconAttack, layout.Attack.Size);
            editor.handles[2] = editor.Handle(skin, "Dodge", skin.actionRing, skin.iconDodge, layout.Dodge.Size);
            editor.handles[3] = editor.Handle(skin, "Skill", skin.actionRing, skin.iconSkill, layout.Skill.Size);
            root.gameObject.SetActive(false);
            return editor;
        }

        static void Button(RectTransform root, UISkin skin, string label, float x, Action onClick)
        {
            var rect = UIFactory.Rect(label, root, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(x, -108), new Vector2(96, 36));
            UIFactory.Button(rect, skin, label, 13, onClick);
        }

        RectTransform Handle(UISkin skin, string name, Sprite ring, Sprite icon, float size)
        {
            var rect = UIFactory.Rect(name, board, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * size);
            UIFactory.Image(rect, ring, new Color(1, 1, 1, 0.9f), false, true);
            if (icon != null)
            {
                var iconRect = UIFactory.Rect("Icon", rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * size * 0.55f);
                UIFactory.Image(iconRect, icon);
            }
            rect.gameObject.AddComponent<Draggable>().Init(this, rect, size);
            return rect;
        }

        /// <summary>Shows the editor over everything, with the controls where the HUD has them now.</summary>
        public void Open(Action whenSaved)
        {
            onSaved = whenSaved;
            resetChosen = false;
            board.SetAsLastSibling();
            gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();
            var size = board.rect.size;
            var layout = ControlLayout.Compute(GameSettings.LeftHanded);
            var placed = GameSettings.ControlPositions;
            for (int i = 0; i < Order.Length; i++)
            {
                var centre = GameSettings.CustomControls ? ControlPlacement.Denormalize(placed[i], size) : ControlPlacement.DefaultCentre(Order[i], layout, size);
                Place(handles[i], centre, size);
            }
        }

        void ResetToDefault()
        {
            var size = board.rect.size;
            var layout = ControlLayout.Compute(GameSettings.LeftHanded);
            for (int i = 0; i < Order.Length; i++)
                Place(handles[i], ControlPlacement.DefaultCentre(Order[i], layout, size), size);
            resetChosen = true;
        }

        void Save()
        {
            if (resetChosen)
                GameSettings.ResetControlPositions();   // the built-in layout keeps following the left-handed toggle
            else
            {
                Vector2 N(int i) => handles[i].anchorMin;
                GameSettings.SaveControlPositions(N(0), N(1), N(2), N(3));
            }
            Close();
            onSaved?.Invoke();
        }

        void Close() => gameObject.SetActive(false);

        void Moved(RectTransform handle, Vector2 screenPoint, Camera cam, float size)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(board, screenPoint, cam, out var local)) return;
            Place(handle, local - board.rect.min, board.rect.size);
            resetChosen = false;
        }

        /// <summary>Pins a handle by its centre as a fraction of the board, so it stays put if the screen resizes.</summary>
        static void Place(RectTransform handle, Vector2 centre, Vector2 boardSize)
        {
            var at = ControlPlacement.Normalize(ControlPlacement.Clamp(centre, handle.sizeDelta.x, boardSize), boardSize);
            handle.anchorMin = handle.anchorMax = at;
            handle.anchoredPosition = Vector2.zero;
        }

        sealed class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler
        {
            ControlLayoutEditor editor;
            RectTransform rect;
            float size;

            public void Init(ControlLayoutEditor owner, RectTransform r, float s) { editor = owner; rect = r; size = s; }

            public void OnBeginDrag(PointerEventData e) => rect.SetAsLastSibling();

            public void OnDrag(PointerEventData e) => editor.Moved(rect, e.position, e.pressEventCamera, size);
        }
    }
}
