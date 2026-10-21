using System;
using Oathdeep.Enemies;
using Oathdeep.Loot;
using UnityEngine;

namespace Oathdeep.Dungeon
{
    [Serializable]
    public struct WeightedEnemy
    {
        public EnemyBase Prefab;
        [Min(0f)] public float Weight;
    }

    /// <summary>Everything that defines one floor: biome, size, enemy mix, waves, boss and rewards.</summary>
    [CreateAssetMenu(menuName = "Oathdeep/Floor Config", fileName = "Floor_")]
    public sealed class FloorConfig : ScriptableObject
    {
        [SerializeField] int floorNumber = 1;
        [SerializeField] BiomeTiles biome;
        [SerializeField, Range(3, 20)] int roomCount = 14;
        [SerializeField] WeightedEnemy[] enemies;
        [SerializeField] Vector2Int wavesPerRoom = new Vector2Int(2, 3);
        [SerializeField] Vector2Int enemiesPerWave = new Vector2Int(4, 7);
        [SerializeField, Range(0f, 1f)] float eliteChance = 0.12f;
        [SerializeField] float statScale = 1f;
        [SerializeField] EnemyBase boss;
        [SerializeField] ItemData[] treasureLoot;

        public int FloorNumber => floorNumber;
        public BiomeTiles Biome => biome;
        public int RoomCount => roomCount;
        public Vector2Int WavesPerRoom => wavesPerRoom;
        public Vector2Int EnemiesPerWave => enemiesPerWave;
        public float StatScale => statScale;
        public float EliteChance => eliteChance;
        public EnemyBase Boss => boss;
        public ItemData[] TreasureLoot => treasureLoot;

        public EnemyBase PickEnemy(System.Random rng)
        {
            if (enemies == null || enemies.Length == 0) return null;
            float total = 0f;
            foreach (var e in enemies) total += e.Weight;
            double roll = rng.NextDouble() * total;
            foreach (var e in enemies)
            {
                roll -= e.Weight;
                if (roll <= 0) return e.Prefab;
            }
            return enemies[enemies.Length - 1].Prefab;
        }

#if UNITY_EDITOR
        public void EditorInit(BiomeTiles b, WeightedEnemy[] pool, EnemyBase bossPrefab, ItemData[] loot)
        {
            biome = b; enemies = pool; boss = bossPrefab; treasureLoot = loot;
        }
#endif
    }
}
