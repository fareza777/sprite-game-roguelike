using System;
using System.Collections.Generic;
using Oathdeep.Audio;
using Oathdeep.Core;
using Oathdeep.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Oathdeep.UI
{
    /// <summary>Hero select: an animated portrait, stat bars, lore and skill for the hero in focus, a strip of all heroes,
    /// and a CHOOSE button that stores the pick in the save.</summary>
    public sealed class HeroesPage : MonoBehaviour
    {
        // Bars are scaled against the strongest hero in each stat so differences read at a glance.
        static readonly (string label, Func<StatBlock, float> value, float max)[] Bars =
        {
            ("HEALTH", s => s.MaxHealth, 150f),
            ("ATTACK", s => s.Attack, 15f),
            ("DEFENSE", s => s.Defense, 8f),
            ("SPEED", s => s.MoveSpeed, 5f),
            ("CRIT", s => s.CritChance, 0.2f),
        };

        PageView page;
        UISkin skin;
        IReadOnlyList<HeroData> heroes;
        int focus;
        Image portrait;
        TextMeshProUGUI nameText, titleText, loreText, skillName, skillText, chooseText;
        readonly List<Image> barFills = new List<Image>();
        readonly List<TextMeshProUGUI> barValues = new List<TextMeshProUGUI>();
        readonly List<Image> tabFrames = new List<Image>();
        float frameClock;

        public static HeroesPage Create(RectTransform parent, UISkin skin, IReadOnlyList<HeroData> heroes)
        {
            var page = PageView.Create("Heroes", parent, skin, "HEROES", null);
            var view = page.gameObject.AddComponent<HeroesPage>();
            view.page = page;
            view.skin = skin;
            view.heroes = heroes;
            view.Build();
            return view;
        }

        public static string IdOf(HeroData hero) => hero.Id.ToString().ToLowerInvariant();

        /// <summary>The hero stored as the player's choice, or the first hero if the save names none we know.</summary>
        public static HeroData Selected(IReadOnlyList<HeroData> heroes)
        {
            string id = SaveSystem.Current.lastHero;
            foreach (var h in heroes) if (h != null && IdOf(h) == id) return h;
            return heroes.Count > 0 ? heroes[0] : null;
        }

        public void Show()
        {
            var chosen = Selected(heroes);
            focus = Mathf.Max(0, IndexOf(chosen));
            Refresh();
            page.Show();
        }

        int IndexOf(HeroData hero)
        {
            for (int i = 0; i < heroes.Count; i++) if (heroes[i] == hero) return i;
            return -1;
        }

        void Build()
        {
            var stage = page.Row("Stage", 132);
            var plinth = UIFactory.Rect("Plinth", stage, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 4), new Vector2(120, 10));
            UIFactory.Image(plinth, skin.white, new Color(0, 0, 0, 0.35f)).preserveAspect = false;
            var art = UIFactory.Rect("Portrait", stage, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, -14), new Vector2(144, 144));
            portrait = UIFactory.Image(art, null);
            Arrow(stage, "Prev", -1, "<");
            Arrow(stage, "Next", 1, ">");

            nameText = page.Paragraph("", 20, skin.accentColor, TextAlignmentOptions.Center);
            titleText = page.Paragraph("", 11, skin.mutedColor, TextAlignmentOptions.Center);

            foreach (var bar in Bars) BarRow(bar.label);

            page.Heading("SKILL");
            skillName = page.Paragraph("", 14, skin.critColor);
            skillText = page.Paragraph("", 11);
            page.Heading("STORY");
            loreText = page.Paragraph("", 11, skin.mutedColor);

            var tabs = page.Row("Tabs", 56);
            var layout = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            for (int i = 0; i < heroes.Count; i++) Tab(tabs, i);

            var choose = page.Row("Choose", 44);
            UIFactory.Button(choose, skin, "", 16, Choose);
            chooseText = choose.GetComponentInChildren<TextMeshProUGUI>();
        }

        void Arrow(RectTransform stage, string name, int step, string glyph)
        {
            float x = step < 0 ? 4 : -4;
            var anchor = new Vector2(step < 0 ? 0 : 1, 0.5f);
            var rt = UIFactory.Rect(name, stage, anchor, anchor, anchor, new Vector2(x, 0), new Vector2(40, 44));
            UIFactory.Button(rt, skin, glyph, 18, () => { focus = (focus + step + heroes.Count) % heroes.Count; Refresh(); });
        }

        void BarRow(string label)
        {
            var row = page.Row(label, 18);
            var l = UIFactory.Rect("Label", row, new Vector2(0, 0), new Vector2(0.3f, 1), new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
            UIFactory.Text(l, skin, label, 10, TextAlignmentOptions.Left, skin.mutedColor);
            var frame = UIFactory.Rect("Bar", row, new Vector2(0.3f, 0.5f), new Vector2(0.86f, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(0, 10));
            UIFactory.Image(frame, skin.barFrame, null, true);
            var fill = UIFactory.Rect("Fill", frame, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(2, 0), new Vector2(0, -4));
            var img = UIFactory.Image(fill, skin.white, skin.accentColor);
            img.preserveAspect = false;
            barFills.Add(img);
            var v = UIFactory.Rect("Value", row, new Vector2(0.88f, 0), new Vector2(1, 1), new Vector2(1, 0.5f), Vector2.zero, Vector2.zero);
            barValues.Add(UIFactory.Text(v, skin, "", 10, TextAlignmentOptions.Right));
        }

        void Tab(RectTransform strip, int index)
        {
            var rt = UIFactory.Rect("Tab" + index, strip, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var frame = UIFactory.Image(rt, skin.panel, null, true, true);
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = frame;
            b.onClick.AddListener(() => { AudioManager.Play(Sfx.UiClick); Haptics.Pulse(8); focus = index; Refresh(); });
            var icon = UIFactory.Rect("Icon", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -4), new Vector2(56, 56));
            UIFactory.Image(icon, heroes[index].Portrait);
            tabFrames.Add(frame);
        }

        void Refresh()
        {
            var hero = heroes[focus];
            nameText.text = hero.DisplayName.ToUpperInvariant();
            titleText.text = hero.Title;
            skillName.text = hero.SkillName + $"   <color=#819796>{hero.SkillCooldown:0.#}s</color>";
            skillText.text = hero.SkillDescription;
            loreText.text = hero.Description;
            portrait.sprite = hero.Portrait;

            var stats = hero.BaseStats;
            for (int i = 0; i < Bars.Length; i++)
            {
                float v = Bars[i].value(stats);
                barFills[i].rectTransform.anchorMax = new Vector2(Mathf.Clamp01(v / Bars[i].max), 1);
                barFills[i].rectTransform.sizeDelta = new Vector2(-4, -4);
                barValues[i].text = Bars[i].label == "CRIT" ? $"{v * 100f:0}%" : Bars[i].label == "SPEED" ? v.ToString("0.0") : v.ToString("0");
            }

            bool chosen = IdOf(hero) == SaveSystem.Current.lastHero;
            chooseText.text = chosen ? "CHOSEN" : "CHOOSE " + hero.DisplayName.ToUpperInvariant();
            chooseText.color = chosen ? skin.mutedColor : skin.accentColor;
            for (int i = 0; i < tabFrames.Count; i++) tabFrames[i].color = i == focus ? skin.accentColor : Color.white;
        }

        void Choose()
        {
            SaveSystem.Current.lastHero = IdOf(heroes[focus]);
            SaveSystem.Save();
            AudioManager.Play(Sfx.UiConfirm);
            Refresh();
        }

        void Update()
        {
            // Idle loop on unscaled time: menus run while the game clock is stopped.
            var frames = heroes.Count > 0 ? heroes[focus].IdleFrames : null;
            if (frames == null || frames.Length == 0) return;
            frameClock += Time.unscaledDeltaTime;
            portrait.sprite = frames[(int)(frameClock / 0.24f) % frames.Length];
        }
    }
}
