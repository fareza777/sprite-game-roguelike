using System.Collections.Generic;
using Oathdeep.Audio;
using Oathdeep.Combat;
using Oathdeep.Core;
using Oathdeep.Data;
using Oathdeep.Loot;
using Oathdeep.Player;
using UnityEngine;

namespace Oathdeep.Dungeon
{
    public enum EncounterKind { Merchant, Sunstone, Pilgrim, Rest, DiceAltar, SunShard, WeepingStatue, CursedChest }

    /// <summary>
    /// The peaceful heart of a non-combat room: Old Tallow's shop, a sunstone, a pilgrim, a campfire or one of the
    /// random events. Each is used once (the shop stays open); results apply to the player's inventory and health.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public sealed class RoomEncounter : MonoBehaviour
    {
        [SerializeField] EncounterKind kind;
        [SerializeField] Sprite portrait;

        Interactable interactable;
        System.Random rng;
        int floor;
        RoomController room;
        List<ShopOffer> stock;

        public EncounterKind Kind => kind;

        public void Init(EncounterKind encounter, int floorNumber, System.Random random, RoomController owner)
        {
            kind = encounter;
            floor = Mathf.Max(1, floorNumber);
            rng = new System.Random(random.Next());
            room = owner;
            interactable = GetComponent<Interactable>();
            interactable.Setup(PromptFor(kind), Use);
            if (kind == EncounterKind.Merchant) stock = BuildStock();
        }

        static string PromptFor(EncounterKind k) => k switch
        {
            EncounterKind.Merchant => "TRADE",
            EncounterKind.Sunstone => "TOUCH",
            EncounterKind.Rest => "REST",
            EncounterKind.Pilgrim => "TALK",
            _ => "EXAMINE",
        };

        PlayerInventory Inventory => FindFirstObjectByType<PlayerInventory>();

        void Use()
        {
            var inv = Inventory;
            if (inv == null) return;
            var health = inv.GetComponent<Health>();
            switch (kind)
            {
                case EncounterKind.Merchant: Merchant(); break;
                case EncounterKind.Sunstone: Sunstone(inv, health); break;
                case EncounterKind.Pilgrim: Pilgrim(inv, health); break;
                case EncounterKind.Rest: Rest(health); break;
                case EncounterKind.DiceAltar: DiceAltar(inv); break;
                case EncounterKind.SunShard: SunShard(inv, health); break;
                case EncounterKind.WeepingStatue: WeepingStatue(inv, health); break;
                case EncounterKind.CursedChest: CursedChest(inv); break;
            }
        }

        // ---------------- merchant ----------------

        List<ShopOffer> BuildStock()
        {
            var list = new List<ShopOffer>();
            for (int i = 0; i < 4; i++)
            {
                var item = ItemGenerator.Roll(rng, floor, 0.9f);
                list.Add(new ShopOffer { Item = item, Price = PriceOf(item) });
            }
            list.Add(new ShopOffer { Item = null, Price = 18 + floor * 6 });   // potion
            list.Add(new ShopOffer { Item = null, Price = 18 + floor * 6 });
            return list;
        }

        int PriceOf(ItemInstance item) => item.SellValue * 3 + floor * 6;

        void Merchant()
        {
            var d = new EncounterDialog
            {
                Speaker = "Old Tallow",
                Portrait = portrait,
                Text = floor == 1
                    ? "Candles, blades, odds and ends. Everything's for sale down here, friend. Even light."
                    : "Back again? The deeper you go, the better my wares. Funny how that works.",
            };
            d.Choices.Add(new EncounterChoice("SHOW ME", () => EncounterHost.OpenShop?.Invoke("OLD TALLOW", portrait, stock)));
            d.Choices.Add(new EncounterChoice("LEAVE", null));
            EncounterHost.Dialog(d);
        }

        // ---------------- sunstone ----------------

        void Sunstone(PlayerInventory inv, Health health)
        {
            var d = new EncounterDialog
            {
                Speaker = "Sunstone",
                Portrait = portrait,
                Text = "A vein of buried sunlight, frozen into crystal. Its warmth can be drawn out once. Choose what to take.",
            };
            // A talent (if any are left) leads the offer; it changes how the hero fights, not just how hard.
            var talents = inv.GetComponent<PlayerTalents>();
            var talent = talents != null ? TalentCatalog.Draw(rng, new System.Collections.Generic.HashSet<Talent>(talents.Owned)) : null;
            if (talent.HasValue)
            {
                var info = TalentCatalog.Info(talent.Value);
                d.Choices.Add(new EncounterChoice(info.Name.ToUpperInvariant() + ": " + info.Text, () =>
                {
                    talents.Add(info.Id);
                    GameEvents.RaiseEncounterUsed("sunstone");
                    AudioManager.Play(Sfx.Boon);
                    Spend();
                }));
            }
            foreach (var b in EncounterData.DrawBoons(rng, talent.HasValue ? 2 : 3))
            {
                var boon = b;
                d.Choices.Add(new EncounterChoice(boon.Name.ToUpperInvariant() + ": " + boon.Text, () =>
                {
                    inv.AddRunBonus(boon.Stats, boon.Specials);
                    GameEvents.RaiseEncounterUsed("sunstone");
                    if (boon.FullHeal) health.Heal(health.Max);
                    AudioManager.Play(Sfx.Boon);
                    GameEvents.RaiseToast("Gained " + boon.Name);
                    Spend();
                }));
            }
            EncounterHost.Dialog(d);
        }

        // ---------------- pilgrim ----------------

        void Pilgrim(PlayerInventory inv, Health health)
        {
            string gift;
            int roll = rng.Next(3);
            if (roll == 0) { health.Heal(health.Max / 3); gift = "She presses a warm stone into your hand. You feel mended."; }
            else if (roll == 1) { int gold = 20 + 12 * floor; inv.AddGold(gold); gift = $"\"Take this. Coin is heavy and I'm going up.\" (+{gold} gold)"; }
            else
            {
                var item = ItemGenerator.Roll(rng, floor, 0.6f);
                LootDropper.SpawnItem(item, transform.position + Vector3.down);
                gift = "\"I found this on someone who stopped walking. You'll need it more.\"";
            }
            var d = new EncounterDialog
            {
                Speaker = "Pilgrim of the Sun",
                Portrait = portrait,
                Text = "\"" + EncounterData.PilgrimLine(floor, rng) + "\"\n\n" + gift,
            };
            d.Choices.Add(new EncounterChoice("THANK THEM", null));
            GameEvents.RaiseEncounterUsed("pilgrim");
            EncounterHost.Dialog(d);
            Spend();
        }

        // ---------------- rest ----------------

        void Rest(Health health)
        {
            var d = new EncounterDialog
            {
                Speaker = "Campfire",
                Portrait = portrait,
                Text = "Someone left a fire burning. It won't last long. Rest a while?",
            };
            d.Choices.Add(new EncounterChoice("REST (heal 50%)", () =>
            {
                health.Heal(health.Max / 2);
                AudioManager.Play(Sfx.Potion);
                GameEvents.RaiseToast("You feel rested.");
                Spend();
            }));
            d.Choices.Add(new EncounterChoice("NOT YET", null));
            EncounterHost.Dialog(d);
        }

        // ---------------- events ----------------

        void DiceAltar(PlayerInventory inv)
        {
            int stake = Mathf.Max(10, inv.Gold / 4);
            var d = new EncounterDialog
            {
                Speaker = "Gambler's Altar",
                Portrait = portrait,
                Text = $"A grinning skull and two bone dice. Carved below: WAGER {stake}. DOUBLE OR NOTHING.",
            };
            d.Choices.Add(new EncounterChoice($"ROLL ({stake} gold)", () =>
            {
                if (!inv.SpendGold(stake)) return;
                GameEvents.RaiseEncounterUsed("dice");
                if (rng.NextDouble() < 0.5)
                {
                    inv.AddGold(stake * 2);
                    LootDropper.SpawnItem(ItemGenerator.Roll(rng, floor, 0.8f), transform.position + Vector3.down);
                    GameEvents.RaiseToast("Sixes! The skull rattles with laughter.");
                    AudioManager.Play(Sfx.Coin);
                }
                else GameEvents.RaiseToast("Snake eyes. The skull keeps your coin.");
                Spend();
            }, inv.Gold >= stake));
            d.Choices.Add(new EncounterChoice("WALK AWAY", null));
            EncounterHost.Dialog(d);
        }

        void SunShard(PlayerInventory inv, Health health)
        {
            int gold = 30 + 15 * floor;
            var d = new EncounterDialog
            {
                Speaker = "Shard of the Sun",
                Portrait = portrait,
                Text = "A splinter of her light, hanging in the dark. It hums when you come close.",
            };
            d.Choices.Add(new EncounterChoice("ABSORB IT (+15 Health, full heal)", () =>
            {
                inv.AddRunBonus(new StatBlock(15, 0, 0, 0f, 0f, 0f), default);
                health.Heal(health.Max);
                GameEvents.RaiseToast("Her light runs through you.");
                Spend();
            }));
            d.Choices.Add(new EncounterChoice($"SHATTER IT (+{gold} gold)", () =>
            {
                inv.AddGold(gold);
                AudioManager.Play(Sfx.Break);
                Spend();
            }));
            EncounterHost.Dialog(d);
        }

        void WeepingStatue(PlayerInventory inv, Health health)
        {
            int cost = Mathf.Max(1, health.Current / 4);
            var d = new EncounterDialog
            {
                Speaker = "Weeping Statue",
                Portrait = portrait,
                Text = "A saint of the Choir, weeping red. Its open palms ask for an offering of blood.",
            };
            d.Choices.Add(new EncounterChoice($"OFFER BLOOD (-{cost} HP)", () =>
            {
                health.TryTakeDamage(new DamageInfo(cost, false, transform.position, 0f, 0f, Team.Hazard));
                LootDropper.SpawnItem(ItemGenerator.Roll(rng, floor, 1.6f), transform.position + Vector3.down);
                GameEvents.RaiseToast("The statue stops weeping.");
                Spend();
            }, health.Current > cost + 1));
            d.Choices.Add(new EncounterChoice("LEAVE IT", null));
            EncounterHost.Dialog(d);
        }

        void CursedChest(PlayerInventory inv)
        {
            var d = new EncounterDialog
            {
                Speaker = "Chained Chest",
                Portrait = portrait,
                Text = "A heavy chest wrapped in black chains. Something inside is breathing. Or it's treasure. Probably treasure.",
            };
            d.Choices.Add(new EncounterChoice("BREAK THE CHAINS", () =>
            {
                Spend();
                if (rng.NextDouble() < 0.6)
                {
                    LootDropper.SpawnItem(ItemGenerator.Roll(rng, floor, 2f), transform.position + Vector3.down);
                    LootDropper.SpawnGold(15 + floor * 10, transform.position + Vector3.down);
                    GameEvents.RaiseToast("Treasure!");
                }
                else
                {
                    GameEvents.RaiseToast("It was breathing.");
                    if (room != null) room.StartAmbush();
                }
            }));
            d.Choices.Add(new EncounterChoice("LEAVE IT CHAINED", null));
            EncounterHost.Dialog(d);
        }

        void Spend()
        {
            if (interactable != null) interactable.Spend();
        }
    }
}
