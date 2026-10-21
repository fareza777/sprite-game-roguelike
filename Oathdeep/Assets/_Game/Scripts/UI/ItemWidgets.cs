using System;
using Oathdeep.Audio;
using Oathdeep.Core;
using Oathdeep.Loot;
using UnityEngine;
using UnityEngine.UI;

namespace Oathdeep.UI
{
    /// <summary>Shared item visuals: a framed icon cell and a fixed-cell grid row for pages.</summary>
    public static class ItemWidgets
    {
        /// <summary>A square button showing an item icon in a frame tinted by rarity. Null sprite = empty slot.</summary>
        public static RectTransform Cell(RectTransform parent, UISkin skin, Sprite icon, Color frameColor, bool selected, Action onClick,
                                         bool silhouette = false, string caption = null)
        {
            var rt = UIFactory.Rect("Cell", parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var frame = UIFactory.Image(rt, skin.panel, selected ? skin.textColor : frameColor, true, true);
            if (onClick != null)
            {
                var b = rt.gameObject.AddComponent<Button>();
                b.targetGraphic = frame;
                b.onClick.AddListener(() => { AudioManager.Play(Sfx.UiClick); Haptics.Pulse(6); onClick(); });
            }
            if (icon != null)
            {
                var ir = UIFactory.Stretch("Icon", rt);
                ir.offsetMin = new Vector2(5, caption != null ? 12 : 5);
                ir.offsetMax = new Vector2(-5, -5);
                var img = UIFactory.Image(ir, icon);
                if (silhouette) img.color = new Color(0f, 0f, 0f, 0.55f);
            }
            if (caption != null)
            {
                var cr = UIFactory.Rect("Caption", rt, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 3), new Vector2(0, 10));
                UIFactory.Text(cr, skin, caption, 8, TMPro.TextAlignmentOptions.Center, skin.mutedColor);
            }
            return rt;
        }

        /// <summary>A row of the page holding square cells laid out left to right.</summary>
        public static RectTransform GridRow(PageView page, string name, int columns, float cellSize, float spacing = 6f)
        {
            var row = page.Row(name, cellSize);
            var grid = row.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(cellSize, cellSize);
            grid.spacing = new Vector2(spacing, spacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.childAlignment = TextAnchor.UpperCenter;
            return row;
        }

        public static string Colored(ItemInstance item) => $"<color={RarityInfo.Hex(item.Rarity)}>{item.Name}</color>";

        public static string SlotName(EquipSlot slot) => slot.ToString().ToUpperInvariant();
    }
}
