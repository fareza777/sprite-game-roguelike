using Oathdeep.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Oathdeep.UI
{
    /// <summary>
    /// Floating stick: touch anywhere inside the zone, the base appears under the thumb.
    /// Knob travel is clamped to radius; small dead zone avoids drift.
    /// A per-frame watchdog releases the stick if its touch ends without a pointer-up event
    /// (cancelled touches, system gestures, focus loss), which otherwise leaves the knight walking on its own.
    /// </summary>
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] RectTransform stickBase;
        [SerializeField] RectTransform knob;
        [SerializeField] float radius = 56f;
        [SerializeField, Range(0f, 0.5f)] float deadZone = 0.12f;
        [SerializeField] bool floating = true;
        [SerializeField] CanvasGroup visual;
        [SerializeField, Range(0f, 1f)] float idleAlpha = 0.35f;

        RectTransform zone;
        Vector2 restPosition;
        int pointerId = -1;
        int touchId = -1;   // -1 = mouse/pen

        public void Setup(RectTransform baseRect, RectTransform knobRect, CanvasGroup group)
        {
            stickBase = baseRect;
            knob = knobRect;
            visual = group;
        }

        void Awake()
        {
            zone = (RectTransform)transform;
            if (stickBase != null) restPosition = stickBase.anchoredPosition;
            SetActiveLook(false);
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (pointerId != -1) return;
            pointerId = e.pointerId;
            touchId = e is ExtendedPointerEventData ext && ext.pointerType == UIPointerType.Touch ? ext.touchId : -1;
            if (floating && RectTransformUtility.ScreenPointToLocalPointInRectangle(zone, e.position, e.pressEventCamera, out var local))
                stickBase.anchoredPosition = local;
            SetActiveLook(true);
            OnDrag(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != pointerId) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(stickBase, e.position, e.pressEventCamera, out var local)) return;
            var offset = Vector2.ClampMagnitude(local, radius);
            knob.anchoredPosition = offset;
            var value = offset / radius;
            PlayerInputReader.SetTouchMove(value.magnitude < deadZone ? Vector2.zero : value);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId == pointerId) Release();
        }

        void Update()
        {
            if (pointerId != -1 && !PointerStillDown()) Release();
        }

        void OnApplicationFocus(bool focused)
        {
            if (!focused && pointerId != -1) Release();
        }

        bool PointerStillDown()
        {
            if (touchId >= 0)
            {
                var screen = Touchscreen.current;
                if (screen == null) return false;
                foreach (var t in screen.touches)
                    if (t.touchId.ReadValue() == touchId && t.press.isPressed) return true;
                return false;
            }
            var mouse = Mouse.current;
            return mouse == null || mouse.leftButton.isPressed;   // unknown pointer: trust the event system
        }

        void Release()
        {
            pointerId = -1;
            touchId = -1;
            knob.anchoredPosition = Vector2.zero;
            if (floating) stickBase.anchoredPosition = restPosition;
            PlayerInputReader.SetTouchMove(Vector2.zero);
            SetActiveLook(false);
        }

        void OnDisable()
        {
            pointerId = -1;
            PlayerInputReader.SetTouchMove(Vector2.zero);
        }

        void SetActiveLook(bool active)
        {
            if (visual != null) visual.alpha = active ? 1f : idleAlpha;
        }
    }
}
