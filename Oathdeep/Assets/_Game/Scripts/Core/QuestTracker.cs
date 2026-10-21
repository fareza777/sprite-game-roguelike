using System.Linq;
using Oathdeep.Audio;
using Oathdeep.Data;
using Oathdeep.Loot;

namespace Oathdeep.Core
{
    /// <summary>
    /// Listens to the run's events and moves side quests forward in the save. Bound fresh at the start of every run
    /// (the event bus is cleared between runs). Completed quests are announced; rewards are claimed on the Quests page.
    /// </summary>
    public static class QuestTracker
    {
        public static void BindRunEvents()
        {
            GameEvents.EnemyKilled += (_, boss) =>
            {
                Bump(QuestTrigger.Kills, 1);
                if (boss) Refresh();
            };
            GameEvents.EliteKilled += () => Bump(QuestTrigger.EliteKills, 1);
            GameEvents.ChestOpened += () => Bump(QuestTrigger.ChestsOpened, 1);
            GameEvents.Purchased += () => Bump(QuestTrigger.Purchases, 1);
            GameEvents.EncounterUsed += kind =>
            {
                if (kind == "pilgrim") Bump(QuestTrigger.PilgrimsMet, 1);
                else if (kind == "shrine") Bump(QuestTrigger.ShrinesPrayed, 1);
                else if (kind == "dice") Bump(QuestTrigger.DiceRolled, 1);
            };
            GameEvents.ItemFound += _ => Refresh();
            Refresh();
        }

        /// <summary>Records that this hero has reached a floor (the Unsung Five counts B3 with every hero).</summary>
        public static void FloorReached(int floor, string heroId)
        {
            var save = SaveSystem.Current;
            if (floor >= 3 && !save.heroesAtB3.Contains(heroId)) save.heroesAtB3.Add(heroId);
            Refresh();
        }

        /// <summary>Re-derives the quests whose goal is a total kept in the save.</summary>
        static void Refresh()
        {
            var save = SaveSystem.Current;
            Set(QuestTrigger.ItemsFound, save.items.Count);
            Set(QuestTrigger.LegendariesFound, save.items.Count(id => ItemCatalog.Get(id)?.Legendary == true));
            Set(QuestTrigger.HeroesAtB3, save.heroesAtB3.Count);
            Set(QuestTrigger.ChoristersSilenced, Bestiary.All.Count(e => e.IsBoss && save.IsCodexUnlocked(e.Id)));
        }

        static void Bump(QuestTrigger trigger, int amount)
        {
            foreach (var q in QuestCatalog.Side.Where(q => q.Trigger == trigger))
                if (QuestCatalog.Advance(SaveSystem.Current, q, amount)) Announce(q);
        }

        static void Set(QuestTrigger trigger, int total)
        {
            foreach (var q in QuestCatalog.Side.Where(q => q.Trigger == trigger))
                if (QuestCatalog.SetProgress(SaveSystem.Current, q, total)) Announce(q);
        }

        static void Announce(SideQuest q)
        {
            AudioManager.Play(Sfx.QuestComplete);
            GameEvents.RaiseToast($"Quest complete: {q.Title}!");
        }

        /// <summary>The permanent bonuses as a stat block and specials, ready to add to a fresh run.</summary>
        public static (StatBlock stats, ItemSpecials specials, int gold) MetaBonus(SaveData save)
        {
            var m = save.meta;
            return (new StatBlock(m.health, m.attack, m.defense, m.crit, 0f, 0f), new ItemSpecials(0f, m.skillHaste, m.goldFind, 0), m.startGold);
        }
    }
}
