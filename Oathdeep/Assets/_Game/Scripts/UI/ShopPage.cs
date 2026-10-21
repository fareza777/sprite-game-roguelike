using System;
using System.Collections.Generic;
using Oathdeep.Audio;
using Oathdeep.Combat;
using Oathdeep.Core;
using Oathdeep.Dungeon;
using Oathdeep.Loot;
using Oathdeep.Player;
using TMPro;
using UnityEngine;

namespace Oathdeep.UI
{
    /// <summary>Old Tallow's wares: rolled items and healing draughts for gold. Bought items go to the bag.</summary>
    public sealed class ShopPage : MonoBehaviour
    {
        const int Columns = 3;
        const float CellSize = 76f;
        const float DraughtHeal = 0.4f;

        PageView page;
        UISkin skin;
        PlayerInventory inventory;
        List<ShopOffer> stock;
        ShopOffer selected;

        public static ShopPage Create(RectTransform parent, UISkin skin, Action onClosed)
        {
            var page = PageView.Create("Shop", parent, skin, "SHOP", onClosed);
            var view = page.gameObject.AddComponent<ShopPage>();
            view.page = page;
            view.skin = skin;
            return view;
        }

        public void Show(string title, PlayerInventory inv, List<ShopOffer> offers)
        {
            page.Title.text = title;
            inventory = inv;
            stock = offers;
            selected = null;
            page.Show();
            Rebuild();
        }

        public void Hide() => page.Hide();

        void Rebuild()
        {
            page.Clear();
            page.Paragraph($"<color=#de9e41>{inventory.Gold} gold</color>    bag {inventory.Bag.Count}/{PlayerInventory.BagSize}", 13, skin.textColor, TextAlignmentOptions.Center);

            for (int start = 0; start < stock.Count; start += Columns)
            {
                var row = ItemWidgets.GridRow(page, "Wares" + start, Columns, CellSize, 8f);
                for (int i = start; i < Mathf.Min(start + Columns, stock.Count); i++)
                {
                    var offer = stock[i];
                    var icon = offer.Item != null ? ItemIcons.For(offer.Item) : skin.iconPotion;
                    var color = offer.Item != null ? RarityInfo.ColorOf(offer.Item.Rarity) : new Color(0.8f, 0.3f, 0.3f);
                    ItemWidgets.Cell(row, skin, offer.Sold ? null : icon, offer.Sold ? new Color(1, 1, 1, 0.15f) : color, offer == selected,
                                     offer.Sold ? null : (Action)(() => { selected = offer; Rebuild(); }), false,
                                     offer.Sold ? "SOLD" : offer.Price + "g");
                }
            }

            if (selected == null || selected.Sold)
            {
                page.Paragraph("\"Take your time. Nobody down here is in a hurry. Except the things chasing you.\"", 11, skin.mutedColor);
                return;
            }

            if (selected.Item != null)
            {
                var item = selected.Item;
                page.Heading(item.Rarity.ToString().ToUpperInvariant() + "  " + ItemWidgets.SlotName(item.Slot) + "  LV " + item.Level);
                page.Paragraph(ItemWidgets.Colored(item), 15);
                page.Paragraph(item.Tooltip(), 11);
                var worn = inventory.Loadout.Get(item.Slot);
                int delta = item.Power - (worn != null ? worn.Power : 0);
                page.Paragraph((worn != null ? "vs " + worn.Name + ": " : "Empty slot: ") +
                               (delta >= 0 ? $"<color=#75a743>+{delta} power</color>" : $"<color=#cf573c>{delta} power</color>"), 10, skin.mutedColor);
            }
            else
            {
                page.Heading("CONSUMABLE");
                page.Paragraph("Healing Draught", 15);
                page.Paragraph($"Drink at once: restores {DraughtHeal * 100f:0}% of your health.", 11);
            }

            bool affordable = inventory.Gold >= selected.Price;
            var buy = page.Row("Buy", 42);
            var button = UIFactory.Button(buy, skin, affordable ? $"BUY  {selected.Price}g" : $"NEED {selected.Price}g", 14, Buy);
            button.interactable = affordable;
        }

        void Buy()
        {
            if (selected == null || selected.Sold) return;
            if (selected.Item != null && !inventory.CanTake(selected.Item))
            {
                GameEvents.RaiseToast("Bag full - salvage something first");
                return;
            }
            if (!inventory.SpendGold(selected.Price)) return;
            if (selected.Item != null) inventory.Take(selected.Item);
            else
            {
                var health = inventory.GetComponent<Health>();
                health.Heal(Mathf.CeilToInt(health.Max * DraughtHeal));
            }
            selected.Sold = true;
            selected = null;
            AudioManager.Play(Sfx.Coin);
            Rebuild();
        }
    }
}
