using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Oathdeep.Dungeon
{
    /// <summary>
    /// Dresses a room so it reads as a place: torches and banners on wall faces, decals, cobwebs,
    /// breakable clusters, statues, braziers, chests and spike traps. Pure placement rules, seeded.
    /// </summary>
    public sealed class DungeonDecorator
    {
        const float DecalChance = 0.08f;
        const int TorchSpacing = 5;
        const float SmallChestChance = 0.3f;
        const float SpikeRoomChance = 0.35f;

        readonly PropSet props;
        readonly Tilemap decals;
        readonly Transform root;
        readonly System.Random rng;
        DungeonGeometry currentGeo;

        public DungeonDecorator(PropSet props, Tilemap decals, Transform root, System.Random rng)
        {
            this.props = props;
            this.decals = decals;
            this.root = root;
            this.rng = rng;
        }

        public void Decorate(RoomGeometry room, DungeonGeometry geo, FloorConfig config, RoomController controller)
        {
            if (props == null) return;
            currentGeo = geo;
            var occupied = new HashSet<Vector2Int>();
            ReserveLanes(room, occupied);

            WallMounts(room, geo);
            Decals(room, geo, config.Biome);

            if (RoomTypes.IsEncounter(room.Node.Type))
            {
                DecorateEncounter(room, geo, config, controller, occupied);
                return;
            }

            switch (room.Node.Type)
            {
                case RoomType.Start:
                    Breakables(room, geo, occupied, 1);
                    break;
                case RoomType.Treasure:
                    Place(props.bigChest != null ? props.bigChest.gameObject : null, room.Centre + Vector2Int.up * 2, occupied);
                    Corners(room, props.candles, occupied, 1);
                    Breakables(room, geo, occupied, 1);
                    break;
                case RoomType.Boss:
                    Corners(room, props.brazier, occupied, 3);
                    Statues(room, occupied);
                    break;
                default:
                    Breakables(room, geo, occupied, rng.Next(2, 5));
                    if (room.Shape == RoomShape.GreatHall || room.Shape == RoomShape.Nave) Statues(room, occupied);
                    if (rng.NextDouble() < 0.4) Corners(room, props.brazier, occupied, 2);
                    if (rng.NextDouble() < SmallChestChance && props.chest != null) PlaceChest(room, occupied, config);
                    if (rng.NextDouble() < SpikeRoomChance) SpikeField(room, occupied);
                    break;
            }
        }

        /// <summary>Keep the cross through the room centre walkable (doors connect there).</summary>
        static void ReserveLanes(RoomGeometry room, HashSet<Vector2Int> occupied)
        {
            var r = room.Interior;
            var c = room.Centre;
            for (int x = r.xMin; x < r.xMax; x++)
                for (int dy = -2; dy <= 2; dy++) occupied.Add(new Vector2Int(x, c.y + dy));
            for (int y = r.yMin; y < r.yMax; y++)
                for (int dx = -2; dx <= 2; dx++) occupied.Add(new Vector2Int(c.x + dx, y));
        }

        void WallMounts(RoomGeometry room, DungeonGeometry geo)
        {
            var r = room.Interior;
            int y = r.yMax; // the face row directly above the interior
            int offset = rng.Next(1, 4);
            for (int x = r.xMin + offset; x < r.xMax - 1; x++)
            {
                var face = new Vector2Int(x, y);
                if (!geo.IsWall(face) || !geo.IsFloor(face + Vector2Int.down)) continue;
                int slot = (x - r.xMin - offset) % TorchSpacing;
                var pos = new Vector3(x + 0.5f, y + 0.55f, 0f);
                if (slot == 0 && props.torch != null) Spawn(props.torch, pos);
                else if (slot == 2 && props.wallDecor != null && props.wallDecor.Length > 0 && rng.NextDouble() < 0.6)
                    Spawn(props.wallDecor[rng.Next(props.wallDecor.Length)], pos);
            }

            // Torches on some pillar faces light the middle of big rooms.
            foreach (var p in room.Pillars)
                if (props.torch != null && rng.NextDouble() < 0.3)
                    Spawn(props.torch, new Vector3(p.x + 1f, p.y + 0.55f, 0f));
        }

        void Decals(RoomGeometry room, DungeonGeometry geo, BiomeTiles biome)
        {
            var r = room.Interior;
            var floorDecals = biome != null && biome.Decals != null && biome.Decals.Length > 0 ? biome.Decals : props.decals;
            var corners = biome != null && biome.CornerDecals != null && biome.CornerDecals.Length > 0 ? biome.CornerDecals : props.cornerDecals;
            if (floorDecals != null && floorDecals.Length > 0)
                for (int x = r.xMin; x < r.xMax; x++)
                    for (int y = r.yMin; y < r.yMax; y++)
                    {
                        var c = new Vector2Int(x, y);
                        if (geo.IsFloor(c) && rng.NextDouble() < DecalChance)
                            decals.SetTile(new Vector3Int(x, y, 0), floorDecals[rng.Next(floorDecals.Length)]);
                    }

            if (corners != null && corners.Length > 0)
            {
                decals.SetTile(new Vector3Int(r.xMin, r.yMax - 1, 0), corners[0]);
                decals.SetTile(new Vector3Int(r.xMax - 1, r.yMax - 1, 0), corners[corners.Length > 1 ? 1 : 0]);
            }
        }

        static readonly EncounterKind[] Events = { EncounterKind.DiceAltar, EncounterKind.SunShard, EncounterKind.WeepingStatue, EncounterKind.CursedChest };

        /// <summary>Peaceful rooms: the encounter in the upper middle, dressed with light and a little clutter.</summary>
        void DecorateEncounter(RoomGeometry room, DungeonGeometry geo, FloorConfig config, RoomController controller, HashSet<Vector2Int> occupied)
        {
            EncounterKind kind = room.Node.Type switch
            {
                RoomType.Merchant => EncounterKind.Merchant,
                RoomType.Shrine => EncounterKind.Shrine,
                RoomType.Pilgrim => EncounterKind.Pilgrim,
                RoomType.Rest => EncounterKind.Rest,
                _ => Events[rng.Next(Events.Length)],
            };
            var prefab = kind switch
            {
                EncounterKind.Merchant => props.merchant,
                EncounterKind.Shrine => props.shrine,
                EncounterKind.Pilgrim => props.pilgrim,
                EncounterKind.Rest => props.campfire,
                EncounterKind.DiceAltar => props.diceAltar,
                EncounterKind.SunShard => props.sunShard,
                EncounterKind.WeepingStatue => props.weepingStatue,
                _ => props.cursedChest,
            };
            if (prefab != null)
            {
                // Off the centre cross so it never blocks the way through the room.
                var cell = room.Centre + new Vector2Int(4, 3);
                var go = Place(prefab.gameObject, cell, occupied);
                if (go != null) go.GetComponent<RoomEncounter>().Init(kind, config.FloorNumber, rng, controller);
            }

            switch (room.Node.Type)
            {
                case RoomType.Shrine:
                    Corners(room, props.brazier, occupied, 2);
                    break;
                case RoomType.Merchant:
                    Corners(room, props.candles, occupied, 1);
                    Breakables(room, geo, occupied, 2);
                    break;
                case RoomType.Rest:
                    Breakables(room, geo, occupied, 1);
                    break;
                default:
                    Corners(room, props.candles, occupied, 2);
                    Breakables(room, geo, occupied, 1);
                    break;
            }
        }

        void Breakables(RoomGeometry room, DungeonGeometry geo, HashSet<Vector2Int> occupied, int clusters)
        {
            if (props.breakables == null || props.breakables.Length == 0) return;
            var r = room.Interior;
            for (int i = 0; i < clusters; i++)
            {
                // Anchor against a wall: pick a side, then a free cell along it.
                Vector2Int anchor;
                int side = rng.Next(4);
                anchor = side switch
                {
                    0 => new Vector2Int(rng.Next(r.xMin, r.xMax), r.yMax - 1),
                    1 => new Vector2Int(rng.Next(r.xMin, r.xMax), r.yMin),
                    2 => new Vector2Int(r.xMin, rng.Next(r.yMin, r.yMax)),
                    _ => new Vector2Int(r.xMax - 1, rng.Next(r.yMin, r.yMax)),
                };
                int count = rng.Next(2, 5);
                for (int k = 0; k < count; k++)
                {
                    var cell = anchor + new Vector2Int(rng.Next(-1, 2), rng.Next(-1, 2));
                    if (!r.Contains(cell) || !geo.IsFloor(cell)) continue;
                    Place(props.breakables[rng.Next(props.breakables.Length)], cell, occupied);
                }
            }
        }

        void Corners(RoomGeometry room, GameObject prefab, HashSet<Vector2Int> occupied, int inset)
        {
            if (prefab == null) return;
            var r = room.Interior;
            Place(prefab, new Vector2Int(r.xMin + inset, r.yMin + inset), occupied);
            Place(prefab, new Vector2Int(r.xMax - 1 - inset, r.yMin + inset), occupied);
            Place(prefab, new Vector2Int(r.xMin + inset, r.yMax - 1 - inset), occupied);
            Place(prefab, new Vector2Int(r.xMax - 1 - inset, r.yMax - 1 - inset), occupied);
        }

        void Statues(RoomGeometry room, HashSet<Vector2Int> occupied)
        {
            if (props.statue == null) return;
            var c = room.Centre;
            int top = room.Interior.yMax - 2;
            Place(props.statue, new Vector2Int(c.x - 4, top), occupied);
            Place(props.statue, new Vector2Int(c.x + 4, top), occupied);
        }

        void PlaceChest(RoomGeometry room, HashSet<Vector2Int> occupied, FloorConfig config)
        {
            var r = room.Interior;
            var cell = new Vector2Int(rng.NextDouble() < 0.5 ? r.xMin + 1 : r.xMax - 2, r.yMax - 2);
            var go = Place(props.chest.gameObject, cell, occupied);
            if (go == null) return;
            var item = rng.NextDouble() < 0.4 ? Loot.ItemGenerator.Roll(rng, config.FloorNumber, 0.5f) : null;
            go.GetComponent<Chest>().Setup(item, new Vector2Int(5, 12) * Mathf.Max(1, config.FloorNumber / 2 + 1));
        }

        void SpikeField(RoomGeometry room, HashSet<Vector2Int> occupied)
        {
            if (props.spikeTrap == null) return;
            var r = room.Interior;
            var c = room.Centre;
            int qx = rng.NextDouble() < 0.5 ? r.xMin + 2 : c.x + 3;
            int qy = rng.NextDouble() < 0.5 ? r.yMin + 2 : c.y + 3;
            for (int x = qx; x < qx + 3; x++)
                for (int y = qy; y < qy + 2; y++)
                    Place(props.spikeTrap.gameObject, new Vector2Int(x, y), occupied);
        }

        GameObject Place(GameObject prefab, Vector2Int cell, HashSet<Vector2Int> occupied)
        {
            if (prefab == null || occupied.Contains(cell)) return null;
            if (currentGeo != null && !currentGeo.IsFloor(cell)) return null;
            occupied.Add(cell);
            return Spawn(prefab, new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f));
        }

        GameObject Spawn(GameObject prefab, Vector3 position) => Object.Instantiate(prefab, position, Quaternion.identity, root);
    }
}
