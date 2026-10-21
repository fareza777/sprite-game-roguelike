using System.Collections;
using Oathdeep.Combat;
using Oathdeep.Core;
using UnityEngine;

namespace Oathdeep.Audio
{
    /// <summary>
    /// Pooled one-shot SFX plus two-deck music with crossfades and stingers.
    /// Gameplay calls <see cref="Play"/> directly; run-wide reactions are bound to GameEvents in <see cref="BindRunEvents"/>.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        const int Voices = 16;
        const float AudibleRadius = 11f;   // world units from the camera; farther sounds fade out
        const float CrossfadeSeconds = 1.2f;

        static AudioManager instance;

        AudioLibrary library;
        AudioSource[] voices;
        AudioSource[] decks;
        AudioSource stinger;
        AudioSource voice;
        int nextVoice, activeDeck;
        float[] lastPlayed;
        AudioLibrary.Entry?[] byId;
        int lastGold;
        MusicTrack currentTrack;
        Coroutine fadeRoutine;

        public static AudioManager Create(AudioLibrary lib)
        {
            if (instance != null) return instance;
            var go = new GameObject("Audio");
            instance = go.AddComponent<AudioManager>();
            instance.Init(lib);
            return instance;
        }

        void Init(AudioLibrary lib)
        {
            library = lib;
            int count = System.Enum.GetValues(typeof(Sfx)).Length;
            lastPlayed = new float[count];
            byId = new AudioLibrary.Entry?[count];
            if (lib != null && lib.sfx != null)
                foreach (var e in lib.sfx) byId[(int)e.Id] = e;
            voices = new AudioSource[Voices];
            for (int i = 0; i < Voices; i++) voices[i] = NewSource(false);
            decks = new[] { NewSource(true), NewSource(true) };
            stinger = NewSource(false);
            voice = NewSource(false);
        }

        AudioSource NewSource(bool loop)
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = loop;
            s.spatialBlend = 0f;
            return s;
        }

        void OnDestroy() { if (instance == this) instance = null; }

        // ---------------- SFX ----------------

        /// <summary>Plays a sound; `at` (world position) attenuates and pans relative to the camera.</summary>
        public static void Play(Sfx id, Vector3? at = null, float volume = 1f)
        {
            if (instance != null) instance.PlayInternal(id, at, volume);
        }

        void PlayInternal(Sfx id, Vector3? at, float volume)
        {
            int index = (int)id;
            if (byId[index] is not AudioLibrary.Entry entry) return;
            if (entry.Clip == null || Time.unscaledTime - lastPlayed[index] < entry.MinInterval) return;

            float pan = 0f, attenuation = 1f;
            var cam = Camera.main;
            if (at.HasValue && cam != null)
            {
                Vector2 offset = at.Value - cam.transform.position;
                attenuation = Mathf.Clamp01(1.4f - offset.magnitude / AudibleRadius);
                pan = Mathf.Clamp(offset.x / 8f, -0.6f, 0.6f);
            }
            float v = entry.Volume * volume * attenuation * GameSettings.SfxVolume;
            if (v <= 0.01f) return;

            lastPlayed[index] = Time.unscaledTime;
            var src = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            src.pitch = 1f + Random.Range(-entry.PitchJitter, entry.PitchJitter);
            src.panStereo = pan;
            src.PlayOneShot(entry.Clip, v);
        }

        // ---------------- narration ----------------

        /// <summary>Plays a narration line over the music (ducked while it speaks). Returns its length in seconds.</summary>
        public static float PlayVoice(AudioClip clip)
        {
            if (instance == null || clip == null) return 0f;
            instance.voice.Stop();
            instance.voice.clip = clip;
            instance.voice.volume = Mathf.Max(0.0001f, GameSettings.SfxVolume);
            instance.voice.Play();
            return clip.length;
        }

        public static void StopVoice() { if (instance != null) instance.voice.Stop(); }

        void LateUpdate()
        {
            // duck the music under narration
            if (decks == null) return;
            float target = GameSettings.MusicVolume * (voice != null && voice.isPlaying ? 0.45f : 1f);
            var deck = decks[activeDeck];
            if (fadeRoutine == null && deck.isPlaying) deck.volume = Mathf.MoveTowards(deck.volume, target, Time.unscaledDeltaTime * 1.5f);
        }

        // ---------------- music ----------------

        public static void PlayMusic(MusicTrack track) { if (instance != null) instance.PlayMusicInternal(track); }

        /// <summary>The current floor's theme and the boss theme it switches to; set when a floor begins.</summary>
        public static MusicTrack FloorTrack { get; set; } = MusicTrack.Crypt;
        public static MusicTrack BossTrack { get; set; } = MusicTrack.Boss;

        public static void PlayStinger(bool victory)
        {
            if (instance == null) return;
            var clip = victory ? instance.library.stingVictory : instance.library.stingGameOver;
            instance.PlayMusicInternal(MusicTrack.None);
            if (clip != null) instance.stinger.PlayOneShot(clip, GameSettings.MusicVolume);
        }

        /// <summary>Re-applies volume settings to whatever is playing (after the player toggles sound).</summary>
        public static void RefreshVolumes()
        {
            if (instance == null) return;
            instance.decks[instance.activeDeck].volume = GameSettings.MusicVolume;
        }

        void PlayMusicInternal(MusicTrack track)
        {
            if (track == currentTrack) return;
            currentTrack = track;
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            var from = decks[activeDeck];
            activeDeck = 1 - activeDeck;
            var to = decks[activeDeck];
            to.clip = library != null ? library.Music(track) : null;
            to.volume = 0f;
            if (to.clip != null)
            {
                if (to.clip.loadState == AudioDataLoadState.Unloaded) to.clip.LoadAudioData();
                to.Play();
            }
            fadeRoutine = StartCoroutine(Crossfade(from, to));
        }

        IEnumerator Crossfade(AudioSource from, AudioSource to)
        {
            float startFrom = from.volume;
            for (float t = 0f; t < CrossfadeSeconds; t += Time.unscaledDeltaTime)
            {
                float k = t / CrossfadeSeconds;
                from.volume = startFrom * (1f - k);
                to.volume = GameSettings.MusicVolume * k;
                yield return null;
            }
            from.Stop();
            to.volume = GameSettings.MusicVolume;
            fadeRoutine = null;
        }

        // ---------------- run-wide reactions ----------------

        /// <summary>GameEvents are cleared at every run start, so the runner calls this right after.</summary>
        public static void BindRunEvents()
        {
            if (instance == null) return;
            instance.lastGold = 0;
            GameEvents.DamageDealt += instance.OnDamage;
            GameEvents.EnemyKilled += (at, boss) => Play(boss ? Sfx.BossSlam : Sfx.EnemyDeath, at);
            GameEvents.GoldChanged += instance.OnGold;
            GameEvents.ItemEquipped += _ => Play(Sfx.ItemPickup);
            GameEvents.RoomCleared += () => Play(Sfx.RoomClear);
            GameEvents.PlayerDied += () => Play(Sfx.PlayerHurt);
            GameEvents.BossEncountered += _ => { Play(Sfx.BossRoar); PlayMusic(BossTrack); };
            GameEvents.BossDefeated += () => PlayMusic(FloorTrack);
        }

        void OnDamage(DamageInfo info, Vector3 at)
        {
            if (info.SourceTeam == Team.Player) Play(info.IsCrit ? Sfx.HitCrit : Sfx.Hit, at);
            else Play(Sfx.PlayerHurt, at);
        }

        void OnGold(int total)
        {
            if (total > lastGold) Play(Sfx.Coin);
            lastGold = total;
        }
    }
}
