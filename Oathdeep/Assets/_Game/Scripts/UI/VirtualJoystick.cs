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
    /// Fixed stick: the base stays at its rest position. Both modes are invisible until held.
    /// Knob travel is clamped to radius; small dead zone avoids drift.
    /// A per-frame watchdog releases the stick if its touch ends without a pointer-up event
    /// (cancelled touches, system gestures, focus loss), which otherwise leaves the knight walking on its own.
    /// </summary>
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IInitializePotentialDragHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] RectTransform stickBase;
        [SerializeField] RectTransform knob;
        [SerializeField] float radius = 56f;
        [SerializeField, Range(0f, 0.5f)] float deadZone = 0.12f;
        [SerializeField] bool floating = true;
        [SerializeField] CanvasGroup visual;

        RectTransform zone;
        Vector2 restPosition;
        Vector2? restFraction;   // a player-placed rest point, as a fraction of the zone
        bool held;
        int pointerId;
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
            zone = (RectTransform)transform;
            restPosition = stickBase.anchoredPosition;
            if (visual != null) visual.blocksRaycasts = false;
            Release();
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
            if (!held && stickBase != null) stickBase.anchoredPosition = Rest;
        }

        // The zone only gets its real size once the canvas lays out; keep the resting stick on its spot.
        void OnRectTransformDimensionsChange() => PlaceAtRest();

        public void OnPointerDown(PointerEventData e)
        {
            if (held || stickBase == null || knob == null || e.button != PointerEventData.InputButton.Left) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(zone, e.position, e.pressEventCamera, out var local)) return;
            held = true;
            pointerId = e.pointerId;
            touchId = e is ExtendedPointerEventData ext && ext.pointerType == UIPointerType.Touch ? ext.touchId : -1;
            // Start exactly under the finger, including at an edge: clamping here used to move the hero on touch-down.
            if (floating) stickBase.anchoredPosition = local - zone.rect.min;
            else stickBase.anchoredPosition = Rest;
            SetActiveLook(true);
            OnDrag(e);
        }

        public void OnInitializePotentialDrag(PointerEventData e) => e.useDragThreshold = false;

        public void OnDrag(PointerEventData e)
        {
            if (!held || e.pointerId != pointerId) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(stickBase, e.position, e.pressEventCamera, out var local)) return;
            // Keep the whole knob inside the rim, rather than allowing its centre to reach the outer edge.
            float travel = Mathf.Max(1f, Mathf.Min(radius,
                (Mathf.Min(stickBase.rect.width, stickBase.rect.height) - Mathf.Max(knob.rect.width, knob.rect.height)) / 2f - 4f));
            if (floating && local.magnitude > travel)
            {
                // The initial zone captures the gesture; an owned drag can continue beyond it without a hard stop.
                stickBase.anchoredPosition = ControlLayout.FollowStick(stickBase.anchoredPosition, local, travel);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(stickBase, e.position, e.pressEventCamera, out local);
            }
            var offset = Vector2.ClampMagnitude(local, travel);
            knob.anchoredPosition = offset;
            var value = offset / travel;
            float magnitude = value.magnitude;
            PlayerInputReader.SetTouchMove(magnitude <= deadZone ? Vector2.zero : value.normalized * ((magnitude - deadZone) / (1f - deadZone)));
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (held && e.pointerId == pointerId) Release();
        }

        void Update()
        {
            if (held && !PointerStillDown()) Release();
        }

        void OnApplicationFocus(bool focused)
        {
            if (!focused) Release();
        }

        void OnApplicationPause(bool paused) { if (paused) Release(); }

        bool PointerStillDown()
        {
            var touches = new List<(int, bool)>();
            var screen = Touchscreen.current;
            if (screen != null)
                foreach (var t in screen.touches)
                    touches.Add((t.touchId.ReadValue(), t.press.isPressed));
            var mouse = Mouse.current;
            var pen = Pen.current;
            return PointerHold.StillDown(touchId, touches,
                mouse != null || pen != null ? mouse != null && mouse.leftButton.isPressed || pen != null && pen.tip.isPressed : (bool?)null);
        }

        void Release()
        {
            held = false;
            pointerId = 0;
            touchId = -1;
            if (knob != null) knob.anchoredPosition = Vector2.zero;
            PlaceAtRest();
            PlayerInputReader.SetTouchMove(Vector2.zero);
            SetActiveLook(false);
        }

        void OnDisable() => Release();

        void SetActiveLook(bool active)
        {
            if (visual != null) visual.alpha = active ? 1f : 0f;
        }
    }
}
