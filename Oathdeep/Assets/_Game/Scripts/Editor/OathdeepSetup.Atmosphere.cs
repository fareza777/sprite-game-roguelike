using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Oathdeep.EditorTools
{
    /// <summary>Each floor's drifting air: ambient.png holds two 8x8 frames of mote per biome, in floor order.</summary>
    public static partial class OathdeepSetup
    {
        const string AmbientPng = ArtDir + "/VFX/ambient.png";

        static void ConfigureAmbientArt()
        {
            var cells = new List<Cell>();
            for (int b = 0; b < 10; b++)
                for (int f = 0; f < 2; f++)
                    cells.Add(new Cell($"ambient{b}_{f}", (b * 2 + f) * 8, 0, 8, 8, Centre));
            Slice(AmbientPng, cells, WorldPpu, SpriteMeshType.FullRect);
        }

        static Sprite[] AmbientFrames(int biomeIndex) =>
            Enumerable.Range(0, 2).Select(f => Spr(AmbientPng, $"ambient{biomeIndex}_{f}")).ToArray();
    }
}
