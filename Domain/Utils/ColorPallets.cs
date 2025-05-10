using Godot;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TerrainGenerationApp.Domain.Utils;

public static class ColorPallets
{
    /*    public static readonly IReadOnlyDictionary<float, Color> DefaultTerrainColors = new ReadOnlyDictionary<float, Color>(
            new Dictionary<float, Color>()
            {
                    { 0f, new Color("f2d16b") }, // light yellow sand
                    { 0.05f, new Color("d8c857") }, // yellow-green transition
                    { 0.1f, new Color("8cc751") }, // light green (lowland)
                    { 0.18f, new Color("66b546") }, // green
                    { 0.28f, new Color("399e2d") }, // deep green
                    { 0.4f, new Color("2d7039") }, // dark green (upland)
                    { 0.48f, new Color("8a9188") }, // grayish (mountain base)
                    { 0.6f, new Color("c9ccc8") }, // whitish (mountain peaks)
                    { 1.0f, new Color("c9ccc8") } // whitish (mountain peaks)
            });*/



    // THIS IS BEST
    public static readonly IReadOnlyDictionary<float, Color> DefaultTerrainColors = new ReadOnlyDictionary<float, Color>(
        new Dictionary<float, Color>()
        {
                { 0f, new Color("e9ce96") }, // yellow-green transition
                { 0.04f, new Color("e9d6a0") }, // light yellow sand
                { 0.08f, new Color("328c0f") }, // green
                { 0.21f, new Color("227f0a") }, // deep green
                { 0.33f, new Color("016500") }, // dark green (upland)
                { 0.48f, new Color("8a9188") }, // grayish (mountain base)
                { 0.57f, new Color("c9ccc8") }, // whitish (mountain peaks)

                { 1.0f, new Color("c9ccc8") } // whitish (mountain peaks)
        });


    public static readonly IReadOnlyDictionary<float, Color> MoistTerrainColors = new ReadOnlyDictionary<float, Color>(
        new Dictionary<float, Color>()
        {
            { 0f, new Color("c4b78a") },    // вологий пісок
            { 0.04f, new Color("c5b684") }, // вологий прибережний пісок
            { 0.08f, new Color("246b0a") }, // вологий зелений
            { 0.21f, new Color("196106") }, // темно-вологий зелений
            { 0.33f, new Color("014c00") }, // вологий темно-зелений
            { 0.48f, new Color("6a716c") }, // вологі скелі
            { 0.57f, new Color("a7aaa6") }, // вологі вершини
            { 1.0f, new Color("e6e6e6") }   // вологий сніг
        });




    /*    public static readonly IReadOnlyDictionary<float, Color> DefaultTerrainColors = new ReadOnlyDictionary<float, Color>(
            new Dictionary<float, Color>()
            {
                { 0f, new Color("f1d699") }, 
                { 0.05f, new Color("F2EE99") }, // yellow-green transition
                { 0.1f, new Color("8cc751") }, // light green (lowland)
                { 0.18f, new Color("66b546") }, // green
                { 0.28f, new Color("399e2d") }, // deep green
                { 0.4f, new Color("2d7039") }, // dark green (upland)
                { 0.48f, new Color("8a9188") }, // grayish (mountain base)
                { 0.6f, new Color("c9ccc8") }, // whitish (mountain peaks)
                { 1.0f, new Color("c9ccc8") } // whitish (mountain peaks)
            });
    */
    public static readonly IReadOnlyDictionary<float, Color> DefaultWaterColors = new ReadOnlyDictionary<float, Color>(
        new Dictionary<float, Color>()
        {
            { 0f, new Color("b9fbf7") },       // turquoise-white (shallowest water)
			{ 0.05f, new Color("7deef3") },  // light turquoise
			{ 0.1f, new Color("42c9df") },  // turquoise
			{ 0.16f, new Color("2a94c8") },  // blue
			{ 0.26f, new Color("1d64aa") },  // dark blue
			{ 0.38f, new Color("214389") },  // deep blue
			{ 0.5f, new Color("372d7a") },  // violet-blue (deepest water)
			{ 1.0f, new Color("372d7a") }   // violet-blue (deepest water)
		});
}
