using System.Collections.Generic;
using System.Linq;

namespace Oathdeep.Data
{
    /// <summary>How a creature fights; the editor picks the AI component and its tuning from this.</summary>
    public enum Archetype { Melee, Tank, Shooter, Turret, Charger, Hopper, Caster, Bomber, Boss }

    /// <summary>One codex entry: identity, home floor, lore and base stats (floor-1 strength; floors scale them).</summary>
    public sealed class BestiaryEntry
    {
        public string Id { get; }
        public string Name { get; }
        public int Floor { get; }
        public Archetype Archetype { get; }
        public string Lore { get; }
        public StatBlock Stats { get; }

        public bool IsBoss => Archetype == Archetype.Boss;

        public BestiaryEntry(string id, string name, int floor, Archetype archetype, string lore, StatBlock stats)
        {
            Id = id; Name = name; Floor = floor; Archetype = archetype; Lore = lore; Stats = stats;
        }
    }

    /// <summary>All forty creatures of the descent: three regulars and one Chorister per floor, in codex order.</summary>
    public static class Bestiary
    {
        static StatBlock S(int hp, int atk, int def, float speed, float crit = 0.04f) => new StatBlock(hp, atk, def, crit, 1.5f, speed);

        static readonly StatBlock Small = S(18, 6, 0, 3.0f);
        static readonly StatBlock Grunt = S(34, 7, 2, 2.4f);
        static readonly StatBlock Heavy = S(70, 9, 4, 1.6f);
        static readonly StatBlock Ranged = S(24, 6, 0, 2.3f, 0.05f);
        static readonly StatBlock Hunter = S(30, 8, 1, 2.8f, 0.06f);
        static readonly StatBlock Mystic = S(26, 7, 0, 2.2f);
        static readonly StatBlock Chorister = S(560, 15, 8, 2.1f, 0.05f);

        static BestiaryEntry E(string id, string name, int floor, Archetype a, StatBlock stats, string lore) =>
            new BestiaryEntry(id, name, floor, a, lore, stats);

        public static readonly IReadOnlyList<BestiaryEntry> All = new[]
        {
            // B1 Sunken Crypt — the Verse of Bone
            E("skeleton", "Skeleton", 1, Archetype.Melee, S(34, 7, 2, 2.4f),
              "The crypt's first dead, still walking their old patrol. They do not remember why they guard, only that they must."),
            E("archer", "Skeleton Archer", 1, Archetype.Shooter, S(24, 6, 0, 2.3f, 0.05f),
              "Bowmen of a vigil nobody relieved. Their arrows are carved from the ribs of those who came before you."),
            E("slime", "Bog Slime", 1, Archetype.Hopper, S(30, 6, 0, 2f),
              "Grave-water that learned to crawl. Cut it and it simply becomes two smaller problems."),
            E("bone_warden", "The Bone Warden", 1, Archetype.Boss, S(520, 15, 8, 2.1f, 0.05f),
              "First of the Choristers. He holds the Verse of Bone in his ribcage and counts every corpse in the crypt, every night."),

            // B2 Mycelial Hollow — the Verse of Rot
            E("sporeling", "Sporeling", 2, Archetype.Bomber, Small,
              "A walking puffball that has never once been patient. When it reaches you, it bursts into a cloud of very bad ideas."),
            E("mire_toad", "Mire Toad", 2, Archetype.Hopper, Hunter,
              "Fat on spores and certain of nothing. It leaps first and wonders later."),
            E("fungal_shambler", "Fungal Shambler", 2, Archetype.Tank, Heavy,
              "Somebody lay down in the Hollow to rest. Mother Morel let them keep walking."),
            E("mother_morel", "Mother Morel", 2, Archetype.Boss, Chorister,
              "Second Chorister, keeper of the Verse of Rot. She feeds the Hollow, and the Hollow feeds her, and nobody leaves."),

            // B3 Drowned Cistern — the Verse of Tide
            E("brine_crab", "Brine Crab", 3, Archetype.Charger, Hunter,
              "Its shell is crusted with the coins of drowned sailors. It charges sideways, which is somehow worse."),
            E("drowned_sailor", "Drowned Sailor", 3, Archetype.Melee, Grunt,
              "Crews of ships that sailed down instead of out. They still keep watch for a shore."),
            E("eelshade", "Eelshade", 3, Archetype.Shooter, Ranged,
              "A shadow that swims through air as if it were water, spitting cold brine at anything warm."),
            E("tidecaller", "The Tidecaller", 3, Archetype.Boss, Chorister,
              "Third Chorister. She drowned a whole sea to keep the Verse of Tide from drying out. Hold your breath, little light."),

            // B4 Ember Forge — the Verse of Flame
            E("cinder_imp", "Cinder Imp", 4, Archetype.Shooter, Ranged,
              "Sparks from the Saint's anvil that grew legs and a grudge. They throw little pieces of themselves."),
            E("slag_golem", "Slag Golem", 4, Archetype.Tank, Heavy,
              "Waste from a thousand failed chains, poured into the shape of a guard."),
            E("forge_sentry", "Forge Sentry", 4, Archetype.Turret, Ranged,
              "A brass watchman bolted to the floor. It cannot chase you. It does not need to."),
            E("furnace_saint", "The Furnace Saint", 4, Archetype.Boss, Chorister,
              "Fourth Chorister. He forges chains for a sun and never once asked whether she wanted them."),

            // B5 Frostbone Caverns — the Verse of Silence
            E("frost_wisp", "Frost Wisp", 5, Archetype.Caster, Mystic,
              "The last breath of someone who froze mid-word. It finishes the sentence on top of you."),
            E("rime_wolf", "Rime Wolf", 5, Archetype.Charger, Hunter,
              "Hunts in perfect silence. You will hear the snow crunch only after it has already passed you."),
            E("ice_revenant", "Ice Revenant", 5, Archetype.Melee, Grunt,
              "Pilgrims who stopped to rest in the cold. The cold kept them."),
            E("pale_matriarch", "The Pale Matriarch", 5, Archetype.Boss, Chorister,
              "Fifth Chorister. She keeps the Verse of Silence so deep that you can hear your own blood begging you to turn back."),

            // B6 Thornwild Ruins — the Verse of Growth
            E("briar_crawler", "Briar Crawler", 6, Archetype.Melee, Hunter,
              "A knot of thorns that learned which way warm things walk."),
            E("thorn_dryad", "Thorn Dryad", 6, Archetype.Shooter, Ranged,
              "She was a garden once. Now she throws what is left of the roses."),
            E("root_horror", "Root Horror", 6, Archetype.Caster, Heavy,
              "Older than the ruins it holds together. When it shifts, the ground erupts beneath you."),
            E("verdant_hierophant", "The Verdant Hierophant", 6, Archetype.Boss, Chorister,
              "Sixth Chorister. Growth is only hunger with patience, and he taught the roots everything he knew."),

            // B7 Clockwork Ossuary — the Verse of Measure
            E("gear_beetle", "Gear Beetle", 7, Archetype.Charger, Hunter,
              "A wind-up scarab that runs until it hits something. Usually you."),
            E("brass_sentinel", "Brass Sentinel", 7, Archetype.Tank, Heavy,
              "Built to stand at a door for a thousand years. It has nine hundred to go and is in no mood for visitors."),
            E("tick_bomb", "Tick Bomb", 7, Archetype.Bomber, Small,
              "It ticks. Then it doesn't. Be elsewhere."),
            E("horologist", "The Horologist", 7, Archetype.Boss, Chorister,
              "Seventh Chorister. He has counted every second the Sun has been buried and is very proud of the number."),

            // B8 Sandsunk Necropolis — the Verse of Memory
            E("sand_wraith", "Sand Wraith", 8, Archetype.Caster, Mystic,
              "A ghost made of everything the desert forgot. It pulls the floor out from under you."),
            E("scarab_swarm", "Scarab Swarm", 8, Archetype.Hopper, Small,
              "A thousand little jewels with a thousand little appetites."),
            E("mummified_priest", "Mummified Priest", 8, Archetype.Shooter, Ranged,
              "Still preaching to an empty temple, still cursing those who will not listen."),
            E("dune_pharaoh", "The Dune Pharaoh", 8, Archetype.Boss, Chorister,
              "Eighth Chorister. He remembers every coin ever buried, and he is still counting yours."),

            // B9 Glass Abyss — the Verse of Reflection
            E("shard_sprite", "Shard Sprite", 9, Archetype.Shooter, Small,
              "A splinter of a broken mirror, flitting about with your face in it."),
            E("mirror_knight", "Mirror Knight", 9, Archetype.Melee, Grunt,
              "It fights exactly as you do, a half-step late. Do not let it catch up."),
            E("prism_eye", "Prism Eye", 9, Archetype.Turret, Ranged,
              "An eye that watches from every angle at once and splits its glare into a fan of light."),
            E("mirrored_choir", "The Mirrored Choir", 9, Archetype.Boss, Chorister,
              "Ninth Chorister: nine voices, three bodies, one truth. Only one of them is real. Strike the one that sings."),

            // B10 Heart of Dusk — the Verse of Hunger
            E("hollow_acolyte", "Hollow Acolyte", 10, Archetype.Caster, Mystic,
              "The Cantor's last faithful. They gave him their names so he could sing louder."),
            E("dusk_hound", "Dusk Hound", 10, Archetype.Charger, Hunter,
              "Bred from the dark between stars. It has your scent, and it has had it since you were born."),
            E("hunger_maw", "Hunger Maw", 10, Archetype.Melee, S(45, 9, 2, 2.6f),
              "A mouth that wandered off from the thing in the sky. It is still looking for something to fill it."),
            E("the_cantor", "The Cantor", 10, Archetype.Boss, Chorister,
              "Last of the Choir of Dawn. He never sang the hymn to save the world. He sang it to keep her for himself."),
        };

        public static BestiaryEntry Get(string id) => All.FirstOrDefault(e => e.Id == id);

        public static IEnumerable<BestiaryEntry> OnFloor(int floor) => All.Where(e => e.Floor == floor);

        public static BestiaryEntry BossOf(int floor) => All.FirstOrDefault(e => e.Floor == floor && e.IsBoss);
    }
}
