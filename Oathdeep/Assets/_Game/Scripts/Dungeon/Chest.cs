using System.Collections;
using Oathdeep.Core;
using Oathdeep.Loot;
using Oathdeep.Player;
using UnityEngine;
using Oathdeep.Audio;

namespace Oathdeep.Dungeon
{
    /// <summary>Opens when the player walks into it (one-handed friendly). Gold burst + optional relic.</summary>
    public sealed class Chest : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite closedSprite;
        [SerializeField] Sprite openSprite;
        [SerializeField] GameObject openVfx;
        [SerializeField] Vector2Int goldRange = new Vector2Int(8, 16);
        [SerializeField, Range(0f, 1f)] float potionChance = 0.5f;

        ItemInstance relic;
        bool opened;

        public void Setup(ItemInstance item, Vector2Int gold)
        {
            relic = item;
            goldRange = gold;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (opened || other.GetComponentInParent<PlayerInventory>() == null) return;
            StartCoroutine(Open());
        }

        IEnumerator Open()
        {
            opened = true;
            GameEvents.RaiseChestOpened();
            AudioManager.Play(Sfx.ChestOpen, transform.position);
            if (openSprite != null) spriteRenderer.sprite = openSprite;
            if (openVfx != null) Pool.Spawn(openVfx, transform.position + Vector3.up * 0.3f, Quaternion.identity);
            CameraRig.Shake(0.12f);
            Haptics.Pulse(30);
            yield return new WaitForSeconds(0.15f);

            LootDropper.SpawnGold(Random.Range(goldRange.x, goldRange.y + 1), transform.position + Vector3.down * 0.4f);
            if (Random.value < potionChance) LootDropper.SpawnPotion(transform.position + Vector3.down * 0.4f);
            if (relic != null)
            {
                LootDropper.SpawnItem(relic, transform.position + Vector3.down * 0.6f);
                GameEvents.RaiseToast($"<color={RarityInfo.Hex(relic.Rarity)}>{relic.Name}</color>!");
            }
        }
    }
}
