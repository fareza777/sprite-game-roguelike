using System.Collections.Generic;

namespace Oathdeep.Enemies
{
    public enum BossMove { Radial, AimedFan, Spiral, Charge, Slam, Rain, Summon }

    /// <summary>One attack in a boss's repertoire. Weight is how often it is picked; Phase2Only moves unlock at half health.</summary>
    public readonly struct BossMoveSpec
    {
        public readonly BossMove Move;
        public readonly int Count;          // projectiles, blasts or minions
        public readonly float Speed;        // projectile or dash speed
        public readonly float Spread;       // fan degrees, or spiral turn speed (deg/s)
        public readonly float Radius;       // slam or blast radius
        public readonly float Windup;
        public readonly float Weight;
        public readonly bool Phase2Only;

        public BossMoveSpec(BossMove move, int count, float speed, float spread, float radius, float windup, float weight, bool phase2Only = false)
        {
            Move = move; Count = count; Speed = speed; Spread = spread; Radius = radius; Windup = windup; Weight = weight; Phase2Only = phase2Only;
        }
    }

    /// <summary>Each Chorister's fight, written as data. The Cantor gets everything plus a third, frantic phase.</summary>
    public static class BossPatterns
    {
        static BossMoveSpec M(BossMove m, int count, float speed, float spread, float radius, float windup, float weight, bool p2 = false) =>
            new BossMoveSpec(m, count, speed, spread, radius, windup, weight, p2);

        static readonly Dictionary<string, BossMoveSpec[]> Table = new Dictionary<string, BossMoveSpec[]>
        {
            // Mother Morel: spore rings, sporeling broods, poison rain
            ["mother_morel"] = new[]
            {
                M(BossMove.Radial, 12, 5f, 0f, 0f, 0.8f, 3f),
                M(BossMove.Summon, 3, 0f, 0f, 0f, 0.9f, 2f),
                M(BossMove.Rain, 4, 0f, 0f, 1.3f, 0.8f, 2f),
                M(BossMove.Radial, 20, 4.5f, 0f, 0f, 0.7f, 2f, true),
            },
            // Tidecaller: aimed water volleys, tidal charge, crab summons
            ["tidecaller"] = new[]
            {
                M(BossMove.AimedFan, 5, 8f, 40f, 0f, 0.7f, 3f),
                M(BossMove.Charge, 1, 12f, 0f, 0f, 0.8f, 2f),
                M(BossMove.Summon, 2, 0f, 0f, 0f, 0.9f, 1.5f),
                M(BossMove.Spiral, 28, 5.5f, 220f, 0f, 0.6f, 2f, true),
            },
            // Furnace Saint: slams that shower embers, fire rain
            ["furnace_saint"] = new[]
            {
                M(BossMove.Slam, 10, 5.5f, 0f, 2.4f, 0.9f, 3f),
                M(BossMove.Rain, 5, 0f, 0f, 1.4f, 0.9f, 2.5f),
                M(BossMove.Charge, 1, 10f, 0f, 0f, 0.9f, 1.5f),
                M(BossMove.Rain, 9, 0f, 0f, 1.2f, 0.8f, 2f, true),
            },
            // Pale Matriarch: ice fans, frost rings, wisp servants
            ["pale_matriarch"] = new[]
            {
                M(BossMove.AimedFan, 7, 7f, 60f, 0f, 0.7f, 3f),
                M(BossMove.Radial, 16, 4.5f, 0f, 0f, 0.8f, 2f),
                M(BossMove.Summon, 2, 0f, 0f, 0f, 1f, 1.5f),
                M(BossMove.Spiral, 32, 4.5f, 260f, 0f, 0.6f, 2f, true),
            },
            // Verdant Hierophant: root eruptions, thorn rings, briar broods
            ["verdant_hierophant"] = new[]
            {
                M(BossMove.Rain, 6, 0f, 0f, 1.3f, 0.9f, 3f),
                M(BossMove.Slam, 14, 6f, 0f, 2.6f, 1f, 2f),
                M(BossMove.Summon, 3, 0f, 0f, 0f, 0.9f, 1.5f),
                M(BossMove.AimedFan, 9, 7.5f, 90f, 0f, 0.6f, 2f, true),
            },
            // Horologist: clockwork spirals, ticking rings of gears
            ["horologist"] = new[]
            {
                M(BossMove.Spiral, 36, 5f, 200f, 0f, 0.7f, 3f),
                M(BossMove.Radial, 12, 6f, 0f, 0f, 0.6f, 2f),
                M(BossMove.Summon, 2, 0f, 0f, 0f, 0.9f, 1.5f),
                M(BossMove.Spiral, 48, 5.5f, -260f, 0f, 0.5f, 2.5f, true),
            },
            // Dune Pharaoh: hammer slams, sand blade fans, scarab plagues
            ["dune_pharaoh"] = new[]
            {
                M(BossMove.Slam, 12, 6f, 0f, 2.6f, 0.9f, 3f),
                M(BossMove.AimedFan, 5, 9f, 50f, 0f, 0.6f, 2.5f),
                M(BossMove.Summon, 4, 0f, 0f, 0f, 1f, 1.5f),
                M(BossMove.Charge, 1, 12f, 0f, 0f, 0.7f, 2f, true),
            },
            // Mirrored Choir: glass volleys from every direction
            ["mirrored_choir"] = new[]
            {
                M(BossMove.AimedFan, 9, 8f, 80f, 0f, 0.6f, 3f),
                M(BossMove.Radial, 18, 6f, 0f, 0f, 0.7f, 2.5f),
                M(BossMove.Rain, 5, 0f, 0f, 1.2f, 0.8f, 2f),
                M(BossMove.Spiral, 40, 6f, 300f, 0f, 0.5f, 2.5f, true),
            },
            // The Cantor: every verse at once
            ["the_cantor"] = new[]
            {
                M(BossMove.AimedFan, 7, 8f, 60f, 0f, 0.6f, 2.5f),
                M(BossMove.Radial, 20, 5.5f, 0f, 0f, 0.7f, 2f),
                M(BossMove.Rain, 6, 0f, 0f, 1.3f, 0.8f, 2f),
                M(BossMove.Charge, 1, 13f, 0f, 0f, 0.7f, 1.5f),
                M(BossMove.Summon, 3, 0f, 0f, 0f, 0.9f, 1.2f),
                M(BossMove.Spiral, 44, 6f, 280f, 0f, 0.5f, 2.5f, true),
                M(BossMove.Slam, 16, 6.5f, 0f, 2.8f, 0.8f, 2f, true),
            },
        };

        public static IReadOnlyList<BossMoveSpec> For(string id) => id != null && Table.TryGetValue(id, out var m) ? m : Table["the_cantor"];

        public static IEnumerable<string> Bosses => Table.Keys;

        /// <summary>Weighted pick among the moves allowed in the current phase.</summary>
        public static BossMoveSpec Pick(IReadOnlyList<BossMoveSpec> moves, bool phaseTwo, System.Random rng, BossMove? avoid = null)
        {
            float total = 0f;
            foreach (var m in moves) if (Allowed(m, phaseTwo, avoid)) total += m.Weight;
            double roll = rng.NextDouble() * total;
            foreach (var m in moves)
            {
                if (!Allowed(m, phaseTwo, avoid)) continue;
                roll -= m.Weight;
                if (roll <= 0) return m;
            }
            return moves[0];
        }

        static bool Allowed(BossMoveSpec m, bool phaseTwo, BossMove? avoid) => (!m.Phase2Only || phaseTwo) && (avoid == null || m.Move != avoid.Value);
    }
}
