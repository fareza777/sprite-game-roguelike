using System;
using Oathdeep.Core;
using Oathdeep.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Oathdeep.UI
{
    /// <summary>On-screen action button: fires on press (not release) for responsiveness, shows a cooldown wipe.</summary>
    public sealed class ActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] PlayerAction action;
        [SerializeField] Image cooldownFill;
        [SerializeField] RectTransform visual;

        Func<float> cooldown;
        Vector3 baseScale = Vector3.one;

        public void Setup(PlayerAction a, Image fill, RectTransform face, Func<float> cooldownFraction)
        {
            action = a;
            cooldownFill = fill;
            visual = face;
            cooldown = cooldownFraction;
            if (visual != null) baseScale = visual.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PlayerInputReader.Press(action);
            if (visual != null) visual.localScale = baseScale * 0.9f;
            Haptics.Pulse(6);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (visual != null) visual.localScale = baseScale;
        }

        void Update()
        {
            if (cooldownFill == null || cooldown == null) return;
            float f = cooldown();
            cooldownFill.fillAmount = f;
            cooldownFill.enabled = f > 0.001f;
        }
    }
}
