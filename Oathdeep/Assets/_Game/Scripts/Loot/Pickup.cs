using Oathdeep.Combat;
using Oathdeep.Core;
using Oathdeep.Player;
using UnityEngine;
using Oathdeep.Audio;

namespace Oathdeep.Loot
{
    public enum PickupKind { Gold, Potion, Item }

    /// <summary>World pickup: pops out, bobs, magnetises to the player, applies on touch.</summary>
    public sealed class Pickup : MonoBehaviour
    {
        [SerializeField] SpriteRenderer sprite;
        [SerializeField] SpriteRenderer glow;
        [SerializeField] float magnetRange = 1.8f;
        [SerializeField] float magnetSpeed = 9f;

        PickupKind kind;
        int amount;
        ItemInstance item;
        Vector2 popVelocity;
        float armedAt;
        Transform player;
        static float nextFullWarning;

        public void Setup(PickupKind k, int value, ItemInstance data, Sprite icon)
        {
            kind = k;
            amount = value;
            item = data;
            if (icon != null) sprite.sprite = icon;
            if (glow != null)
            {
                glow.enabled = k == PickupKind.Item;
                if (data != null) glow.color = RarityInfo.ColorOf(data.Rarity) * new Color(1, 1, 1, 0.6f);
            }
            popVelocity = Random.insideUnitCircle.normalized * Random.Range(1.5f, 3f);
            armedAt = Time.time + (k == PickupKind.Item ? 0.6f : 0.35f);
            player = null;
        }

        void Update()
        {
            popVelocity = Vector2.MoveTowards(popVelocity, Vector2.zero, 8f * Time.deltaTime);
            transform.position += (Vector3)(popVelocity * Time.deltaTime);
            sprite.transform.localPosition = new Vector3(0f, 0.08f + Mathf.Sin(Time.time * 5f) * 0.05f, 0f);

            if (Time.time < armedAt || kind == PickupKind.Item) return; // items need a deliberate walk-over
            if (player == null)
            {
                var pc = FindFirstObjectByType<PlayerController>();
                if (pc == null) return;
                player = pc.transform;
            }
            Vector2 to = player.position - transform.position;
            if (to.sqrMagnitude < magnetRange * magnetRange)
                transform.position += (Vector3)(to.normalized * magnetSpeed * Time.deltaTime);
        }

        void OnTriggerEnter2D(Collider2D other) => TryCollect(other);
        void OnTriggerStay2D(Collider2D other) => TryCollect(other);

        void TryCollect(Collider2D other)
        {
            if (Time.time < armedAt) return;
            var inv = other.GetComponentInParent<PlayerInventory>();
            if (inv == null) return;
            var health = inv.GetComponent<Health>();
            if (health.IsDead) return;

            switch (kind)
            {
                case PickupKind.Gold:
                    inv.AddGold(amount);
                    break;
                case PickupKind.Potion:
                    if (health.Current >= health.Max) return; // leave it for later
                    health.Heal(amount);
                    AudioManager.Play(Sfx.Potion);
                    GameEvents.RaiseToast($"+{amount} HP");
                    break;
                case PickupKind.Item:
                    string name = $"<color={RarityInfo.Hex(item.Rarity)}>{item.Name}</color>";
                    switch (inv.Take(item))
                    {
                        case PlayerInventory.TakeResult.Equipped: GameEvents.RaiseToast($"Equipped {name}"); break;
                        case PlayerInventory.TakeResult.Bagged: GameEvents.RaiseToast($"{name} - in your bag"); break;
                        default:
                            if (Time.time >= nextFullWarning) { GameEvents.RaiseToast("Bag full - salvage something"); nextFullWarning = Time.time + 3f; }
                            return;   // stays on the floor
                    }
                    break;
            }
            Haptics.Pulse(10);
            Pool.Despawn(gameObject);
        }
    }
}
