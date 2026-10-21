using System;
using System.Collections;
using Oathdeep.Core;
using Oathdeep.Player;
using TMPro;
using UnityEngine;

namespace Oathdeep.UI
{
    /// <summary>
    /// First-run tutorial: one hint at a time, each cleared by actually doing it (move, strike, roll, skill),
    /// with a pulsing ring on the control it talks about. Skippable; completion is saved.
    /// </summary>
    public sealed class OnboardingCoach : MonoBehaviour
    {
        struct Step
        {
            public string Text;
            public string Control;               // child of the HUD controls root to highlight, or null
            public Func<bool> Done;
        }

        UISkin skin;
        HudView hud;
        PlayerController player;
        TextMeshProUGUI text;
        CanvasGroup group;
        RectTransform ring;
        float moved;
        PlayerAction? lastAction;

        public static OnboardingCoach Begin(RectTransform parent, UISkin skin, HudView hud, PlayerController player)
        {
            var root = UIFactory.Rect("Onboarding", parent, new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 78));
            UIFactory.Image(root, skin.panel, new Color(1, 1, 1, 0.92f), true, false);
            var coach = root.gameObject.AddComponent<OnboardingCoach>();
            coach.skin = skin;
            coach.hud = hud;
            coach.player = player;
            coach.group = root.gameObject.AddComponent<CanvasGroup>();
            coach.group.blocksRaycasts = true;
            var textRect = UIFactory.Stretch("Text", root);
            textRect.offsetMin = new Vector2(12, 22);
            textRect.offsetMax = new Vector2(-12, -8);
            coach.text = UIFactory.Text(textRect, skin, "", 12, TextAlignmentOptions.Center);
            coach.text.textWrappingMode = TextWrappingModes.Normal;
            var skip = UIFactory.Rect("Skip", root, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-8, 4), new Vector2(60, 18));
            UIFactory.Button(skip, skin, "SKIP", 9, coach.Finish);
            player.Performed += a => coach.lastAction = a;
            coach.StartCoroutine(coach.Run());
            return coach;
        }

        IEnumerator Run()
        {
            var steps = new[]
            {
                new Step { Text = "Drag anywhere on the lower left to move.", Control = null, Done = () => moved > 0.6f },
                new Step { Text = "Tap the sword to strike. Keep tapping to chain a combo.", Control = "Attack", Done = () => Took(PlayerAction.Attack) },
                new Step { Text = "Tap the boots to roll. Nothing can touch you mid-roll.", Control = "Dodge", Done = () => Took(PlayerAction.Dodge) },
                new Step { Text = "Tap the shield for your hero's skill. It needs time to recharge.", Control = "Skill", Done = () => Took(PlayerAction.Skill) },
                new Step { Text = "Doors seal when foes rise. Silence them all to move on.\nThe Sun is waiting below.", Control = null, Done = null },
            };
            foreach (var step in steps)
            {
                text.text = step.Text;
                lastAction = null;
                ShowRing(step.Control);
                yield return FadeTo(1f);
                if (step.Done == null) yield return new WaitForSecondsRealtime(4f);
                else while (!step.Done()) yield return null;
                Core.Haptics.Pulse(10);
                Audio.AudioManager.Play(Audio.Sfx.UiConfirm);
                yield return FadeTo(0f);
            }
            Finish();
        }

        bool Took(PlayerAction action) => lastAction == action;

        void Update()
        {
            if (player != null && PlayerMoving()) moved += Time.deltaTime;
            if (ring != null) ring.localScale = Vector3.one * (1.1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.06f);
        }

        bool PlayerMoving()
        {
            var body = player.GetComponent<Rigidbody2D>();
            return body != null && body.linearVelocity.sqrMagnitude > 0.5f;
        }

        void ShowRing(string control)
        {
            if (ring != null) Destroy(ring.gameObject);
            ring = null;
            var target = control != null ? hud.ControlRect(control) : null;
            if (target == null) return;
            ring = UIFactory.Stretch("CoachRing", target);
            UIFactory.Image(ring, skin.actionRing, new Color(skin.accentColor.r, skin.accentColor.g, skin.accentColor.b, 0.55f));
        }

        IEnumerator FadeTo(float target)
        {
            float start = group.alpha;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.25f)
            {
                group.alpha = Mathf.Lerp(start, target, t);
                yield return null;
            }
            group.alpha = target;
        }

        void Finish()
        {
            SaveSystem.Current.onboardingDone = true;
            SaveSystem.Save();
            if (ring != null) Destroy(ring.gameObject);
            Destroy(gameObject);
        }
    }
}
