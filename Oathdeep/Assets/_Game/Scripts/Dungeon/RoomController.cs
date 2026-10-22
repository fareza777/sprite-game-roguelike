using System.Collections;
using System.Collections.Generic;
using Oathdeep.Combat;
using Oathdeep.Core;
using Oathdeep.Enemies;
using Oathdeep.Loot;
using Oathdeep.Player;
using UnityEngine;
using Oathdeep.Audio;

namespace Oathdeep.Dungeon
{
    /// <summary>
    /// Runs one room: camera bounds on entry, door lock + enemy waves for combat rooms,
    /// boss encounter, and rewards when cleared.
    /// </summary>
    public sealed class RoomController : MonoBehaviour
    {
        const float SpawnTelegraphSeconds = 0.7f;
        const float MinSpawnDistance = 3.5f;
        const float AliveCheckInterval = 0.2f;

        RoomGeometry room;
        System.Func<Vector2Int, bool> isFloor;
        FloorConfig config;
        System.Random rng;
        Transform player;
        List<Door> doors;
        Transform enemyRoot;
        GameObject spawnMarkerPrefab;
        System.Action<Vector3> spawnExit;

        bool visited;
        bool cleared;
        bool fighting;
        bool playerInside;

        public RoomType Type => room.Node.Type;
        public bool Cleared => cleared;
        public Rect WorldBounds { get; private set; }

        public void Init(RoomGeometry geometry, System.Func<Vector2Int, bool> floorTest, FloorConfig floor, System.Random random,
                         Transform playerTransform, List<Door> roomDoors, GameObject spawnMarker, System.Action<Vector3> onSpawnExit)
        {
            room = geometry;
            isFloor = floorTest;
            config = floor;
            rng = random;
            player = playerTransform;
            doors = roomDoors;
            spawnMarkerPrefab = spawnMarker;
            spawnExit = onSpawnExit;

            var r = room.Interior;
            // Include the wall ring so the camera shows the walls around the room.
            WorldBounds = new Rect(r.xMin - 1, r.yMin - 1, r.width + 2, r.height + 3);

            var trigger = gameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.offset = r.center;
            trigger.size = new Vector2(r.width - 1.5f, r.height - 1.5f);

            enemyRoot = new GameObject("Enemies").transform;
            enemyRoot.SetParent(transform, false);

            cleared = !RoomTypes.IsFight(room.Node.Type);
        }

        /// <summary>
        /// The entry trigger is inset so doors never close on the player; leaving is judged on the full interior
        /// instead, so walking along a wall doesn't release the camera.
        /// </summary>
        void Update()
        {
            if (!playerInside || player == null) return;
            Vector2 p = player.position;
            var r = room.Interior;
            if (p.x < r.xMin - 0.2f || p.x > r.xMax + 0.2f || p.y < r.yMin - 0.2f || p.y > r.yMax + 0.2f)
            {
                playerInside = false;
                CameraRig.ReleaseBounds(WorldBounds);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;
            CameraRig.SetBounds(WorldBounds);
            playerInside = true;
            GameEvents.RaiseRoomEntered(room.Node.GridPosition);
            if (visited) return;
            visited = true;

            if (room.Node.Type == RoomType.Treasure) SpawnTreasure();
            if (room.Node.Type == RoomType.Elite) GameEvents.RaiseToast("Something strong waits here...");
            if (!cleared && !fighting) StartCoroutine(room.Node.Type == RoomType.Boss ? RunBoss() : RunWaves());
        }

        IEnumerator RunWaves()
        {
            fighting = true;
            SetDoors(true);
            bool elite = room.Node.Type == RoomType.Elite;
            int waves = rng.Next(config.WavesPerRoom.x, config.WavesPerRoom.y + 1) + (elite ? 1 : 0);
            for (int w = 0; w < waves; w++)
            {
                int count = rng.Next(config.EnemiesPerWave.x, config.EnemiesPerWave.y + 1);
                yield return SpawnWave(count, elite ? 0.45f : config.EliteChance);
                yield return WaitUntilClear();
                if (w < waves - 1) yield return new WaitForSeconds(0.6f);
            }
            Clear();
            if (elite) Reward(1.4f, "The elite falls. Spoils!");
        }

        /// <summary>A cursed chest or similar trap turns a peaceful room into a sealed elite fight.</summary>
        public void StartAmbush()
        {
            if (fighting) return;
            StartCoroutine(RunAmbush());
        }

        IEnumerator RunAmbush()
        {
            fighting = true;
            SetDoors(true);
            for (int w = 0; w < 2; w++)
            {
                yield return SpawnWave(rng.Next(config.EnemiesPerWave.x, config.EnemiesPerWave.y + 1), 0.6f);
                yield return WaitUntilClear();
            }
            Clear();
            Reward(2f, "You survived the ambush.");
        }

        void Reward(float luck, string toast)
        {
            var at = (Vector2)room.Interior.center;
            LootDropper.SpawnItem(ItemGenerator.Roll(rng, config.FloorNumber, luck), at);
            LootDropper.SpawnGold(10 + config.FloorNumber * 8, at);
            GameEvents.RaiseToast(toast);
        }

        IEnumerator RunBoss()
        {
            fighting = true;
            SetDoors(true);
            yield return new WaitForSeconds(0.8f);
            if (config.Boss != null)
            {
                var pos = (Vector3)(Vector2)room.Interior.center + Vector3.up * 3f;
                yield return Telegraph(pos);
                SpawnEnemy(config.Boss, pos);
            }
            yield return WaitUntilClear();
            Clear();
            spawnExit?.Invoke((Vector2)room.Interior.center);
        }

        IEnumerator SpawnWave(int count, float eliteChance)
        {
            var positions = PickSpawnPositions(count);
            foreach (var p in positions) if (spawnMarkerPrefab != null) Pool.Spawn(spawnMarkerPrefab, p, Quaternion.identity);
            if (positions.Count > 0) AudioManager.Play(Sfx.Spawn, positions[0]);
            yield return new WaitForSeconds(SpawnTelegraphSeconds);
            foreach (var p in positions)
            {
                var prefab = config.PickEnemy(rng);
                if (prefab != null) SpawnEnemy(prefab, p, rng.NextDouble() < eliteChance);
            }
        }

        IEnumerator Telegraph(Vector3 at)
        {
            if (spawnMarkerPrefab != null) Pool.Spawn(spawnMarkerPrefab, at, Quaternion.identity);
            yield return new WaitForSeconds(SpawnTelegraphSeconds);
        }

        void SpawnEnemy(EnemyBase prefab, Vector3 at, bool elite = false)
        {
            var e = Instantiate(prefab, at, Quaternion.identity, enemyRoot);
            e.Spawn(player, config.StatScale, elite);
            if (e.TryGetComponent<Enemies.BossCore>(out var boss))
            {
                var r = room.Interior;
                boss.Begin(player, new Rect(r.xMin, r.yMin, r.width, r.height), isFloor);
            }
        }

        List<Vector3> PickSpawnPositions(int count)
        {
            var result = new List<Vector3>();
            var cells = room.SpawnCells;
            int guard = 0;
            while (result.Count < count && guard++ < 200 && cells.Count > 0)
            {
                var c = cells[rng.Next(cells.Count)];
                var p = new Vector3(c.x + 0.5f, c.y + 0.5f, 0f);
                if (player != null && Vector2.Distance(p, player.position) < MinSpawnDistance) continue;
                bool crowded = false;
                foreach (var q in result) if (Vector2.Distance(p, q) < 1.5f) { crowded = true; break; }
                if (!crowded) result.Add(p);
            }
            return result;
        }

        IEnumerator WaitUntilClear()
        {
            var wait = new WaitForSeconds(AliveCheckInterval);
            while (AnyAlive())
            {
                RescueStranded();
                KeepPlayerInside();
                yield return wait;
            }
        }

        /// <summary>Insurance against a sealed fight with the player on the wrong side of the doors.</summary>
        void KeepPlayerInside()
        {
            if (player == null || !RoomRules.IsStranded(player.position, room.Interior, isFloor)) return;
            var cell = RoomRules.NearestCell(player.position, room.SpawnCells, room.Centre);
            var to = new Vector2(cell.x + 0.5f, cell.y + 0.5f);
            var body = player.GetComponent<Rigidbody2D>();
            if (body != null) { body.position = to; body.linearVelocity = Vector2.zero; }
            player.position = to;
        }

        /// <summary>
        /// Summons and slime splits can land inside a wall, pillar or doorway; an enemy the player can't reach
        /// would keep the doors sealed forever, so put it back on the nearest spawn cell.
        /// </summary>
        void RescueStranded()
        {
            foreach (Transform child in enemyRoot)
            {
                var health = child.GetComponent<Health>();
                if (health == null || health.IsDead || !RoomRules.IsStranded(child.position, room.Interior, isFloor)) continue;
                var cell = RoomRules.NearestCell(child.position, room.SpawnCells, room.Centre);
                var to = new Vector2(cell.x + 0.5f, cell.y + 0.5f);
                var body = child.GetComponent<Rigidbody2D>();
                if (body != null) { body.position = to; body.linearVelocity = Vector2.zero; }
                child.position = to;
            }
        }

        bool AnyAlive()
        {
            foreach (Transform child in enemyRoot)
            {
                var h = child.GetComponent<Health>();
                if (h != null && !h.IsDead) return true;
            }
            return false;
        }

        void Clear()
        {
            fighting = false;
            cleared = true;
            SetDoors(false);
            GameEvents.RaiseRoomCleared();
            CameraRig.Shake(0.1f);
            if (room.Node.Type == RoomType.Combat && rng.NextDouble() < 0.35)
                LootDropper.SpawnPotion((Vector2)room.Interior.center);
        }

        void SpawnTreasure()
        {
            var item = ItemGenerator.Roll(rng, config.FloorNumber, 1.2f);
            LootDropper.SpawnItem(item, (Vector2)room.Interior.center);
            GameEvents.RaiseToast("A relic glints in the dark...");
        }

        void SetDoors(bool closed)
        {
            foreach (var d in doors) d.SetClosed(closed);
            if (closed) CameraRig.Shake(0.15f);
        }
    }
}
