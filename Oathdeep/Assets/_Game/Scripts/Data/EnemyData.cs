using UnityEngine;

namespace Oathdeep.Data
{
    [CreateAssetMenu(menuName = "Oathdeep/Enemy", fileName = "Enemy_")]
    public sealed class EnemyData : ScriptableObject
    {
        [SerializeField] string displayName;
        [SerializeField] StatBlock stats = new StatBlock(30, 6, 0, 0f, 1.5f, 2.2f);
        [SerializeField] float aggroRange = 7f;
        [SerializeField] float attackRange = 1.2f;
        [SerializeField] Vector2Int goldDrop = new Vector2Int(1, 4);
        [SerializeField, Range(0f, 1f)] float itemDropChance = 0.08f;
        [SerializeField, Range(0f, 1f)] float potionDropChance = 0.06f;
        [SerializeField] bool isBoss;

        public string DisplayName => displayName;
        public StatBlock Stats => stats;
        public float AggroRange => aggroRange;
        public float AttackRange => attackRange;
        public Vector2Int GoldDrop => goldDrop;
        public float ItemDropChance => itemDropChance;
        public float PotionDropChance => potionDropChance;
        public bool IsBoss => isBoss;

#if UNITY_EDITOR
        public void EditorInit(string name, StatBlock s, float aggro, float range, Vector2Int gold, float itemChance, float potionChance, bool boss)
        {
            displayName = name; stats = s; aggroRange = aggro; attackRange = range; goldDrop = gold;
            itemDropChance = itemChance; potionDropChance = potionChance; isBoss = boss;
        }
#endif
    }
}
