using Oathdeep.Player;
using UnityEngine;

namespace Oathdeep.Data
{
    /// <summary>One playable hero: identity and lore for the hero page, base stats, cooldowns and the prefab to spawn.
    /// How the hero fights lives in <see cref="HeroMoves"/>, keyed by <see cref="Id"/>.</summary>
    [CreateAssetMenu(menuName = "Oathdeep/Hero", fileName = "Hero_")]
    public sealed class HeroData : ScriptableObject
    {
        [SerializeField] HeroId id;
        [SerializeField] string displayName = "Knight";
        [SerializeField] string title;
        [SerializeField, TextArea] string description;
        [SerializeField] StatBlock baseStats = new StatBlock(100, 12, 4, 0.08f, 1.75f, 4.2f);
        [SerializeField] float skillCooldown = 5f;
        [SerializeField] float dodgeCooldown = 0.45f;
        [SerializeField] string skillName;
        [SerializeField, TextArea] string skillDescription;
        [SerializeField] Sprite skillIcon;
        [SerializeField] Sprite[] idleFrames;
        [SerializeField] GameObject playerPrefab;

        public HeroId Id => id;
        public string DisplayName => displayName;
        public string Title => title;
        public string Description => description;
        public StatBlock BaseStats => baseStats;
        public float SkillCooldown => skillCooldown;
        public float DodgeCooldown => dodgeCooldown;
        public string SkillName => skillName;
        public string SkillDescription => skillDescription;
        public Sprite SkillIcon => skillIcon;
        public Sprite[] IdleFrames => idleFrames;
        public Sprite Portrait => idleFrames != null && idleFrames.Length > 0 ? idleFrames[0] : null;
        public GameObject PlayerPrefab => playerPrefab;
        public HeroMoves Moves => HeroMoves.For(id);
    }
}
