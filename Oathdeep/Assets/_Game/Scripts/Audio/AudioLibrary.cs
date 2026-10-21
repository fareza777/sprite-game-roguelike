using System;
using UnityEngine;

namespace Oathdeep.Audio
{
    public enum Sfx
    {
        Swing, SwingHeavy, Hit, HitCrit, PlayerHurt, EnemyDeath, SlimeSquish, Dodge, Bash, ArrowShoot, Spike,
        BossSlam, BossCast, BossRoar, Coin, Potion, ItemPickup, ChestOpen, Break, DoorSlam, DoorOpen, Spawn,
        RoomClear, UiClick, UiConfirm,
    }

    public enum MusicTrack { None, Title, Crypt, Boss }

    /// <summary>Every clip the game plays, with per-sound mix settings. One asset, edited in one place.</summary>
    [CreateAssetMenu(menuName = "Oathdeep/Audio Library", fileName = "AudioLibrary")]
    public sealed class AudioLibrary : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public Sfx Id;
            public AudioClip Clip;
            [Range(0f, 1f)] public float Volume;
            [Range(0f, 0.3f)] public float PitchJitter;   // random +/- so repeated hits don't machine-gun
            [Min(0f)] public float MinInterval;            // seconds between plays of the same sound
        }

        public Entry[] sfx;
        public AudioClip musicTitle;
        public AudioClip musicCrypt;
        public AudioClip musicBoss;
        public AudioClip stingVictory;
        public AudioClip stingGameOver;

        public AudioClip Music(MusicTrack track) => track switch
        {
            MusicTrack.Title => musicTitle,
            MusicTrack.Crypt => musicCrypt,
            MusicTrack.Boss => musicBoss,
            _ => null,
        };
    }
}
