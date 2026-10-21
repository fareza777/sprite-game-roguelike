using System.Collections.Generic;
using Oathdeep.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Oathdeep.UI
{
    /// <summary>
    /// Floating stick: touch anywhere inside the zone, the base appears under the thumb and follows long drags.
    /// Fixed stick: the base stays at its rest position.
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
        Vector2? restFraction;   // a player-placed rest point, as a fraction of the zone
        int pointerId = -1;
        int touchId = -1;   // -1 = mouse/pen

        /// <summary>
        /// Must set the rest position here: Awake runs during AddComponent, before the stick exists, and a zero rest
        /// position used to snap the stick into the bottom-left corner after every release.
        /// </summary>
        public void Setup(RectTransform baseRect, RectTransform knobRect, CanvasGroup group, bool isFloating)
        {
            stickBase = baseRect;
            knob = knobRect;
            visual = group;
            floating = isFloating;
            restPosition = stickBase.anchoredPosition;
            SetActiveLook(false);
        }

        void Awake() => zone = (RectTransform)transform;

        /// <summary>Rest the stick at a player-chosen point (fraction of the zone) instead of the built-in spot.</summary>
        public void UseRestFraction(Vector2 fraction)
        {
            restFraction = fraction;
            PlaceAtRest();
        }

        Vector2 Rest => restFraction.HasValue && zone != null ? Vector2.Scale(zone.rect.size, restFraction.Value) : restPosition;

        void PlaceAtRest()
        {
            if (pointerId == -1 && stickBase != null) stickBase.anchoredPosition = Rest;
        }

        // The zone only gets its real size once the canvas lays out; keep the resting stick on its spot.
        void OnRectTransformDimensionsChange() => PlaceAtRest();

        public void OnPointerDown(PointerEventData e)
        {
            if (pointerId != -1) return;
            pointerId = e.pointerId;
            touchId = e is ExtendedPointerEventData ext && ext.pointerType == UIPointerType.Touch ? ext.touchId : -1;
            if (floating && RectTransformUtility.ScreenPointToLocalPointInRectangle(zone, e.position, e.pressEventCamera, out var local))
                stickBase.anchoredPosition = ControlLayout.ClampStick(local, zone.rect.size, stickBase.sizeDelta.x);
            SetActiveLook(true);
            OnDrag(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != pointerId) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(stickBase, e.position, e.pressEventCamera, out var local)) return;
            if (floating && local.magnitude > radius)
            {
                // The base trails the thumb so a long drag never runs out of stick.
                var moved = ControlLayout.FollowStick(stickBase.anchoredPosition, local, radius);
                stickBase.anchoredPosition = ControlLayout.ClampStick(moved, zone.rect.size, stickBase.sizeDelta.x);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(stickBase, e.position, e.pressEventCamera, out local);
            }
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
            var touches = new List<(int, bool)>();
            var screen = Touchscreen.current;
            if (screen != null)
                foreach (var t in screen.touches)
                    touches.Add((t.touchId.ReadValue(), t.press.isPressed));
            var mouse = Mouse.current;
            return PointerHold.StillDown(touchId, touches, mouse != null ? mouse.leftButton.isPressed : (bool?)null);
        }

        void Release()
        {
            pointerId = -1;
            touchId = -1;
            knob.anchoredPosition = Vector2.zero;
            if (floating) stickBase.anchoredPosition = Rest;
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
