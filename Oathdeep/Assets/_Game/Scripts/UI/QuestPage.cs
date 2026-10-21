using Oathdeep.Audio;
using Oathdeep.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Oathdeep.UI
{
    /// <summary>Main story chapters and side quests. Completed side quests are claimed here for permanent bonuses.</summary>
    public sealed class QuestPage : MonoBehaviour
    {
        PageView page;
        UISkin skin;
        bool sideTab;

        public static QuestPage Create(RectTransform parent, UISkin skin)
        {
            var page = PageView.Create("Quests", parent, skin, "QUESTS", SaveSystem.Save);
            var view = page.gameObject.AddComponent<QuestPage>();
            view.page = page;
            view.skin = skin;
            return view;
        }

        public void Show()
        {
            page.Show();
            Rebuild();
        }

        void Rebuild()
        {
            page.Clear();
            var tabs = page.Row("Tabs", 36);
            Tab(tabs, 0f, 0.5f, "MAIN", !sideTab, () => { sideTab = false; Rebuild(); });
            Tab(tabs, 0.5f, 1f, "SIDE", sideTab, () => { sideTab = true; Rebuild(); });
            if (sideTab) BuildSide(); else BuildMain();
        }

        void Tab(RectTransform row, float from, float to, string label, bool on, System.Action pick)
        {
            var rt = UIFactory.Rect(label, row, new Vector2(from, 0), new Vector2(to, 1), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-6, 0));
            UIFactory.Button(rt, skin, label, 14, pick);
            rt.GetComponentInChildren<TextMeshProUGUI>().color = on ? skin.accentColor : skin.mutedColor;
        }

        void BuildMain()
        {
            var save = SaveSystem.Current;
            foreach (var c in QuestCatalog.Main)
            {
                bool done = QuestCatalog.IsDone(c, save);
                page.Heading((done ? "<color=#75a743>DONE</color>  " : "") + c.Title.ToUpperInvariant());
                page.Paragraph(done ? c.Journal : c.Goal, 11, done ? skin.textColor : skin.mutedColor);
            }
        }

        void BuildSide()
        {
            var save = SaveSystem.Current;
            var m = save.meta;
            page.Paragraph($"Permanent bonuses: +{m.health} HP  +{m.attack} ATK  +{m.defense} DEF  +{m.crit * 100f:0}% CRIT  " +
                           $"+{m.goldFind * 100f:0}% GOLD  +{m.skillHaste * 100f:0}% HASTE  +{m.startGold} start gold",
                           10, skin.critColor, TextAlignmentOptions.Center);
            foreach (var q in QuestCatalog.Side)
            {
                var p = save.Quest(q.Id);
                page.Heading(q.Title.ToUpperInvariant() + (p.claimed ? "  <color=#75a743>CLAIMED</color>" : ""));
                page.Paragraph(q.Description + "\n<color=#de9e41>Reward: " + q.Reward.Describe() + "</color>", 11);
                var row = page.Row("Progress" + q.Id, 26);
                Bar(row, p.progress, q.Goal);
                if (p.completed && !p.claimed)
                {
                    var claim = UIFactory.Rect("Claim", row, new Vector2(0.7f, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                    var quest = q;
                    UIFactory.Button(claim, skin, "CLAIM", 12, () =>
                    {
                        if (!QuestCatalog.Claim(SaveSystem.Current, quest)) return;
                        SaveSystem.Save();
                        AudioManager.Play(Sfx.UiConfirm);
                        Rebuild();
                    });
                }
            }
        }

        void Bar(RectTransform row, int progress, int goal)
        {
            var frame = UIFactory.Rect("Bar", row, new Vector2(0, 0.5f), new Vector2(0.66f, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(0, 14));
            UIFactory.Image(frame, skin.barFrame, null, true);
            var fill = UIFactory.Rect("Fill", frame, Vector2.zero, new Vector2(Mathf.Clamp01(progress / (float)goal), 1), new Vector2(0, 0.5f), new Vector2(2, 0), new Vector2(-4, -4));
            var img = UIFactory.Image(fill, skin.white, progress >= goal ? new Color32(0x75, 0xA7, 0x43, 0xFF) : skin.accentColor);
            img.preserveAspect = false;
            var label = UIFactory.Stretch("Count", frame);
            UIFactory.Text(label, skin, $"{progress}/{goal}", 9, TextAlignmentOptions.Center);
        }
    }
}
