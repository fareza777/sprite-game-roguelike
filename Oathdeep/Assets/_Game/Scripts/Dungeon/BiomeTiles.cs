using UnityEngine;
using UnityEngine.Tilemaps;

namespace Oathdeep.Dungeon
{
    /// <summary>Visual set for one biome. Wall caps are indexed by WallAutotile mask (0..15).</summary>
    [CreateAssetMenu(menuName = "Oathdeep/Biome Tiles", fileName = "Biome_")]
    public sealed class BiomeTiles : ScriptableObject
    {
        [SerializeField] string displayName = "Sunken Crypt";
        [SerializeField] TileBase[] floors;                       // common slabs, picked often
        [SerializeField] TileBase[] rareFloors;                   // drains, runes, broken slabs
        [SerializeField, Range(0f, 0.2f)] float rareFloorChance = 0.04f;
        [SerializeField] TileBase[] wallFaces;
        [SerializeField] TileBase[] wallCaps = new TileBase[16];
        [SerializeField] TileBase shadowTop;                      // under a wall face
        [SerializeField] TileBase shadowLeft;                     // right of a west wall
        [SerializeField] TileBase shadowCorner;
        [SerializeField] TileBase[] decals;                       // bones, cracks, moss... plus this biome's own decals
        [SerializeField] TileBase[] cornerDecals;                 // cobwebs
        [SerializeField] Color ambientLight = new Color(0.45f, 0.5f, 0.7f);
        [SerializeField, Range(0f, 1f)] float ambientIntensity = 0.35f;
        [SerializeField] Audio.MusicTrack music = Audio.MusicTrack.Crypt;
        [SerializeField] Audio.MusicTrack bossMusic = Audio.MusicTrack.Boss;
        [SerializeField] Sprite[] ambient;                        // the floor's drifting motes (two frames)

        public string DisplayName => displayName;
        public TileBase[] Decals => decals;
        public TileBase[] CornerDecals => cornerDecals;
        public Color AmbientLight => ambientLight;
        public float AmbientIntensity => ambientIntensity;
        public Audio.MusicTrack Music => music;
        public Audio.MusicTrack BossMusic => bossMusic;
        public Sprite[] Ambient => ambient;

        public TileBase Floor(System.Random rng)
        {
            if (rareFloors != null && rareFloors.Length > 0 && rng.NextDouble() < rareFloorChance)
                return rareFloors[rng.Next(rareFloors.Length)];
            if (floors == null || floors.Length == 0) return null;
            // First slab is the calm base; variants sprinkle in.
            return rng.NextDouble() < 0.5 ? floors[0] : floors[rng.Next(floors.Length)];
        }

        public TileBase WallFace(System.Random rng)
        {
            if (wallFaces == null || wallFaces.Length == 0) return null;
            return rng.NextDouble() < 0.7 ? wallFaces[0] : wallFaces[rng.Next(wallFaces.Length)];
        }

        public TileBase WallCap(int mask) => wallCaps != null && mask >= 0 && mask < wallCaps.Length ? wallCaps[mask] : null;

        /// <summary>Contact shadow for a floor cell given whether walls sit above and/or to its left.</summary>
        public TileBase Shadow(bool wallAbove, bool wallLeft)
        {
            if (wallAbove && wallLeft) return shadowCorner;
            if (wallAbove) return shadowTop;
            if (wallLeft) return shadowLeft;
            return null;
        }

#if UNITY_EDITOR
        public void EditorSetAmbient(Sprite[] motes) => ambient = motes;

        public void EditorInit(TileBase[] common, TileBase[] rare, TileBase[] faces, TileBase[] caps, TileBase top, TileBase left, TileBase corner,
                               TileBase[] floorDecals, TileBase[] corners)
        {
            floors = common; rareFloors = rare; wallFaces = faces; wallCaps = caps;
            shadowTop = top; shadowLeft = left; shadowCorner = corner;
            decals = floorDecals; cornerDecals = corners;
        }
#endif
    }
}
