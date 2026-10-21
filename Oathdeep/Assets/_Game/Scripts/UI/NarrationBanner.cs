using System.Collections;
using Oathdeep.Audio;
using Oathdeep.Core;
using TMPro;
using UnityEngine;

namespace Oathdeep.UI
{
    /// <summary>
    /// In-run storytelling: the floor title card with the Sun's narration when a floor begins, and a boss
    /// card (name + who they were) when a Chorister appears.
    /// </summary>
    public sealed class NarrationBanner : MonoBehaviour
    {
        UISkin skin;
        CanvasGroup group;
        TextMeshProUGUI title, subtitle, body;

        public static NarrationBanner Create(RectTransform parent, UISkin skin)
        {
            // Below the health bar and left of the minimap/bag column, stretching with the screen, so it never covers
            // them on a narrow 20:9 phone. The first-run coach sits under the bag at the same time.
            var root = UIFactory.Rect("Narration", parent, new Vector2(0.03f, 1f), new Vector2(0.7f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -64), new Vector2(0, 110));
            var banner = root.gameObject.AddComponent<NarrationBanner>();
            banner.skin = skin;
            banner.group = root.gameObject.AddComponent<CanvasGroup>();
            banner.group.blocksRaycasts = false;
            banner.group.alpha = 0f;
            var shade = UIFactory.Image(root, skin.white, new Color(0.035f, 0.04f, 0.08f, 0.65f));
            shade.preserveAspect = false;
            banner.title = UIFactory.Text(UIFactory.Rect("Title", root, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(0, 26)),
                skin, "", 20, TextAlignmentOptions.Center, skin.accentColor);
            banner.subtitle = UIFactory.Text(UIFactory.Rect("Subtitle", root, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -32), new Vector2(0, 16)),
                skin, "", 11, TextAlignmentOptions.Center, skin.mutedColor);
            banner.body = UIFactory.Text(UIFactory.Rect("Body", root, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -22), new Vector2(-24, -56)),
                skin, "", 11, TextAlignmentOptions.Top);
            banner.body.textWrappingMode = TextWrappingModes.Normal;
            return banner;
        }

        public void ShowFloor(FloorStory floor, AudioClip voice)
        {
            if (floor == null) return;
            StopAllCoroutines();
            StartCoroutine(Show($"B{floor.floor}  {floor.biome.ToUpperInvariant()}", $"The Verse of {floor.verse}", floor.entry, voice, 5.5f));
        }

        public void ShowBoss(FloorStory floor)
        {
            if (floor == null) return;
            StopAllCoroutines();
            StartCoroutine(Show(floor.boss.ToUpperInvariant(), $"Chorister of {floor.verse}", floor.bossLine, null, 3.6f));
        }

        IEnumerator Show(string t, string s, string b, AudioClip voice, float minSeconds)
        {
            title.text = t;
            subtitle.text = s;
            body.text = b;
            float hold = Mathf.Max(minSeconds, AudioManager.PlayVoice(voice) + 0.5f);
            for (float k = 0f; k < 1f; k += Time.unscaledDeltaTime / 0.5f) { group.alpha = k; yield return null; }
            group.alpha = 1f;
            yield return new WaitForSecondsRealtime(hold);
            for (float k = 1f; k > 0f; k -= Time.unscaledDeltaTime / 0.8f) { group.alpha = k; yield return null; }
            group.alpha = 0f;
        }
    }
}
