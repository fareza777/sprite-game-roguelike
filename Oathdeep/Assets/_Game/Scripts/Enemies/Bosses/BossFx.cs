using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>The art the boss mechanics lay into an arena (from ArtSource/boss_fx.aseprite).</summary>
    [CreateAssetMenu(menuName = "Oathdeep/Boss Fx", fileName = "BossFx")]
    public sealed class BossFx : ScriptableObject
    {
        [Header("Anchors (break them to drop a boss's shield)")]
        public Sprite boneTotem;
        public Sprite rootCore;
        public Sprite sporePod;
        public Sprite duskAnchor;

        [Header("Ground hazards (tile or scale)")]
        public Sprite sporeCloud;
        public Sprite water;
        public Sprite ice;
        public Sprite quicksand;
        public Sprite lava;
        public Sprite rootSnare;
        public Sprite timeBubble;

        [Header("Effects")]
        public Sprite steam;
        public Sprite clockHand;
        public Sprite sandFog;
    }
}
