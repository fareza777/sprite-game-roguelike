using Oathdeep.Combat;
using Oathdeep.Core;
using Oathdeep.Data;
using Oathdeep.Loot;
using UnityEngine;

namespace Oathdeep.Player
{
    /// <summary>Gold + equipped gear for the current run. Re-applies stats whenever gear changes.</summary>
    [RequireComponent(typeof(CombatStats), typeof(Health))]
    public sealed class PlayerInventory : MonoBehaviour
    {
        CombatStats stats;
        Health health;

        public Loadout Loadout { get; private set; }
        public int Gold { get; private set; }

        void Awake()
        {
            stats = GetComponent<CombatStats>();
            health = GetComponent<Health>();
        }

        public void BeginRun(HeroData hero)
        {
            Loadout = new Loadout(hero.BaseStats);
            Gold = 0;
            stats.Set(Loadout.Total, Team.Player);
            health.Init(Loadout.Total.MaxHealth, Team.Player);
            GameEvents.RaiseGoldChanged(Gold);
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            Gold += amount;
            GameEvents.RaiseGoldChanged(Gold);
        }

        /// <summary>Equips the item; returns whatever was in that slot so it can be dropped.</summary>
        public ItemData Equip(ItemData item)
        {
            Loadout = Loadout.Equip(item, out var replaced);
            stats.Set(Loadout.Total, Team.Player);
            health.SetMax(Loadout.Total.MaxHealth);
            GameEvents.RaiseItemEquipped(item);
            return replaced;
        }
    }
}
