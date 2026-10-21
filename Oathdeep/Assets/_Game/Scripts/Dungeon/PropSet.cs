using UnityEngine;
using UnityEngine.Tilemaps;

namespace Oathdeep.Dungeon
{
    /// <summary>Decoration + interactable prefabs used to dress a biome's rooms.</summary>
    [CreateAssetMenu(menuName = "Oathdeep/Prop Set", fileName = "Props_")]
    public sealed class PropSet : ScriptableObject
    {
        [Header("Wall mounted")]
        public GameObject torch;
        public GameObject[] wallDecor;        // banners, chains, skull niches
        [Header("Floor")]
        public TileBase[] decals;             // bones, cracks, blood, moss, puddles
        public TileBase[] cornerDecals;       // cobwebs
        public GameObject[] breakables;       // pot, barrel, crate
        public GameObject statue;
        public GameObject brazier;
        public GameObject candles;
        [Header("Interactables")]
        public Chest chest;
        public Chest bigChest;
        public SpikeTrap spikeTrap;
        public Door door;
        public GameObject exitPortal;
        public GameObject spawnMarker;
    }
}
