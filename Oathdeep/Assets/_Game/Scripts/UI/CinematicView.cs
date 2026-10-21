using System;
using System.Collections;
using Oathdeep.Audio;
using Oathdeep.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Oathdeep.UI
{
    /// <summary>
    /// Narrated slideshow: each panel slowly pans and zooms (Ken Burns) inside a gold frame while the Sun's voice
    /// reads the line and the caption types itself out. Tap to finish the line / go on; SKIP leaves at any time.
    /// </summary>
    public sealed class CinematicView : MonoBehaviour, IPointerClickHandler
    {
        const float MinHold = 4.5f;

        UISkin skin;
        StoryAssets story;
        CanvasGroup group, panelGroup;
        RectTransform panelImageRect;
        Image panelImage;
        TextMeshProUGUI caption;
        bool advance;
        bool done;
        Action onFinished;

        public static CinematicView Create(RectTransform parent, UISkin skin, StoryAssets story)
        {
            var root = UIFactory.Stretch("Cinematic", parent);
            var bg = UIFactory.Image(root, skin.white, new Color32(0x09, 0x0a, 0x14, 0xff), false, true);
            bg.preserveAspect = false;
            var view = root.gameObject.AddComponent<CinematicView>();
            view.skin = skin;
            view.story = story;
            view.group = root.gameObject.AddComponent<CanvasGroup>();

            var frame = UIFactory.Rect("Frame", root, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(328, 248));
            UIFactory.Image(frame, skin.white, skin.accentColor).preserveAspect = false;
            var mask = UIFactory.Stretch("Mask", frame);
            mask.offsetMin = new Vector2(4, 4);
            mask.offsetMax = new Vector2(-4, -4);
            UIFactory.Image(mask, skin.white, Color.black).preserveAspect = false;
            mask.gameObject.AddComponent<RectMask2D>();
            view.panelGroup = mask.gameObject.AddComponent<CanvasGroup>();
            view.panelImageRect = UIFactory.Stretch("Panel", mask);
            view.panelImage = UIFactory.Image(view.panelImageRect, null);
            view.panelImage.preserveAspect = false;

            var capRect = UIFactory.Rect("Caption", root, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 1f), new Vector2(0, -140), new Vector2(310, 120));
            view.caption = UIFactory.Text(capRect, skin, "", 13, TextAlignmentOptions.Top);
            view.caption.textWrappingMode = TextWrappingModes.Normal;
            view.caption.lineSpacing = 14f;

            var skip = UIFactory.Rect("Skip", root, Vector2.one, Vector2.one, Vector2.one, new Vector2(-10, -10), new Vector2(70, 26));
            UIFactory.Button(skip, skin, "SKIP", 11, () => view.Finish());
            var hint = UIFactory.Rect("Hint", root, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(200, 14));
            UIFactory.Text(hint, skin, "tap to continue", 9, TextAlignmentOptions.Center, skin.mutedColor);
            root.gameObject.SetActive(false);
            return view;
        }

        public void Play(StoryLine[] lines, MusicTrack music, Action finished)
        {
            onFinished = finished;
            done = false;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            group.alpha = 1f;
            AudioManager.PlayMusic(music);
            StartCoroutine(Run(lines));
        }

        IEnumerator Run(StoryLine[] lines)
        {
            for (int i = 0; i < lines.Length && !done; i++)
            {
                var line = lines[i];
                panelImage.sprite = story.PanelFor(line.id);
                caption.text = line.text;
                caption.maxVisibleCharacters = 0;
                float voiceLength = AudioManager.PlayVoice(story.VoiceFor(line.id));
                float hold = Mathf.Max(MinHold, voiceLength + 0.8f);
                float reveal = Mathf.Max(1.5f, voiceLength * 0.85f);
                advance = false;
                var kenBurns = KenBurns(i, hold + 1.2f);
                StartCoroutine(kenBurns);
                yield return Fade(panelGroup, 0f, 1f, 0.6f);
                for (float t = 0f; t < hold && !advance && !done; t += Time.unscaledDeltaTime)
                {
                    caption.maxVisibleCharacters = Mathf.CeilToInt(line.text.Length * Mathf.Clamp01(t / reveal));
                    yield return null;
                }
                caption.maxVisibleCharacters = line.text.Length;
                if (advance && !done) // first tap only finishes the line; wait for a second tap or a short beat
                {
                    advance = false;
                    for (float t = 0f; t < 1.2f && !advance && !done; t += Time.unscaledDeltaTime) yield return null;
                }
                AudioManager.StopVoice();
                yield return Fade(panelGroup, 1f, 0f, 0.5f);
                StopCoroutine(kenBurns);
            }
            Finish();
        }

        IEnumerator KenBurns(int index, float seconds)
        {
            // alternate drift directions so consecutive panels feel like camera moves
            var dir = new Vector2(index % 2 == 0 ? 1f : -1f, index % 3 == 0 ? 0.6f : -0.4f);
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                float k = t / seconds;
                panelImageRect.localScale = Vector3.one * Mathf.Lerp(1.02f, 1.12f, k);
                panelImageRect.anchoredPosition = dir * Mathf.Lerp(-6f, 6f, k);
                yield return null;
            }
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

        public void OnPointerClick(PointerEventData eventData) => advance = true;

        void Finish()
        {
            if (done) return;
            done = true;
            StopAllCoroutines();
            AudioManager.StopVoice();
            gameObject.SetActive(false);
            onFinished?.Invoke();
        }
    }
}
