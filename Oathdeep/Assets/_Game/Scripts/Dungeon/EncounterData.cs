using System.Collections.Generic;
using Oathdeep.Data;
using Oathdeep.Loot;
using UnityEngine;

namespace Oathdeep.Dungeon
{
    /// <summary>A sunstone boon: a run-long stat and/or special bonus, optionally with a full heal.</summary>
    public sealed class Boon
    {
        public string Name { get; }
        public string Text { get; }
        public StatBlock Stats { get; }
        public ItemSpecials Specials { get; }
        public bool FullHeal { get; }

        public Boon(string name, string text, StatBlock stats, ItemSpecials specials, bool fullHeal = false)
        {
            Name = name; Text = text; Stats = stats; Specials = specials; FullHeal = fullHeal;
        }
    }

    /// <summary>Static content for the peaceful rooms: sunstone boons, pilgrim words, event wording.</summary>
    public static class EncounterData
    {
        public static readonly IReadOnlyList<Boon> Boons = new[]
        {
            new Boon("Warmth", "Its warmth closes every wound.", default, default, true),
            new Boon("Radiance", "+3 Attack for the rest of the descent.", new StatBlock(0, 3, 0, 0f, 0f, 0f), default),
            new Boon("Fortitude", "+25 Health for the rest of the descent.", new StatBlock(25, 0, 0, 0f, 0f, 0f), default, true),
            new Boon("Swiftness", "+0.4 Speed for the rest of the descent.", new StatBlock(0, 0, 0, 0f, 0f, 0.4f), default),
            new Boon("Clarity", "+15% Skill haste for the rest of the descent.", default, new ItemSpecials(0f, 0.15f, 0f, 0)),
            new Boon("Fortune", "+40% Gold find for the rest of the descent.", default, new ItemSpecials(0f, 0f, 0.4f, 0)),
            new Boon("Keen Eye", "+7% Crit chance for the rest of the descent.", new StatBlock(0, 0, 0, 0.07f, 0f, 0f), default),
            new Boon("Bulwark", "+3 Defense for the rest of the descent.", new StatBlock(0, 0, 3, 0f, 0f, 0f), default),
            new Boon("Thirst", "+4% Lifesteal for the rest of the descent.", default, new ItemSpecials(0.04f, 0f, 0f, 0)),
        };

        /// <summary>Three different boons, seeded.</summary>
        public static List<Boon> DrawBoons(System.Random rng, int count = 3)
        {
            var pool = new List<Boon>(Boons);
            var picked = new List<Boon>();
            while (picked.Count < count && pool.Count > 0)
            {
                int i = rng.Next(pool.Count);
                picked.Add(pool[i]);
                pool.RemoveAt(i);
            }
            return picked;
        }

        static readonly string[][] PilgrimLines =
        {
            new[] { "The dead here were buried facing up. They wanted to see her rise.",
                    "Mind the Warden. He guards the first verse like a dog guards a bone it cannot eat." },
            new[] { "Mother Morel feeds the floor, and the floor feeds her. Don't sit down.",
                    "The spores sing a little. If you hum back, they stop. Mostly." },
            new[] { "The Tidecaller drowned a whole sea to hold one verse. Imagine the patience.",
                    "Water remembers the light. That is why it glints, even down here." },
            new[] { "The Saint forges chains for a sun. He never once asked whether she wanted them.",
                    "Heat rises. So will she." },
            new[] { "The Pale Matriarch keeps her verse in a silence so deep you can hear your own blood.",
                    "Cold doesn't kill the light. It only hides it." },
            new[] { "Growth is just hunger with patience. The Hierophant taught the roots that.",
                    "Something sweet blooms near the sunstone. Don't eat it." },
            new[] { "The Horologist counts every second the Sun has been buried. He's proud of the number.",
                    "Listen for the tick that comes late. That is where the doors are." },
            new[] { "The Pharaoh remembers every coin ever buried. He's still counting yours.",
                    "Sand is only stone that gave up. Don't give up." },
            new[] { "In the Abyss your reflection walks a step behind you. Don't let it catch up.",
                    "The Mirrored Choir sings in nine voices. Only one of them is real." },
            new[] { "The Cantor never sang to save us. He sang to keep her for himself.",
                    "This is the bottom of the world. She is so close you can feel her warmth." },
        };

        public static string PilgrimLine(int floor, System.Random rng)
        {
            var lines = PilgrimLines[Mathf.Clamp(floor, 1, PilgrimLines.Length) - 1];
            return lines[rng.Next(lines.Length)];
        }

        public static string BiomeName(int floor)
        {
            string[] names = { "Sunken Crypt", "Mycelial Hollow", "Drowned Cistern", "Ember Forge", "Frostbone Caverns",
                               "Thornwild Ruins", "Clockwork Ossuary", "Sandsunk Necropolis", "Glass Abyss", "Heart of Dusk" };
            return names[Mathf.Clamp(floor, 1, names.Length) - 1];
        }
    }
}
