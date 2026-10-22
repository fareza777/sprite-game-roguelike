using Oathdeep.Combat;
using UnityEngine;

namespace Oathdeep.Dungeon
{
    /// <summary>
    /// What a combat room asks of the player. Waves: clear them. Survive: hold out until the timer runs down.
    /// Nests: break the nests that keep spawning enemies. Gauntlet: fewer foes, more hostile ground.
    /// </summary>
    public enum RoomChallenge { Waves, Survive, Nests, Gauntlet }

    /// <summary>Which art a biome's terrain hazard uses (sprites live in BossFx).</summary>
    public enum TerrainArt { Spores, Water, Lava, Ice, Roots, Bubble, Quicksand }

    /// <summary>A biome's standing ground hazard in combat rooms.</summary>
    public readonly struct Terrain
    {
        public readonly HazardKind Kind;
        public readonly float Strength;
        public readonly TerrainArt Art;

        public Terrain(HazardKind kind, float strength, TerrainArt art) { Kind = kind; Strength = strength; Art = art; }
    }

    public static class RoomChallenges
    {
        /// <summary>A room's challenge from a 0..1 roll. The first floor stays simple; plain waves remain the norm.</summary>
        public static RoomChallenge Pick(int floor, double roll)
        {
            if (floor <= 1) return roll < 0.8 ? RoomChallenge.Waves : RoomChallenge.Nests;
            if (roll < 0.55) return RoomChallenge.Waves;
            if (roll < 0.70) return RoomChallenge.Survive;
            if (roll < 0.85) return RoomChallenge.Nests;
            return RoomChallenge.Gauntlet;
        }

        public static float SurviveSeconds(int floor) => 20f + Mathf.Clamp(floor, 1, 10);

        /// <summary>The biome's terrain in combat rooms; the crypt has none (it keeps its spike traps).</summary>
        public static Terrain? TerrainFor(int floor) => floor switch
        {
            2 => new Terrain(HazardKind.Burn, 0f, TerrainArt.Spores),
            3 => new Terrain(HazardKind.Slow, 0.6f, TerrainArt.Water),
            4 => new Terrain(HazardKind.Burn, 0f, TerrainArt.Lava),
            5 => new Terrain(HazardKind.Ice, 0.08f, TerrainArt.Ice),
            6 => new Terrain(HazardKind.Slow, 0.55f, TerrainArt.Roots),
            7 => new Terrain(HazardKind.Slow, 0.6f, TerrainArt.Bubble),
            8 => new Terrain(HazardKind.Slow, 0.35f, TerrainArt.Quicksand),
            9 => new Terrain(HazardKind.Ice, 0.1f, TerrainArt.Ice),
            10 => new Terrain(HazardKind.Burn, 0f, TerrainArt.Lava),
            _ => null,
        };

        /// <summary>How many terrain patches a room lays: none on the first floor, a few more in a gauntlet.</summary>
        public static int TerrainPatches(int floor, RoomChallenge challenge) =>
            TerrainFor(floor) == null ? 0 : challenge == RoomChallenge.Gauntlet ? 5 : 2;
    }
}
