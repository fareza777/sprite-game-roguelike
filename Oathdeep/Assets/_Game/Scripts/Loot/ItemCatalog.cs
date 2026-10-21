using System.Collections.Generic;
using System.Linq;
using Oathdeep.Data;

namespace Oathdeep.Loot
{
    /// <summary>One item identity: 64 bases that roll random rarity + affixes, and 40 legendaries with fixed power.</summary>
    public sealed class ItemBase
    {
        public string Id { get; }
        public string Name { get; }
        public string Icon { get; }
        public EquipSlot Slot { get; }
        public int MinFloor { get; }
        public StatBlock Stats { get; }
        public ItemSpecials Specials { get; }
        public string Flavor { get; }
        public bool Legendary { get; }

        public ItemBase(string id, string name, string icon, EquipSlot slot, int minFloor, StatBlock stats, ItemSpecials specials, string flavor, bool legendary)
        {
            Id = id; Name = name; Icon = icon; Slot = slot; MinFloor = minFloor; Stats = stats; Specials = specials; Flavor = flavor; Legendary = legendary;
        }
    }

    /// <summary>A random modifier. Values are the maximum roll at floor 1; flat values grow with depth.</summary>
    public sealed class AffixDef
    {
        public string Id { get; }
        public string Name { get; }
        public bool IsPrefix { get; }
        public StatBlock Stats { get; }
        public ItemSpecials Specials { get; }

        public AffixDef(string id, string name, bool prefix, StatBlock stats, ItemSpecials specials)
        {
            Id = id; Name = name; IsPrefix = prefix; Stats = stats; Specials = specials;
        }
    }

    public static class ItemCatalog
    {
        static StatBlock S(int hp = 0, int atk = 0, int def = 0, float crit = 0f, float critMult = 0f, float move = 0f) =>
            new StatBlock(hp, atk, def, crit, critMult, move);

        static ItemSpecials X(float lifesteal = 0f, float haste = 0f, float gold = 0f, int thorns = 0) =>
            new ItemSpecials(lifesteal, haste, gold, thorns);

        static ItemBase B(string id, string name, string icon, EquipSlot slot, int floor, StatBlock stats, string flavor, ItemSpecials specials = default) =>
            new ItemBase(id, name, icon, slot, floor, stats, specials, flavor, false);

        static ItemBase L(string id, string name, string icon, EquipSlot slot, int floor, StatBlock stats, ItemSpecials specials, string flavor) =>
            new ItemBase(id, name, icon, slot, floor, stats, specials, flavor, true);

        const EquipSlot W = EquipSlot.Weapon, A = EquipSlot.Armor, C = EquipSlot.Charm, R = EquipSlot.Relic;

        public static readonly IReadOnlyList<ItemBase> Bases = new[]
        {
            // ---- weapons ----
            B("rusted_blade", "Rusted Blade", "sword", W, 1, S(atk: 4), "Notched, but it remembers its oath."),
            B("pilgrim_sword", "Pilgrim's Sword", "sword", W, 1, S(atk: 5, crit: 0.02f), "Blessed at a roadside shrine, once."),
            B("crypt_longsword", "Crypt Longsword", "longsword", W, 2, S(atk: 6, crit: 0.03f), "Pried from a sarcophagus lid."),
            B("oathsworn_greatsword", "Oathsworn Greatsword", "greatsword", W, 3, S(atk: 9, critMult: 0.2f, move: -0.2f), "Heavy enough to split a crypt door."),
            B("woodcutter_axe", "Woodcutter's Axe", "axe", W, 1, S(atk: 5, critMult: 0.15f), "It has felled worse than trees."),
            B("headsman_axe", "Headsman's Axe", "axe", W, 4, S(atk: 8, critMult: 0.3f, move: -0.15f), "Never asked a name, never needed one."),
            B("chapel_mace", "Chapel Mace", "mace", W, 1, S(atk: 5, def: 1), "Rang the vespers before it rang skulls."),
            B("morningstar", "Morningstar", "flail", W, 3, S(atk: 7, critMult: 0.2f), "A star that only rises downward."),
            B("forge_hammer", "Forge Hammer", "hammer", W, 4, S(atk: 9, def: 1, move: -0.2f), "Still warm from the Ember Forge."),
            B("bell_maul", "Bell Maul", "hammer", W, 6, S(hp: 10, atk: 11, move: -0.25f), "Every swing tolls."),
            B("gutting_knife", "Gutting Knife", "dagger", W, 1, S(atk: 3, crit: 0.06f), "Short, cruel and honest."),
            B("mercy_dirk", "Mercy Dirk", "dagger", W, 3, S(atk: 4, crit: 0.08f, critMult: 0.2f), "Made to end things quickly."),
            B("boar_spear", "Boar Spear", "spear", W, 2, S(atk: 6, move: 0.1f), "The crossbar stops what charges."),
            B("tide_trident", "Tide Trident", "spear", W, 3, S(atk: 7, crit: 0.03f), "Salt-crusted tines that never dry."),
            B("hunting_crossbow", "Hunting Crossbow", "crossbow", W, 1, S(atk: 5, crit: 0.04f), "Lamplighters keep one by the door."),
            B("arbalest", "Arbalest", "crossbow", W, 5, S(atk: 8, crit: 0.05f, move: -0.1f), "Cranked steel that punches through bone."),
            B("ash_staff", "Ash Staff", "staff", W, 1, S(hp: 5, atk: 5), "Cut from the last ash tree in the dusk."),
            B("choir_staff", "Choir Staff", "staff", W, 5, S(atk: 8, crit: 0.03f), "Hums the old hymn if you hold it close.", X(haste: 0.05f)),
            B("grave_scythe", "Grave Scythe", "scythe", W, 6, S(atk: 10, critMult: 0.25f), "Reaps what the Choir sowed."),
            B("glass_falchion", "Glass Falchion", "longsword", W, 8, S(def: -1, atk: 12, crit: 0.06f), "Brittle, and terribly sharp."),

            // ---- armor ----
            B("pilgrim_tabard", "Pilgrim's Tabard", "robe", A, 1, S(hp: 15, def: 1, move: 0.1f), "Light cloth for long descents."),
            B("crypt_plate", "Crypt Plate", "plate", A, 2, S(hp: 25, def: 4, move: -0.1f), "Cold iron, still warm from its last owner."),
            B("helm_of_vigil", "Helm of the Vigil", "helm", A, 1, S(hp: 18, def: 2), "Its wearer never sleeps below ground."),
            B("leather_jerkin", "Leather Jerkin", "leather", A, 1, S(hp: 12, def: 2, move: 0.05f), "Patched a hundred times. Still holds."),
            B("riveted_mail", "Riveted Mail", "mail", A, 2, S(hp: 20, def: 3), "Each ring hammered shut by hand."),
            B("mourner_robe", "Mourner's Robe", "robe", A, 3, S(hp: 16, def: 1, crit: 0.03f), "Grey wool soaked in a thousand funerals."),
            B("bog_waders", "Bog Waders", "boots", A, 2, S(hp: 8, def: 1, move: 0.25f), "Keeps the mire out. Mostly."),
            B("iron_gauntlets", "Iron Gauntlets", "gauntlet", A, 2, S(hp: 8, atk: 1, def: 2), "Knuckles first."),
            B("drowned_cloak", "Drowned Cloak", "cloak", A, 3, S(hp: 14, def: 2, move: 0.15f), "It drips, endlessly, and never gets lighter."),
            B("forge_apron", "Forgewright's Apron", "leather", A, 4, S(hp: 22, def: 3), "Scorched black. Sparks bite back.", X(thorns: 2)),
            B("frost_hauberk", "Frost Hauberk", "mail", A, 5, S(hp: 30, def: 4), "Rime fills the gaps between the rings."),
            B("thorn_mantle", "Thorn Mantle", "cloak", A, 6, S(hp: 20, def: 2), "Woven from briars that remember pain.", X(thorns: 4)),
            B("brass_greaves", "Brass Greaves", "boots", A, 7, S(hp: 16, def: 3, move: 0.2f), "Ticks softly with every step."),
            B("sand_wraps", "Sand Wraps", "leather", A, 8, S(hp: 22, def: 2, crit: 0.02f, move: 0.2f), "Linen from a priest who forgot to die."),
            B("mirror_plate", "Mirror Plate", "plate", A, 9, S(hp: 45, def: 6, move: -0.15f), "You see yourself in it. Older."),
            B("dusk_cowl", "Dusk Cowl", "helm", A, 10, S(hp: 30, def: 4, crit: 0.04f), "Worn by the Cantor's acolytes."),

            // ---- charms ----
            B("copper_ring", "Copper Ring", "ring", C, 1, S(atk: 1, crit: 0.03f), "Greens the finger, sharpens the eye."),
            B("ember_ring", "Ember Ring", "ringGem", C, 2, S(atk: 3, crit: 0.05f, critMult: 0.2f), "A spark that never went out."),
            B("warden_amulet", "Warden's Amulet", "amulet", C, 3, S(hp: 20, atk: 2, def: 1), "Worn by the crypt's keepers."),
            B("bone_talisman", "Bone Talisman", "talisman", C, 1, S(hp: 10, crit: 0.02f), "Knucklebones strung on gut."),
            B("spore_brooch", "Spore Brooch", "brooch", C, 2, S(hp: 12), "It breathes, very slowly.", X(lifesteal: 0.02f)),
            B("tide_pearl", "Tide Pearl", "ringGem", C, 3, S(def: 1, move: 0.2f), "Pulled from the Cistern's throat."),
            B("cinder_idol", "Cinder Idol", "idol", C, 4, S(atk: 3, critMult: 0.25f), "A little saint of slag."),
            B("rime_locket", "Rime Locket", "amulet", C, 5, S(def: 2), "Holds a single snowflake that never melts.", X(haste: 0.08f)),
            B("briar_band", "Briar Band", "ring", C, 6, S(hp: 10), "Cuts its wearer too.", X(thorns: 3)),
            B("cog_signet", "Cog Signet", "ring", C, 7, S(crit: 0.06f, move: 0.1f), "The teeth still turn."),
            B("scarab_charm", "Scarab Charm", "talisman", C, 8, S(hp: 10), "Smells money in the sand.", X(gold: 0.25f)),
            B("prism_brooch", "Prism Brooch", "brooch", C, 9, S(crit: 0.08f, critMult: 0.2f), "Splits light into seven griefs."),
            B("hollow_idol", "Hollow Idol", "idol", C, 10, S(atk: 5), "Something inside it is hungry.", X(lifesteal: 0.03f)),
            B("lucky_knucklebone", "Lucky Knucklebone", "talisman", C, 1, S(crit: 0.02f), "Always lands on its feet.", X(gold: 0.2f)),

            // ---- relics ----
            B("worn_hymnal", "Worn Hymnal", "tome", R, 1, S(), "Half the pages are prayers, half are warnings.", X(haste: 0.08f)),
            B("saints_skull", "Saint's Skull", "skull", R, 2, S(hp: 15, def: 1), "Its saint is still deciding whether to help."),
            B("dim_lantern", "Dim Lantern", "lantern", R, 1, S(crit: 0.03f), "Enough light to find coins by.", X(gold: 0.1f)),
            B("cracked_orb", "Cracked Orb", "orb", R, 2, S(atk: 2), "The crack whistles a note.", X(haste: 0.05f)),
            B("sand_glass", "Sand Glass", "hourglass", R, 3, S(move: 0.2f), "Runs a little fast.", X(haste: 0.06f)),
            B("rook_feather", "Rook Feather", "feather", R, 2, S(move: 0.3f), "Rooks never go underground. This one did."),
            B("handbell", "Handbell", "bell", R, 3, S(def: 2), "Rings when something is close.", X(thorns: 2)),
            B("stone_heart", "Stone Heart", "heart", R, 4, S(hp: 30, move: -0.1f), "Beats once a minute."),
            B("pauper_crown", "Pauper's Crown", "crown", R, 5, S(crit: 0.03f), "Tin and glass, and it still feels heavy.", X(gold: 0.35f)),
            B("vigil_candle", "Vigil Candle", "candle", R, 1, S(hp: 10), "Lit for the dead. They noticed.", X(lifesteal: 0.01f)),
            B("drowned_codex", "Drowned Codex", "tome", R, 6, S(atk: 2), "The ink runs, but the words stay.", X(haste: 0.12f)),
            B("ember_heart", "Ember Heart", "heart", R, 7, S(hp: 15, atk: 4), "Still beating, still burning."),
            B("frozen_tear", "Frozen Tear", "orb", R, 8, S(def: 2, crit: 0.05f), "Wept by the Pale Matriarch."),
            B("choir_bell", "Choir Bell", "bell", R, 9, S(), "One of the bells that rang the Sun down.", X(haste: 0.1f, thorns: 3)),
        };

        public static readonly IReadOnlyList<ItemBase> Legendaries = new[]
        {
            // B1 Sunken Crypt
            L("wardens_oath", "The Warden's Oath", "greatsword", W, 1, S(atk: 12, critMult: 0.3f), X(lifesteal: 0.04f), "Pried from the Bone Warden's grip. It still wants to guard something."),
            L("ossuary_aegis", "Ossuary Aegis", "plate", A, 1, S(hp: 40, def: 6), X(thorns: 4), "Plates of fused rib and iron."),
            L("first_verse", "The First Verse", "tome", R, 1, S(atk: 2), X(haste: 0.2f), "The opening line of the hymn that buried the Sun."),
            L("gravelight", "Gravelight", "lantern", C, 1, S(crit: 0.08f), X(gold: 0.3f), "A lantern that only shines on the dead's treasure."),
            // B2 Mycelial Hollow
            L("morels_kiss", "Morel's Kiss", "dagger", W, 2, S(atk: 7, crit: 0.12f), X(lifesteal: 0.05f), "Mother Morel's paring knife. It feeds whoever holds it."),
            L("sporeweave", "Sporeweave Shroud", "cloak", A, 2, S(hp: 35, def: 3, move: 0.2f), X(lifesteal: 0.02f), "Alive, warm, and quietly growing."),
            L("rot_hymnal", "Hymnal of Rot", "tome", R, 2, S(hp: 20), X(haste: 0.15f, lifesteal: 0.02f), "The second verse, written in mould."),
            L("puffball_idol", "Puffball Idol", "idol", C, 2, S(hp: 25, def: 2), X(thorns: 5), "Squeeze it and the room coughs."),
            // B3 Drowned Cistern
            L("tidecallers_fork", "Tidecaller's Fork", "spear", W, 3, S(atk: 11, move: 0.25f), X(haste: 0.1f), "Pulls the tide in. Pulls you forward."),
            L("barnacle_hauberk", "Barnacle Hauberk", "mail", A, 3, S(hp: 45, def: 5), X(thorns: 6), "The barnacles bite anyone who leans close."),
            L("drowned_bell", "The Drowned Bell", "bell", R, 3, S(def: 3), X(haste: 0.18f), "It rings underwater. You can hear it in your teeth."),
            L("eelshade_ring", "Eelshade Ring", "ringGem", C, 3, S(crit: 0.1f, move: 0.3f), X(), "Slippery as its namesake."),
            // B4 Ember Forge
            L("saints_anvil", "The Saint's Anvil", "hammer", W, 4, S(hp: 20, atk: 15, def: 2, move: -0.2f), X(thorns: 4), "The Furnace Saint's hammer, forged on itself."),
            L("slag_plate", "Slag Plate", "plate", A, 4, S(hp: 55, def: 7, move: -0.2f), X(), "Poured on in one piece. Takes a week to cool."),
            L("cinder_heart", "Cinder Heart", "heart", R, 4, S(atk: 6, critMult: 0.4f), X(), "The third verse, burning in a chest of iron."),
            L("forgefire_signet", "Forgefire Signet", "ring", C, 4, S(atk: 5, crit: 0.08f), X(lifesteal: 0.03f), "Leaves a brand on every hand it touches."),
            // B5 Frostbone Caverns
            L("matriarchs_hush", "The Matriarch's Hush", "staff", W, 5, S(atk: 12, crit: 0.06f), X(haste: 0.2f), "A staff of ice that swallows sound."),
            L("rimeguard", "Rimeguard", "helm", A, 5, S(hp: 40, def: 6, crit: 0.04f), X(), "Frost grows over it to fit your face."),
            L("silent_verse", "The Silent Verse", "orb", R, 5, S(def: 3), X(haste: 0.25f), "The fifth verse is silence. It is the loudest one."),
            L("wolfs_tooth", "Wolf's Tooth", "talisman", C, 5, S(atk: 4, crit: 0.1f, move: 0.2f), X(), "Taken from the first rime wolf. It still growls."),
            // B6 Thornwild Ruins
            L("hierophants_crook", "Hierophant's Crook", "scythe", W, 6, S(atk: 14, critMult: 0.3f), X(lifesteal: 0.04f), "Its curve was grown, not forged."),
            L("bramble_mantle", "Bramble Mantle", "cloak", A, 6, S(hp: 40, def: 4), X(thorns: 10), "Hug it and regret it."),
            L("seed_of_growth", "Seed of Growth", "heart", R, 6, S(hp: 60), X(lifesteal: 0.02f), "The sixth verse, waiting to sprout."),
            L("dryad_brooch", "Dryad Brooch", "brooch", C, 6, S(hp: 20, move: 0.3f), X(haste: 0.1f), "Smells of rain on old stone."),
            // B7 Clockwork Ossuary
            L("horologists_hand", "The Horologist's Hand", "crossbow", W, 7, S(atk: 13, crit: 0.1f), X(haste: 0.12f), "Fires on the tick. Reloads on the tock."),
            L("escapement_plate", "Escapement Plate", "plate", A, 7, S(hp: 50, def: 7, move: 0.1f), X(), "Gears inside turn your stumbles into strides."),
            L("stopped_hourglass", "The Stopped Hourglass", "hourglass", R, 7, S(move: 0.3f), X(haste: 0.3f), "The seventh verse measured time. Now time is stuck."),
            L("mainspring", "Mainspring Pendant", "amulet", C, 7, S(atk: 4, crit: 0.06f, move: 0.25f), X(), "Wound tight. Never unwinds."),
            // B8 Sandsunk Necropolis
            L("pharaohs_flail", "Pharaoh's Flail", "flail", W, 8, S(atk: 16, critMult: 0.35f), X(gold: 0.4f), "The Dune Pharaoh's scepter of judgement."),
            L("burial_wraps", "Royal Burial Wraps", "leather", A, 8, S(hp: 50, def: 5), X(lifesteal: 0.03f), "Linen soaked in nine centuries of patience."),
            L("memory_crown", "Crown of Memory", "crown", R, 8, S(crit: 0.08f), X(gold: 0.6f), "The eighth verse remembers every coin ever buried."),
            L("golden_scarab", "Golden Scarab", "idol", C, 8, S(hp: 25, atk: 4), X(gold: 0.5f), "It pushes your fortune ahead of you."),
            // B9 Glass Abyss
            L("mirror_edge", "The Mirror's Edge", "longsword", W, 9, S(atk: 17, crit: 0.12f, critMult: 0.3f), X(), "Cuts both the foe and their reflection."),
            L("prism_aegis", "Prism Aegis", "plate", A, 9, S(hp: 60, def: 8), X(thorns: 8), "Every blow is scattered into colours."),
            L("reflecting_eye", "The Reflecting Eye", "orb", R, 9, S(crit: 0.1f), X(haste: 0.2f), "The ninth verse watches you back."),
            L("shard_of_choir", "Shard of the Choir", "ringGem", C, 9, S(atk: 6, crit: 0.08f), X(lifesteal: 0.04f), "A splinter of the Mirrored Choir's voice."),
            // B10 Heart of Dusk
            L("dawnbreaker", "Dawnbreaker", "greatsword", W, 10, S(atk: 22, crit: 0.08f, critMult: 0.4f), X(lifesteal: 0.05f), "Forged for the day the Sun rises again."),
            L("cantors_vestment", "The Cantor's Vestment", "robe", A, 10, S(hp: 70, def: 8, crit: 0.05f), X(haste: 0.1f), "Embroidered with the whole hymn, backwards."),
            L("last_verse", "The Last Verse", "tome", R, 10, S(atk: 6, hp: 30), X(haste: 0.3f), "Sing it, and the burial ends."),
            L("sunseed", "Sunseed", "candle", C, 10, S(hp: 40, atk: 6, def: 3, crit: 0.06f), X(lifesteal: 0.03f, gold: 0.3f), "A single ember of the buried Sun. It is warm."),
        };

        public static readonly IReadOnlyList<AffixDef> Affixes = new[]
        {
            new AffixDef("keen", "Keen", true, S(crit: 0.06f), X()),
            new AffixDef("brutal", "Brutal", true, S(atk: 4), X()),
            new AffixDef("vicious", "Vicious", true, S(critMult: 0.3f), X()),
            new AffixDef("sturdy", "Sturdy", true, S(def: 3), X()),
            new AffixDef("hale", "Hale", true, S(hp: 16), X()),
            new AffixDef("swift", "Swift", true, S(move: 0.35f), X()),
            new AffixDef("vampiric", "Vampiric", true, S(), X(lifesteal: 0.04f)),
            new AffixDef("thorned", "Thorned", true, S(), X(thorns: 6)),
            new AffixDef("hymnal", "Hymnal", true, S(), X(haste: 0.12f)),
            new AffixDef("gilded", "Gilded", true, S(), X(gold: 0.25f)),
            new AffixDef("fierce", "Fierce", true, S(atk: 2, crit: 0.03f), X()),
            new AffixDef("bulwark", "Bulwark", true, S(hp: 8, def: 2), X()),
            new AffixDef("ember", "of the Ember", false, S(atk: 3, critMult: 0.15f), X()),
            new AffixDef("bones", "of Bones", false, S(hp: 20), X()),
            new AffixDef("tide", "of the Tide", false, S(def: 2, move: 0.2f), X()),
            new AffixDef("silence", "of Silence", false, S(), X(haste: 0.15f)),
            new AffixDef("hunt", "of the Hunt", false, S(crit: 0.08f), X()),
            new AffixDef("oath", "of the Oath", false, S(def: 4), X()),
            new AffixDef("plenty", "of Plenty", false, S(), X(gold: 0.3f)),
            new AffixDef("leeching", "of Leeching", false, S(), X(lifesteal: 0.05f)),
            new AffixDef("thorns", "of Thorns", false, S(), X(thorns: 8)),
            new AffixDef("giant", "of the Giant", false, S(hp: 25, move: -0.1f), X()),
            new AffixDef("fury", "of Fury", false, S(atk: 5), X()),
            new AffixDef("dawn", "of the Dawn", false, S(hp: 6, atk: 1, def: 1, crit: 0.02f), X()),
        };

        static Dictionary<string, ItemBase> byId;
        static Dictionary<string, AffixDef> affixById;

        /// <summary>Every identity in compendium order: bases by slot, then legendaries by floor.</summary>
        public static IEnumerable<ItemBase> All => Bases.Concat(Legendaries);

        public static int Count => Bases.Count + Legendaries.Count;

        public static ItemBase Get(string id)
        {
            byId ??= All.ToDictionary(b => b.Id);
            return id != null && byId.TryGetValue(id, out var b) ? b : null;
        }

        public static AffixDef Affix(string id)
        {
            affixById ??= Affixes.ToDictionary(a => a.Id);
            return id != null && affixById.TryGetValue(id, out var a) ? a : null;
        }

        public static IEnumerable<string> IconKeys => All.Select(b => b.Icon).Distinct();
    }
}
