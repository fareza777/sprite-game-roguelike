using System.Collections.Generic;
using Oathdeep.Dungeon;
using UnityEngine;
using UnityEngine.UI;

namespace Oathdeep.UI
{
    /// <summary>Room-graph minimap. Rooms reveal when entered or adjacent to a visited room; current room pulses.</summary>
    public sealed class MinimapView : MonoBehaviour
    {
        const float Cell = 13f;
        const float RoomSize = 10f;

        static readonly Color Unknown = new Color(1f, 1f, 1f, 0.12f);
        static readonly Color Visited = new Color32(0x81, 0x97, 0x96, 0xE6);
        static readonly Color Current = new Color32(0xEB, 0xED, 0xE9, 0xFF);
        static readonly Color BossColor = new Color32(0xCF, 0x57, 0x3C, 0xFF);
        static readonly Color TreasureColor = new Color32(0xDE, 0x9E, 0x41, 0xFF);

        UISkin skin;
        RectTransform content;
        readonly Dictionary<Vector2Int, Image> rooms = new Dictionary<Vector2Int, Image>();
        readonly Dictionary<Vector2Int, RoomType> types = new Dictionary<Vector2Int, RoomType>();
        readonly HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        readonly HashSet<Vector2Int> known = new HashSet<Vector2Int>();
        DungeonLayoutView layout;
        Vector2Int current;

        public void Build(UISkin uiSkin)
        {
            skin = uiSkin;
            var rt = (RectTransform)transform;
            var bg = gameObject.AddComponent<Image>();
            bg.sprite = skin.panel;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(1, 1, 1, 0.75f);
            bg.raycastTarget = false;
            gameObject.AddComponent<RectMask2D>();
            content = UIFactory.Rect("Content", rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }

        public void Show(IReadOnlyList<RoomGeometry> geometryRooms)
        {
            foreach (Transform child in content) Destroy(child.gameObject);
            rooms.Clear(); types.Clear(); visited.Clear(); known.Clear();
            layout = new DungeonLayoutView(geometryRooms);

            foreach (var room in geometryRooms)
            {
                var g = room.Node.GridPosition;
                types[g] = room.Node.Type;
                foreach (var n in room.Node.Connections)
                {
                    if (n.x < g.x || n.y < g.y) continue; // draw each corridor once
                    var mid = ((Vector2)(g + n)) * 0.5f * Cell;
                    bool horizontal = n.y == g.y;
                    var link = UIFactory.Rect("Link", content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), mid,
                        horizontal ? new Vector2(Cell - RoomSize, 2) : new Vector2(2, Cell - RoomSize));
                    UIFactory.Image(link, skin.white, new Color(1, 1, 1, 0.25f));
                }
                var rect = UIFactory.Rect($"Room {g}", content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), (Vector2)g * Cell, new Vector2(RoomSize, RoomSize));
                var img = UIFactory.Image(rect, skin.white, Unknown);
                img.preserveAspect = false;
                img.enabled = false;
                rooms[g] = img;
            }
            Enter(geometryRooms[0].Node.GridPosition);
        }

        public void Enter(Vector2Int cell)
        {
            current = cell;
            visited.Add(cell);
            known.Add(cell);
            foreach (var n in layout.Neighbours(cell)) known.Add(n);
            content.anchoredPosition = -(Vector2)cell * Cell;
            Refresh();
        }

        void Refresh()
        {
            foreach (var kv in rooms)
            {
                var img = kv.Value;
                img.enabled = known.Contains(kv.Key);
                if (!img.enabled) continue;
                var type = types[kv.Key];
                Color c = visited.Contains(kv.Key) ? Visited : Unknown;
                if (type == RoomType.Boss) c = BossColor;
                else if (type == RoomType.Treasure && known.Contains(kv.Key)) c = TreasureColor;
                if (kv.Key == current) c = Current;
                img.color = c;
            }
        }

        void Update()
        {
            if (rooms.TryGetValue(current, out var img) && img != null)
            {
                float s = 1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.12f;
                img.rectTransform.localScale = new Vector3(s, s, 1f);
            }
        }

        /// <summary>Tiny adjacency helper so the minimap does not depend on the full layout object.</summary>
        sealed class DungeonLayoutView
        {
            readonly Dictionary<Vector2Int, IReadOnlyList<Vector2Int>> links = new Dictionary<Vector2Int, IReadOnlyList<Vector2Int>>();
            public DungeonLayoutView(IReadOnlyList<RoomGeometry> rooms) { foreach (var r in rooms) links[r.Node.GridPosition] = r.Node.Connections; }
            public IEnumerable<Vector2Int> Neighbours(Vector2Int c) => links.TryGetValue(c, out var l) ? l : (IEnumerable<Vector2Int>)System.Array.Empty<Vector2Int>();
        }
    }
}
