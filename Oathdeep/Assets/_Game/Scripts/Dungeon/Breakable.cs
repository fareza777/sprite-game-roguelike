using Oathdeep.Combat;
using Oathdeep.Core;
using Oathdeep.Loot;
using UnityEngine;
using Oathdeep.Audio;

namespace Oathdeep.Dungeon
{
    /// <summary>Pot / barrel / crate. One or two hits shatter it and may spill gold or a potion.</summary>
    [RequireComponent(typeof(Health))]
    public sealed class Breakable : MonoBehaviour
    {
        [SerializeField] int hitPoints = 1;
        [SerializeField] Vector2Int goldRange = new Vector2Int(0, 4);
        [SerializeField, Range(0f, 1f)] float potionChance = 0.06f;
        [SerializeField] GameObject shatterVfx;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite[] variants;

        Health health;

        void Awake()
        {
            health = GetComponent<Health>();
            health.Init(hitPoints, Team.Enemy);
            health.Died += Shatter;
            health.Damaged += _ => CameraRig.Shake(0.04f);
            if (spriteRenderer != null && variants != null && variants.Length > 0)
                spriteRenderer.sprite = variants[Random.Range(0, variants.Length)];
        }

        void Shatter()
        {
            AudioManager.Play(Sfx.Break, transform.position);
            if (shatterVfx != null) Pool.Spawn(shatterVfx, transform.position, Quaternion.identity);
            int gold = Random.Range(goldRange.x, goldRange.y + 1);
            if (gold > 0) LootDropper.SpawnGold(gold, transform.position);
            if (Random.value < potionChance) LootDropper.SpawnPotion(transform.position);
            Haptics.Pulse(8);
            Destroy(gameObject);
        }
    }
}
