using System.Collections;
using System.Collections.Generic;
using Oathdeep.Combat;
using Oathdeep.Core;
using Oathdeep.Enemies;
using Oathdeep.Loot;
using UnityEngine;

namespace Oathdeep.Dungeon
{
    /// <summary>
    /// Room variety: each combat room gets a challenge (waves, survive, nests, gauntlet) and the biome's terrain,
    /// and the harder rooms end in a choice of spoils.
    /// </summary>
    public sealed partial class RoomController
    {
        const int SurviveCrowd = 6;   // spawning pauses while this many foes are still up
        const int NestCrowd = 8;
        static readonly AttackSpec TerrainBurn = new AttackSpec(1f, 0f, 0f, 0f, 0.03f);

        RoomChallenge challenge = RoomChallenge.Waves;
        BossFx fx;
        CombatStats terrainOwner;
        Health playerHealth;

        public RoomChallenge Challenge => challenge;

        /// <summary>Called by the builder after Init: picks the challenge and lays the biome's terrain.</summary>
        public void ConfigureChallenge(BossFx art, CombatStats terrainStats)
        {
            fx = art;
            terrainOwner = terrainStats;
            playerHealth = player != null ? player.GetComponent<Health>() : null;
            if (room.Node.Type == RoomType.Combat) challenge = RoomChallenges.Pick(config.FloorNumber, rng.NextDouble());
            if (room.Node.Type == RoomType.Combat || room.Node.Type == RoomType.Elite)
                PlaceTerrain(RoomChallenges.TerrainPatches(config.FloorNumber, challenge));
        }

        IEnumerator RunChallenge() => challenge switch
        {
            RoomChallenge.Survive => RunSurvive(),
            RoomChallenge.Nests => RunNests(),
            _ => RunWaves(),
        };

        // ---------------- terrain ----------------

        void PlaceTerrain(int patches)
        {
            var terrain = RoomChallenges.TerrainFor(config.FloorNumber);
            if (terrain == null || fx == null || patches <= 0) return;
            var t = terrain.Value;
            var sprite = TerrainSprite(t.Art);
            if (sprite == null) return;
            var c = room.Centre;
            // Keep the cross lanes through the room (and so every doorway) clear of hazards.
            var cells = room.SpawnCells.FindAll(p => Mathf.Abs(p.x - c.x) > 3 && Mathf.Abs(p.y - c.y) > 3);
            var tint = new Color(1f, 1f, 1f, t.Kind == HazardKind.Ice ? 0.5f : 0.7f);
            for (int i = 0; i < patches && cells.Count > 0; i++)
            {
                var cell = cells[rng.Next(cells.Count)];
                var at = new Vector2(cell.x + 0.5f, cell.y + 0.5f);
                HazardZone.Spawn(transform, sprite, tint, t.Kind, t.Strength, TerrainShape(t.Art, at), 0f, 99999f, playerHealth, terrainOwner, TerrainBurn);
                cells.RemoveAll(p => (new Vector2(p.x + 0.5f, p.y + 0.5f) - at).sqrMagnitude < 25f);   // spread the patches out
            }
        }

        Sprite TerrainSprite(TerrainArt art) => art switch
        {
            TerrainArt.Spores => fx.sporeCloud,
            TerrainArt.Water => fx.water,
            TerrainArt.Lava => fx.lava,
            TerrainArt.Ice => fx.ice,
            TerrainArt.Roots => fx.rootSnare,
            TerrainArt.Bubble => fx.timeBubble,
            _ => fx.quicksand,
        };

        /// <summary>Circles match their art's size exactly; tiled terrain is a small slab.</summary>
        HazardShape TerrainShape(TerrainArt art, Vector2 at) => art switch
        {
            TerrainArt.Spores => HazardShape.Circle(at, 2.25f),
            TerrainArt.Roots => HazardShape.Circle(at, 1.25f),
            TerrainArt.Bubble or TerrainArt.Quicksand => HazardShape.Circle(at, 2.5f),
            _ => HazardShape.Box(rng.Next(2) == 0 ? new Rect(at.x - 1.5f, at.y - 1f, 3f, 2f) : new Rect(at.x - 1f, at.y - 1.5f, 2f, 3f)),
        };

        // ---------------- survive ----------------

        IEnumerator RunSurvive()
        {
            fighting = true;
            SetDoors(true);
            float seconds = RoomChallenges.SurviveSeconds(config.FloorNumber);
            GameEvents.RaiseToast($"Survive for {seconds:0} seconds!");
            float end = Time.time + seconds, nextWave = 0f;
            bool warned10 = false, warned5 = false;
            while (Time.time < end)
            {
                if (Time.time >= nextWave && CountAlive() < SurviveCrowd)
                {
                    nextWave = Time.time + 4.5f;
                    StartCoroutine(SpawnWave(rng.Next(2, 4), config.EliteChance));
                }
                float left = end - Time.time;
                if (!warned10 && left <= 10f) { warned10 = true; GameEvents.RaiseToast("10 seconds left!"); }
                if (!warned5 && left <= 5f) { warned5 = true; GameEvents.RaiseToast("5 seconds!"); }
                RescueStranded();
                KeepPlayerInside();
                yield return new WaitForSeconds(AliveCheckInterval);
            }
            // Time is up: whatever is left crumbles.
            foreach (Transform child in enemyRoot)
            {
                var h = child.GetComponent<Health>();
                if (h != null && !h.IsDead) h.TryTakeDamage(new DamageInfo(999999, false, child.position, 0f, 0f, Team.Hazard));
            }
            yield return new WaitForSeconds(0.5f);
            Clear();
            OfferSpoils("You held the line!");
        }

        // ---------------- nests ----------------

        IEnumerator RunNests()
        {
            fighting = true;
            SetDoors(true);
            int count = config.FloorNumber >= 6 ? 3 : 2;
            int hp = Mathf.RoundToInt(45f * config.StatScale);
            var nests = new List<BossAnchor>();
            foreach (var p in PickSpawnPositions(count))
            {
                var nest = BossAnchor.Spawn(transform, fx != null ? fx.nest : null, p, hp, new Vector2(1.4f, 1.4f));
                nest.Broken += n =>
                {
                    nests.Remove(n);
                    GameEvents.RaiseToast(nests.Count > 0 ? "A nest is broken!" : "The last nest falls!");
                };
                nests.Add(nest);
            }
            GameEvents.RaiseToast("Nests! Break them to stop the swarm.");
            float nextSpawn = 1.5f;
            while (nests.Count > 0)
            {
                nextSpawn -= AliveCheckInterval;
                if (nextSpawn <= 0f)
                {
                    nextSpawn = 5f;
                    foreach (var nest in nests)
                        if (nest != null && CountAlive() < NestCrowd) SpawnFromNest(nest.transform.position);
                }
                RescueStranded();
                KeepPlayerInside();
                yield return new WaitForSeconds(AliveCheckInterval);
            }
            yield return WaitUntilClear();
            Clear();
            OfferSpoils("The nests are broken!");
        }

        void SpawnFromNest(Vector2 at)
        {
            var prefab = config.PickEnemy(rng);
            if (prefab == null) return;
            var near = at + new Vector2((float)rng.NextDouble() * 2f - 1f, (float)rng.NextDouble() * 2f - 1f) * 1.5f;
            var cell = RoomRules.NearestCell(near, room.SpawnCells, room.Centre);
            var pos = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
            if (spawnMarkerPrefab != null) Pool.Spawn(spawnMarkerPrefab, pos, Quaternion.identity);
            SpawnEnemy(prefab, pos, rng.NextDouble() < config.EliteChance * 0.5);
        }

        int CountAlive()
        {
            int n = 0;
            foreach (Transform child in enemyRoot)
            {
                var h = child.GetComponent<Health>();
                if (h != null && !h.IsDead) n++;
            }
            return n;
        }

        // ---------------- spoils ----------------

        /// <summary>The harder rooms end in a choice: gold, healing, or a piece of gear.</summary>
        void OfferSpoils(string title)
        {
            int floor = config.FloorNumber;
            int gold = 15 + floor * 10;
            Vector2 at = player != null ? (Vector2)player.position : room.Interior.center;
            var dialog = new EncounterDialog { Speaker = "Spoils", Text = title + "\nChoose your reward." };
            dialog.Choices.Add(new EncounterChoice($"GOLD: +{gold}", () => LootDropper.SpawnGold(gold, at)));
            dialog.Choices.Add(new EncounterChoice("MEND: heal 40%", () =>
            {
                if (playerHealth != null) playerHealth.Heal(Mathf.RoundToInt(playerHealth.Max * 0.4f));
            }));
            dialog.Choices.Add(new EncounterChoice("GEAR: a random item", () => LootDropper.SpawnItem(ItemGenerator.Roll(rng, floor, 1.4f), at)));
            EncounterHost.Dialog(dialog);
        }
    }
}
