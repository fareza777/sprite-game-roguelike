using System.Collections.Generic;
using System.Linq;
using Oathdeep.Core;
using Oathdeep.Data;
using Oathdeep.Dungeon;
using TMPro;
using UnityEngine;

namespace Oathdeep.UI
{
    /// <summary>The bestiary: forty creatures by floor. An entry opens the first time that creature falls to you.</summary>
    public sealed class CodexPage : MonoBehaviour
    {
        const int Columns = 4;
        const float CellSize = 64f;

        PageView page;
        UISkin skin;
        Dictionary<string, EnemyData> byId;
        BestiaryEntry selected;

        public static CodexPage Create(RectTransform parent, UISkin skin, IReadOnlyList<EnemyData> enemies)
        {
            var page = PageView.Create("Codex", parent, skin, "CODEX", null);
            var view = page.gameObject.AddComponent<CodexPage>();
            view.page = page;
            view.skin = skin;
            view.byId = enemies.Where(e => e != null && !string.IsNullOrEmpty(e.Id)).GroupBy(e => e.Id).ToDictionary(g => g.Key, g => g.First());
            return view;
        }

        public void Show()
        {
            selected = null;
            page.Show();
            Rebuild();
        }

        void Rebuild()
        {
            page.Clear();
            var save = SaveSystem.Current;
            int known = Bestiary.All.Count(e => save.IsCodexUnlocked(e.Id));
            page.Paragraph($"{known} of {Bestiary.All.Count} recorded. Defeat a creature to learn its story.", 10, skin.mutedColor,
                           TextAlignmentOptions.Center);
            BuildDetail(save);

            for (int floor = 1; floor <= 10; floor++)
            {
                var entries = Bestiary.OnFloor(floor).ToList();
                page.Heading($"B{floor}  {EncounterData.BiomeName(floor).ToUpperInvariant()}");
                var row = ItemWidgets.GridRow(page, "Floor" + floor, Columns, CellSize, 6f);
                foreach (var e in entries)
                {
                    var entry = e;
                    bool open = save.IsCodexUnlocked(e.Id);
                    byId.TryGetValue(e.Id, out var data);
                    var frame = e.IsBoss ? skin.critColor : new Color(1, 1, 1, open ? 0.55f : 0.2f);
                    ItemWidgets.Cell(row, skin, data != null ? data.Portrait : null, frame, entry == selected,
                                     () => { selected = entry; Rebuild(); page.GetComponentInChildren<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition = 1f; },
                                     !open, e.IsBoss ? "BOSS" : null);
                }
            }
        }

        void BuildDetail(SaveData save)
        {
            if (selected == null) return;
            bool open = save.IsCodexUnlocked(selected.Id);
            page.Heading((selected.IsBoss ? "CHORISTER" : selected.Archetype.ToString().ToUpperInvariant()) + "  B" + selected.Floor);
            if (!open)
            {
                page.Paragraph("???", 16, skin.mutedColor);
                page.Paragraph($"Something waits in the {EncounterData.BiomeName(selected.Floor)}. Defeat it to fill this page.", 11, skin.mutedColor);
                return;
            }
            page.Paragraph(selected.Name, 16, selected.IsBoss ? skin.critColor : skin.accentColor);
            page.Paragraph(selected.Lore, 11);
            var s = selected.Stats;
            page.Paragraph($"Health {s.MaxHealth}   Attack {s.Attack}   Defense {s.Defense}   (at B1 strength)", 10, skin.mutedColor);
        }
    }
}
