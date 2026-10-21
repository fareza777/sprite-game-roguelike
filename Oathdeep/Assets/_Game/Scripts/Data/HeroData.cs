using UnityEngine;

namespace Oathdeep.Data
{
    [CreateAssetMenu(menuName = "Oathdeep/Hero", fileName = "Hero_")]
    public sealed class HeroData : ScriptableObject
    {
        [SerializeField] string displayName = "Knight";
        [SerializeField, TextArea] string description;
        [SerializeField] StatBlock baseStats = new StatBlock(100, 12, 4, 0.08f, 1.75f, 4.2f);
        [SerializeField] float skillCooldown = 5f;
        [SerializeField] float dodgeCooldown = 0.45f;

        public string DisplayName => displayName;
        public string Description => description;
        public StatBlock BaseStats => baseStats;
        public float SkillCooldown => skillCooldown;
        public float DodgeCooldown => dodgeCooldown;
    }
}
