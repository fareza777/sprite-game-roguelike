using System.Collections;
using Oathdeep.Audio;
using Oathdeep.Combat;
using Oathdeep.Dungeon;
using Oathdeep.Loot;
using Oathdeep.UI;
using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>Floor-by-floor descent: build a floor, introduce it, and go down through the portal after its boss.</summary>
    public sealed partial class GameRunner
    {
        const float DescendFadeSeconds = 0.6f;

        DungeonBuilder dungeon;
        int floorIndex;
        bool descending;

        FloorConfig CurrentFloor => floors[Mathf.Clamp(floorIndex, 0, floors.Length - 1)];
        FloorStory CurrentStory => story != null ? story.Data.Floor(CurrentFloor.FloorNumber) : null;

        void EnterFloor(int index)
        {
            StopCoroutine(nameof(IntroduceFloor));
            TeardownFloor();
            floorIndex = Mathf.Clamp(index, 0, floors.Length - 1);
            var config = CurrentFloor;
            int seed = System.Environment.TickCount ^ (floorIndex * 7919);

            TargetRegistry.Clear();
            LootDropper.BeginFloor(config.FloorNumber, seed);
            dungeon = new GameObject("Dungeon").AddComponent<DungeonBuilder>();
            dungeon.SetProps(props);
            dungeon.Build(config, seed, player.transform, OnBossRoomCleared);

            var body = player.GetComponent<Rigidbody2D>();
            body.position = dungeon.StartPosition;
            body.linearVelocity = Vector2.zero;
            player.transform.position = dungeon.StartPosition;
            player.ResetState();
            CameraRig.Follow(player.transform);

            if (globalLight != null && config.Biome != null)
            {
                globalLight.color = config.Biome.AmbientLight;
                globalLight.intensity = config.Biome.AmbientIntensity;
            }
            if (worldCamera != null && config.Biome != null) worldCamera.backgroundColor = config.Biome.AmbientLight * 0.08f;
            if (ambient != null && config.Biome != null) ambient.Show(config.Biome.Ambient, AmbientStyles.For(config.FloorNumber));

            hud.SetFloor(config.FloorNumber, config.Biome != null ? config.Biome.DisplayName : "The Deep");
            hud.Minimap.Show(dungeon.Geometry.Rooms);
            AudioManager.FloorTrack = config.Biome != null ? config.Biome.Music : MusicTrack.Crypt;
            AudioManager.BossTrack = config.Biome != null ? config.Biome.BossMusic : MusicTrack.Boss;
            AudioManager.PlayMusic(AudioManager.FloorTrack);
            SaveSystem.Current.bestFloor = Mathf.Max(SaveSystem.Current.bestFloor, config.FloorNumber);
            QuestTracker.FloorReached(config.FloorNumber, HeroesPage.IdOf(HeroesPage.Selected(heroes)));
            StartCoroutine(nameof(IntroduceFloor));
        }

        void TeardownFloor()
        {
            // Pooled followers (Requiem wisps) must let go before the pool is wiped.
            var choir = player != null ? player.GetComponent<Player.SoulWisps>() : null;
            if (choir != null) choir.enabled = false;
            if (dungeon != null) Destroy(dungeon.gameObject);
            dungeon = null;
            Pool.Clear();
        }

        /// <summary>The Sun names the floor first; first-time players get the tutorial once she has spoken.</summary>
        IEnumerator IntroduceFloor()
        {
            float wait = 0f;
            var floorStory = CurrentStory;
            if (floorStory != null)
            {
                var clip = story.VoiceFor(floorStory.id);
                narration.ShowFloor(floorStory, clip);
                wait = Mathf.Max(5.5f, clip != null ? clip.length + 0.5f : 0f) + 1.3f;
            }
            if (SaveSystem.Current.onboardingDone || floorIndex > 0) yield break;
            yield return new WaitForSecondsRealtime(wait);
            if (hud != null && player != null) OnboardingCoach.Begin(hudRoot, skin, hud, player);
        }

        void OnBossRoomCleared(Vector3 at)
        {
            if (props != null && props.exitPortal != null) Instantiate(props.exitPortal, at, Quaternion.identity, dungeon.transform);
        }

        void OnPortalReached()
        {
            if (descending || phase != Phase.Playing) return;
            if (floorIndex >= FinalFloor - 1) { StartCoroutine(EndAfter(0.4f, true)); return; }
            StartCoroutine(Descend());
        }

        /// <summary>A short fade, then the next biome. Health is topped up a little so a won floor feels earned.</summary>
        IEnumerator Descend()
        {
            descending = true;
            GameEvents.RaiseToast("Deeper...");
            AudioManager.Play(Sfx.DoorOpen);
            yield return frontEnd.FadeOut(DescendFadeSeconds);
            var health = player.GetComponent<Health>();
            health.Heal(health.Max / 4);
            EnterFloor(floorIndex + 1);
            yield return frontEnd.FadeIn(DescendFadeSeconds);
            descending = false;
        }
    }
}
