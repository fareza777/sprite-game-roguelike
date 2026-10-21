using System;
using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>The story script plus the panels and narration clips that bring it to life.</summary>
    [CreateAssetMenu(menuName = "Oathdeep/Story Assets", fileName = "StoryAssets")]
    public sealed class StoryAssets : ScriptableObject
    {
        [Serializable]
        public struct Panel { public string Id; public Sprite Sprite; }

        [Serializable]
        public struct Voice { public string Id; public AudioClip Clip; }

        public TextAsset script;
        public Panel[] panels;
        public Voice[] voices;

        // Never serialized: Unity keeps private fields across domain reloads, which once preserved an empty parse.
        [NonSerialized] StoryData parsed;
        public StoryData Data => parsed ??= StoryData.Parse(script != null ? script.text : null);

        void OnEnable() => parsed = null;

        public Sprite PanelFor(string id)
        {
            foreach (var p in panels) if (p.Id == id) return p.Sprite;
            return null;
        }

        /// <summary>Narration for a line id (pro_1, end_2, floor_3...).</summary>
        public AudioClip VoiceFor(string id)
        {
            foreach (var v in voices) if (v.Id == id) return v.Clip;
            return null;
        }
    }
}
