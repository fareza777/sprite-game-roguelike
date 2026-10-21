using System.Collections.Generic;
using Oathdeep.Combat;
using Oathdeep.Core;
using Oathdeep.Data;
using Oathdeep.Loot;
using UnityEngine;

namespace Oathdeep.Player
{
    /// <summary>Gold, equipped gear and the bag for the current run. Re-applies stats whenever gear changes and
    /// drives the item specials: lifesteal, thorns, gold find (skill haste is read by the controller).</summary>
    [RequireComponent(typeof(CombatStats), typeof(Health))]
    public sealed class PlayerInventory : MonoBehaviour
    {
        public const int BagSize = 12;
        const float ThornsReach = 1.8f;

        public enum TakeResult { Equipped, Bagged, Full }

        CombatStats stats;
        Health health;
        readonly List<ItemInstance> bag = new List<ItemInstance>();
        float lifestealCarry;

        public Loadout Loadout { get; private set; }
        public IReadOnlyList<ItemInstance> Bag => bag;
        public int Gold { get; private set; }
        public ItemSpecials Specials => Loadout != null ? Loadout.Specials : ItemSpecials.None;
        /// <summary>Multiplier applied to the hero's skill cooldown.</summary>
        public float SkillCooldownScale => 1f - Specials.SkillHaste;
        public event System.Action Changed;

        void Awake()
        {
            stats = GetComponent<CombatStats>();
            health = GetComponent<Health>();
        }

        void OnEnable() => health.Damaged += OnDamaged;
        void OnDisable() => health.Damaged -= OnDamaged;

        public void BeginRun(HeroData hero)
        {
            Loadout = new Loadout(hero.BaseStats);
            bag.Clear();
            Gold = 0;
            stats.Set(Loadout.Total, Team.Player);
            health.Init(Loadout.Total.MaxHealth, Team.Player);
            GameEvents.DamageDealt += OnDamageDealt;   // cleared with the bus at the next run
            GameEvents.RaiseGoldChanged(Gold);
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            Gold += Mathf.RoundToInt(amount * (1f + Specials.GoldFind));
            GameEvents.RaiseGoldChanged(Gold);
        }

        public bool SpendGold(int amount)
        {
            if (amount < 0 || Gold < amount) return false;
            Gold -= amount;
            GameEvents.RaiseGoldChanged(Gold);
            return true;
        }

        /// <summary>A picked-up item fills an empty slot, otherwise goes to the bag. Full bag leaves it on the floor.</summary>
        public TakeResult Take(ItemInstance item)
        {
            if (item == null) return TakeResult.Full;
            bool empty = Loadout.Get(item.Slot) == null;
            if (!empty && bag.Count >= BagSize) return TakeResult.Full;
            if (SaveSystem.Current.DiscoverItem(item.BaseId)) GameEvents.RaiseItemFound(item);
            if (empty)
            {
                Apply(Loadout.Equip(item, out _));
                GameEvents.RaiseItemEquipped(item);
                return TakeResult.Equipped;
            }
            bag.Add(item);
            Changed?.Invoke();
            return TakeResult.Bagged;
        }

        /// <summary>Equips a bag item; whatever was worn goes back into its bag slot.</summary>
        public void EquipFromBag(ItemInstance item)
        {
            int index = bag.IndexOf(item);
            if (index < 0) return;
            Apply(Loadout.Equip(item, out var replaced));
            if (replaced != null) bag[index] = replaced; else bag.RemoveAt(index);
            GameEvents.RaiseItemEquipped(item);
            Changed?.Invoke();
        }

        public bool Unequip(EquipSlot slot)
        {
            if (Loadout.Get(slot) == null || bag.Count >= BagSize) return false;
            Apply(Loadout.Unequip(slot, out var removed));
            bag.Add(removed);
            Changed?.Invoke();
            return true;
        }

        /// <summary>Breaks a bag item down into gold.</summary>
        public int Salvage(ItemInstance item)
        {
            if (!bag.Remove(item)) return 0;
            Gold += item.SellValue;
            GameEvents.RaiseGoldChanged(Gold);
            Changed?.Invoke();
            return item.SellValue;
        }

        void Apply(Loadout next)
        {
            Loadout = next;
            stats.Set(Loadout.Total, Team.Player);
            health.SetMax(Loadout.Total.MaxHealth);
            Changed?.Invoke();
        }

        void OnDamageDealt(DamageInfo info, Vector3 at)
        {
            if (info.SourceTeam != Team.Player || health.IsDead || Specials.Lifesteal <= 0f) return;
            lifestealCarry += info.Amount * Specials.Lifesteal;   // keep fractions so small hits still add up
            int heal = (int)lifestealCarry;
            if (heal <= 0) return;
            lifestealCarry -= heal;
            health.Heal(heal);
        }

        void OnDamaged(DamageInfo info)
        {
            int thorns = Specials.Thorns;
            if (thorns <= 0) return;
            var attacker = TargetRegistry.Nearest(info.SourcePosition, ThornsReach);
            if (attacker == null || Vector2.Distance(attacker.transform.position, transform.position) > ThornsReach * 1.5f) return;
            var hit = new DamageInfo(thorns, false, transform.position, 2f, 0f, Team.Player);
            if (attacker.TryTakeDamage(hit)) GameEvents.RaiseDamageDealt(hit, attacker.transform.position);
        }
    }
}
