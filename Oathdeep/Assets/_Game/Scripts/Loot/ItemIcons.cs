using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oathdeep.Loot
{
    /// <summary>Icon sprite for each ItemBase.Icon key, sliced from the item sheet.</summary>
    [CreateAssetMenu(menuName = "Oathdeep/Item Icons", fileName = "ItemIcons")]
    public sealed class ItemIcons : ScriptableObject
    {
        [Serializable]
        public struct Entry { public string Key; public Sprite Sprite; }

        [SerializeField] Entry[] entries;
        [SerializeField] Sprite fallback;

        [NonSerialized] Dictionary<string, Sprite> map;
        static ItemIcons active;

        /// <summary>Registers the set used by pickups and item pages for this session.</summary>
        public static void Use(ItemIcons icons) => active = icons;

        public static Sprite For(string key)
        {
            if (active == null) return null;
            active.map ??= BuildMap(active.entries);
            return key != null && active.map.TryGetValue(key, out var s) ? s : active.fallback;
        }

        public static Sprite For(ItemInstance item) => item == null ? null : For(item.Icon);

        static Dictionary<string, Sprite> BuildMap(Entry[] list)
        {
            var d = new Dictionary<string, Sprite>();
            if (list != null) foreach (var e in list) if (!string.IsNullOrEmpty(e.Key)) d[e.Key] = e.Sprite;
            return d;
        }

        void OnEnable() => map = null;

#if UNITY_EDITOR
        public void EditorInit(Entry[] list, Sprite missing) { entries = list; fallback = missing; map = null; }
#endif
    }
}
