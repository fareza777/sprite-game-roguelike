using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Oathdeep.UI
{
    /// <summary>Boot splash: creator line, then the UNDERSUN logo rising out of black. Tap to skip.</summary>
    public sealed class SplashView : MonoBehaviour, IPointerClickHandler
    {
        CanvasGroup creatorGroup, logoGroup, rootGroup;
        RectTransform logo;
        Action done;
        bool finished;

        public static SplashView Create(RectTransform parent, UISkin skin, string creatorLine)
        {
            var root = UIFactory.Stretch("Splash", parent);
            var bg = UIFactory.Image(root, skin.white, new Color32(0x09, 0x0a, 0x14, 0xff), false, true);
            bg.preserveAspect = false;
            var view = root.gameObject.AddComponent<SplashView>();
            view.rootGroup = root.gameObject.AddComponent<CanvasGroup>();

            var creator = UIFactory.Rect("Creator", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 30));
            UIFactory.Text(creator, skin, creatorLine, 13, TextAlignmentOptions.Center, skin.mutedColor);
            view.creatorGroup = creator.gameObject.AddComponent<CanvasGroup>();

            view.logo = UIFactory.Rect("Logo", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(288, 144));
            UIFactory.Image(view.logo, skin.logo);
            view.logoGroup = view.logo.gameObject.AddComponent<CanvasGroup>();
            view.gameObject.SetActive(false);
            return view;
        }

        public void Play(Action onDone)
        {
            done = onDone;
            finished = false;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            creatorGroup.alpha = 0f;
            logoGroup.alpha = 0f;
            rootGroup.alpha = 1f;
            yield return Fade(creatorGroup, 0f, 1f, 0.6f);
            yield return Wait(0.9f);
            yield return Fade(creatorGroup, 1f, 0f, 0.4f);
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 1.1f)
            {
                logoGroup.alpha = t;
                logo.anchoredPosition = new Vector2(0, Mathf.Lerp(-10f, 20f, Mathf.SmoothStep(0, 1, t)));
                yield return null;
            }
            logoGroup.alpha = 1f;
            yield return Wait(1.2f);
            yield return Fade(rootGroup, 1f, 0f, 0.5f);
            Finish();
        }

        static IEnumerator Fade(CanvasGroup g, float from, float to, float seconds)
        {
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / seconds)
            {
                g.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
            g.alpha = to;
        }

        static IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime) yield return null;
        }

        public void OnPointerClick(PointerEventData eventData) => Finish();

        void Finish()
        {
            if (finished) return;
            finished = true;
            StopAllCoroutines();
            gameObject.SetActive(false);
            done?.Invoke();
        }
    }
}
