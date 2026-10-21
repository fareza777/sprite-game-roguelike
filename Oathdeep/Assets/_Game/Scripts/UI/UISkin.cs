using TMPro;
using UnityEngine;

namespace Oathdeep.UI
{
    /// <summary>All UI art in one place so screens are built from data and reskinnable.</summary>
    [CreateAssetMenu(menuName = "Oathdeep/UI Skin", fileName = "UISkin")]
    public sealed class UISkin : ScriptableObject
    {
        public TMP_FontAsset font;
        public Sprite panel;            // 9-slice dark stone panel
        public Sprite button;           // 9-slice button
        public Sprite buttonPressed;
        public Sprite barFrame;         // 9-slice bar frame
        public Sprite barFillHealth;
        public Sprite barFillBoss;
        public Sprite iconGold;
        public Sprite iconPotion;
        public Sprite iconAttack;
        public Sprite iconDodge;
        public Sprite iconSkill;
        public Sprite iconPause;
        public Sprite actionRing;       // round button background
        public Sprite joystickBase;
        public Sprite joystickKnob;
        public Sprite logo;
        public Sprite white;            // 1px white for fills, minimap, vignette
        [Header("Front end")]
        public Sprite menuBackground;
        public Sprite appIcon;
        public Sprite iconSettings;
        public Sprite iconInfo;
        public Sprite iconShare;
        public Sprite iconRate;
        public Sprite iconCodex;
        public Sprite iconQuest;
        public Sprite iconHeroes;
        public Sprite iconBack;
        public Sprite iconClose;
        public Sprite iconBag;
        public Color textColor = new Color32(0xEB, 0xED, 0xE9, 0xFF);
        public Color accentColor = new Color32(0xDE, 0x9E, 0x41, 0xFF);
        public Color dangerColor = new Color32(0xCF, 0x57, 0x3C, 0xFF);
        public Color critColor = new Color32(0xE8, 0xC1, 0x70, 0xFF);
        public Color mutedColor = new Color32(0x81, 0x97, 0x96, 0xFF);
    }
}
