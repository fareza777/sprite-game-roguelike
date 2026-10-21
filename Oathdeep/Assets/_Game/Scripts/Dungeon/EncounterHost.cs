using System;
using System.Collections.Generic;
using Oathdeep.Loot;
using UnityEngine;

namespace Oathdeep.Dungeon
{
    /// <summary>One button in an encounter dialog.</summary>
    public readonly struct EncounterChoice
    {
        public readonly string Label;
        public readonly bool Enabled;
        public readonly Action Pick;

        public EncounterChoice(string label, Action pick, bool enabled = true)
        {
            Label = label; Pick = pick; Enabled = enabled;
        }
    }

    /// <summary>What the dialog shows: who speaks, what they say, and the answers.</summary>
    public sealed class EncounterDialog
    {
        public string Speaker;
        public Sprite Portrait;
        public string Text;
        public List<EncounterChoice> Choices = new List<EncounterChoice>();
    }

    /// <summary>A shop offer: the item (or null for a potion) and its price.</summary>
    public sealed class ShopOffer
    {
        public ItemInstance Item;
        public int Price;
        public bool Sold;
    }

    /// <summary>
    /// Bridge from dungeon encounters to the UI layer. GameRunner plugs the screens in; encounters only describe
    /// what they want shown, so the dungeon code never depends on UI classes.
    /// </summary>
    public static class EncounterHost
    {
        public static Action<EncounterDialog> ShowDialog;
        public static Action<string, Sprite, List<ShopOffer>> OpenShop;

        public static void Dialog(EncounterDialog dialog)
        {
            if (ShowDialog != null) ShowDialog(dialog);
            else if (dialog.Choices.Count > 0) dialog.Choices[0].Pick?.Invoke();   // headless (tests, no UI): take the first answer
        }
    }
}
