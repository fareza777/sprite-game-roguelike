using System;
using System.Linq;
using Oathdeep.Audio;
using Oathdeep.Loot;
using Oathdeep.Player;
using TMPro;
using UnityEngine;

namespace Oathdeep.UI
{
    /// <summary>In-run equipment screen: the four worn slots, the hero's live stats, the bag, and a detail panel
    /// for the selected item with a comparison against what is worn and Equip / Unequip / Salvage actions.</summary>
    public sealed class InventoryPage : MonoBehaviour
    {
        const int Columns = 4;
        const float CellSize = 62f;

        PageView page;
        UISkin skin;
        PlayerInventory inventory;
        ItemInstance selected;
        bool rebuildQueued;

        public static InventoryPage Create(RectTransform parent, UISkin skin, Action onClosed)
        {
            var page = PageView.Create("Inventory", parent, skin, "EQUIPMENT", onClosed);
            var view = page.gameObject.AddComponent<InventoryPage>();
            view.page = page;
            view.skin = skin;
            return view;
        }

        public bool IsOpen => page.gameObject.activeSelf;

        public void Show(PlayerInventory inv)
        {
            if (inventory != null) inventory.Changed -= QueueRebuild;
            inventory = inv;
            inventory.Changed += QueueRebuild;
            selected = null;
            page.Show();
            Rebuild();
        }

        public void Hide()
        {
            if (inventory != null) inventory.Changed -= QueueRebuild;
            page.Hide();
        }

        void QueueRebuild() => rebuildQueued = true;

        void LateUpdate()
        {
            if (!rebuildQueued) return;
            rebuildQueued = false;
            Rebuild();
        }

        void Rebuild()
        {
            page.Clear();
            if (inventory == null || inventory.Loadout == null) return;
            var loadout = inventory.Loadout;

            var worn = ItemWidgets.GridRow(page, "Worn", Columns, CellSize);
            foreach (EquipSlot slot in Enum.GetValues(typeof(EquipSlot)))
            {
                var item = loadout.Get(slot);
                ItemWidgets.Cell(worn, skin, item != null ? ItemIcons.For(item) : null,
                    item != null ? RarityInfo.ColorOf(item.Rarity) : new Color(1, 1, 1, 0.35f), item != null && item == selected,
                    item != null ? (Action)(() => Select(item)) : null, false, ItemWidgets.SlotName(slot));
            }

            var t = loadout.Total;
            var x = loadout.Specials;
            string stats = $"ATK {t.Attack}   HP {t.MaxHealth}   DEF {t.Defense}   CRIT {t.CritChance * 100f:0}%   SPD {t.MoveSpeed:0.0}";
            if (!x.IsEmpty)
                stats += "\n<color=#819796>" + string.Join("   ", ItemInstance.Describe(default, x)) + "</color>";
            page.Paragraph(stats, 10, skin.textColor, TextAlignmentOptions.Center);

            page.Heading($"BAG  {inventory.Bag.Count}/{PlayerInventory.BagSize}");
            for (int r = 0; r < PlayerInventory.BagSize / Columns; r++)
            {
                var row = ItemWidgets.GridRow(page, "Bag" + r, Columns, CellSize);
                for (int c = 0; c < Columns; c++)
                {
                    int i = r * Columns + c;
                    var item = i < inventory.Bag.Count ? inventory.Bag[i] : null;
                    ItemWidgets.Cell(row, skin, item != null ? ItemIcons.For(item) : null,
                        item != null ? RarityInfo.ColorOf(item.Rarity) : new Color(1, 1, 1, 0.2f), item != null && item == selected,
                        item != null ? (Action)(() => Select(item)) : null);
                }
            }

            BuildDetail();
        }

        void Select(ItemInstance item)
        {
            selected = item;
            Rebuild();
        }

        void BuildDetail()
        {
            if (selected == null)
            {
                page.Paragraph("Tap an item to inspect it.", 11, skin.mutedColor, TextAlignmentOptions.Center);
                return;
            }

            bool worn = inventory.Loadout.Get(selected.Slot) == selected;
            page.Heading(selected.Rarity.ToString().ToUpperInvariant() + "  " + ItemWidgets.SlotName(selected.Slot) + "  LV " + selected.Level);
            page.Paragraph(ItemWidgets.Colored(selected), 15);
            page.Paragraph(selected.Tooltip(), 11);

            if (!worn)
            {
                var current = inventory.Loadout.Get(selected.Slot);
                int delta = selected.Power - (current != null ? current.Power : 0);
                string verdict = delta > 0 ? $"<color=#75a743>+{delta} power</color>" : delta < 0 ? $"<color=#cf573c>{delta} power</color>" : "same power";
                page.Paragraph(current != null ? $"vs {current.Name}: {verdict}" : $"Empty slot: {verdict}", 10, skin.mutedColor);
            }
            if (!string.IsNullOrEmpty(selected.Base.Flavor))
                page.Paragraph("\"" + selected.Base.Flavor + "\"", 10, skin.mutedColor);

            var actions = page.Row("Actions", 40);
            if (worn)
            {
                Button(actions, 0f, 1f, "UNEQUIP", () =>
                {
                    if (!inventory.Unequip(selected.Slot)) Core.GameEvents.RaiseToast("Bag full");
                });
            }
            else
            {
                Button(actions, 0f, 0.5f, "EQUIP", () => { inventory.EquipFromBag(selected); AudioManager.Play(Sfx.ItemPickup); });
                Button(actions, 0.5f, 1f, $"SALVAGE +{selected.SellValue}", () =>
                {
                    inventory.Salvage(selected);
                    selected = null;
                    AudioManager.Play(Sfx.Coin);
                });
            }
        }

        void Button(RectTransform row, float from, float to, string label, Action onClick)
        {
            var rt = UIFactory.Rect(label, row, new Vector2(from, 0), new Vector2(to, 1), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-6, 0));
            UIFactory.Button(rt, skin, label, 13, onClick);
        }
    }
}
