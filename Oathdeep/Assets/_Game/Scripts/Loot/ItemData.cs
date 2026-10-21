using Oathdeep.Data;
using UnityEngine;

namespace Oathdeep.Loot
{
    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

    public enum EquipSlot { Weapon, Armor, Accessory }

    [CreateAssetMenu(menuName = "Oathdeep/Item", fileName = "Item_")]
    public sealed class ItemData : ScriptableObject
    {
        [SerializeField] string id;
        [SerializeField] string displayName;
        [SerializeField, TextArea] string description;
        [SerializeField] Sprite icon;
        [SerializeField] Rarity rarity;
        [SerializeField] EquipSlot slot;
        [SerializeField] StatBlock bonus;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public Rarity Rarity => rarity;
        public EquipSlot Slot => slot;
        public StatBlock Bonus => bonus;

#if UNITY_EDITOR
        public void EditorInit(string newId, string name, string desc, Sprite sprite, Rarity r, EquipSlot s, StatBlock b)
        {
            id = newId; displayName = name; description = desc; icon = sprite; rarity = r; slot = s; bonus = b;
        }
#endif
    }

    public static class RarityColors
    {
        public static Color Of(Rarity r) => r switch
        {
            Rarity.Common => new Color32(0xC9, 0xC9, 0xC9, 0xFF),
            Rarity.Uncommon => new Color32(0x5F, 0xD3, 0x6A, 0xFF),
            Rarity.Rare => new Color32(0x4A, 0x9B, 0xFF, 0xFF),
            Rarity.Epic => new Color32(0xB5, 0x5C, 0xFF, 0xFF),
            Rarity.Legendary => new Color32(0xFF, 0xB0, 0x2E, 0xFF),
            _ => Color.white,
        };
    }
}
