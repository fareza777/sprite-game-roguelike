using System;
using System.Collections.Generic;
using System.Linq;
using Oathdeep.Core;
using Oathdeep.Loot;
using TMPro;
using UnityEngine;

namespace Oathdeep.UI
{
    /// <summary>Every item identity in the game. Found items show their art, base stats and flavor; the rest are
    /// silhouettes with a hint of where to look.</summary>
    public sealed class ItemCompendiumPage : MonoBehaviour
    {
        const int Columns = 6;
        const float CellSize = 44f;

        PageView page;
        UISkin skin;
        ItemBase selected;

        public static ItemCompendiumPage Create(RectTransform parent, UISkin skin)
        {
            var page = PageView.Create("Items", parent, skin, "ITEMS", null);
            var view = page.gameObject.AddComponent<ItemCompendiumPage>();
            view.page = page;
            view.skin = skin;
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
            var found = SaveSystem.Current.items;
            int count = ItemCatalog.All.Count(b => found.Contains(b.Id));
            page.Paragraph($"Found {count} of {ItemCatalog.Count}. Every item rolls new stats each time it drops.",
                           10, skin.mutedColor, TextAlignmentOptions.Center);

            BuildDetail(found);

            foreach (EquipSlot slot in Enum.GetValues(typeof(EquipSlot)))
                Section(slot == EquipSlot.Armor ? "ARMOR" : ItemWidgets.SlotName(slot) + "S", ItemCatalog.Bases.Where(b => b.Slot == slot).ToList(), found);
            Section("LEGENDARIES", ItemCatalog.Legendaries.ToList(), found);
        }

        void Section(string title, List<ItemBase> items, List<string> found)
        {
            int have = items.Count(b => found.Contains(b.Id));
            page.Heading($"{title}  {have}/{items.Count}");
            for (int start = 0; start < items.Count; start += Columns)
            {
                var row = ItemWidgets.GridRow(page, title + start, Columns, CellSize, 4f);
                foreach (var item in items.Skip(start).Take(Columns))
                {
                    bool known = found.Contains(item.Id);
                    var color = item.Legendary ? RarityInfo.ColorOf(Rarity.Legendary) : new Color(1, 1, 1, 0.55f);
                    ItemWidgets.Cell(row, skin, ItemIcons.For(item.Icon), known ? color : new Color(1, 1, 1, 0.2f),
                                     item == selected, () => { selected = item; Rebuild(); }, !known);
                }
            }
        }

        void BuildDetail(List<string> found)
        {
            if (selected == null) return;
            bool known = found.Contains(selected.Id);
            string kind = (selected.Legendary ? "LEGENDARY " : "") + ItemWidgets.SlotName(selected.Slot);
            page.Heading(kind + "  from B" + selected.MinFloor);
            if (!known)
            {
                page.Paragraph("???", 15, skin.mutedColor);
                page.Paragraph(selected.Legendary
                    ? $"A legendary of the {FloorName(selected.MinFloor)}. Bosses and elites guard such things."
                    : $"Not yet found. It starts dropping around B{selected.MinFloor}.", 11, skin.mutedColor);
                return;
            }
            var color = selected.Legendary ? RarityInfo.Hex(Rarity.Legendary) : "#ebede9";
            page.Paragraph($"<color={color}>{selected.Name}</color>", 15);
            var lines = ItemInstance.Describe(selected.Stats, selected.Specials);
            page.Paragraph((selected.Legendary ? "" : "Base, before rarity and depth:\n") + string.Join("\n", lines), 11);
            page.Paragraph("\"" + selected.Flavor + "\"", 10, skin.mutedColor);
        }

        static string FloorName(int floor)
        {
            string[] names = { "Sunken Crypt", "Mycelial Hollow", "Drowned Cistern", "Ember Forge", "Frostbone Caverns",
                               "Thornwild Ruins", "Clockwork Ossuary", "Sandsunk Necropolis", "Glass Abyss", "Heart of Dusk" };
            return names[Mathf.Clamp(floor, 1, names.Length) - 1];
        }
    }
}
