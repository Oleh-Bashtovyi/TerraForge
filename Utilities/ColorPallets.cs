using Godot;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TerrainGenerationApp.Utilities;

public static class ColorPallets
{
    public static readonly IReadOnlyDictionary<float, Color> DefaultTerrainColors = new ReadOnlyDictionary<float, Color>(
        new Dictionary<float, Color>()
        {
            { 0f, new Color("f2d16b") }, // light yellow sand
            { 0.0588f, new Color("d8c857") }, // yellow-green transition
            { 0.1765f, new Color("8cc751") }, // light green (lowland)
            { 0.3333f, new Color("66b546") }, // green
            { 0.4902f, new Color("399e2d") }, // deep green
            { 0.6275f, new Color("2d7039") }, // dark green (upland)
            { 0.7451f, new Color("8a9188") }, // grayish (mountain base)
            { 0.8627f, new Color("c9ccc8") }, // whitish (mountain peaks)
            { 1.0f, new Color("c9ccc8") } // whitish (mountain peaks)
        });

    public static readonly IReadOnlyDictionary<float, Color> DefaultWaterColors = new ReadOnlyDictionary<float, Color>(
        new Dictionary<float, Color>()
        {
            { 0f, new Color("b9fbf7") },       // turquoise-white (shallowest water)
            { 0.0980f, new Color("7deef3") },  // light turquoise
            { 0.1961f, new Color("42c9df") },  // turquoise
            { 0.3137f, new Color("2a94c8") },  // blue
            { 0.4706f, new Color("1d64aa") },  // dark blue
            { 0.6275f, new Color("214389") },  // deep blue
            { 0.7843f, new Color("372d7a") },  // violet-blue (deepest water)
            { 1.0f, new Color("372d7a") }   // violet-blue (deepest water)
        });



    //private Dictionary<float, Color> _defaultTerrainColors = new()
    //{
    //{ 0f / 255f, Colors.Yellow },
    //{ 15f / 255f, Colors.GreenYellow },
    //{ 45f / 255f, Colors.LawnGreen },
    //{ 105f / 255f, Colors.LimeGreen },
    //{ 130f / 255f, new Color("1cb32d") },
    //{ 150f / 255f, Colors.ForestGreen },
    //{ 170f / 255f, Colors.Gray },
    //{ 190f / 255f, Colors.WebGray },
    //{ 210f / 255f, Colors.DimGray },
    //{ 230f / 255f, Colors.White },
    //{ 255f / 255f, Colors.White }
    //};
    //
    //private Dictionary<float, Color> _defaultWaterColors = new()
    //{
    //{ 0f / 255f, Colors.PowderBlue },
    //{ 10f / 255f, Colors.LightSkyBlue },
    //{ 25f / 255f, Colors.DodgerBlue },
    //{ 40f / 255f, new Color("0083e0") },
    //{ 55f / 255f, new Color("006bb9") },
    //{ 75f / 255f, Colors.Blue },
    //{ 90f / 255f, Colors.MediumBlue },
    //{ 110f / 255f, Colors.DarkBlue },
    //{ 135f / 255f, Colors.MidnightBlue },
    //{ 155f / 255f, new Color("111461") },
    //{ 180f / 255f, new Color("0a0d49") },
    //{ 255f / 255f, new Color("0a0d49") }
    //};



    //private Dictionary<float, Color> _defaultTerrainColors = new()
    //{
    //{ 0f / 255f, new Color("f5e6a8") },      // світло-жовтий пісок
    //{ 15f / 255f, new Color("e8d68a") },      // жовтий пісок
    //{ 45f / 255f, new Color("b9d179") },      // жовто-зелений
    //{ 75f / 255f, new Color("8fbe63") },      // салатовий
    //{ 105f / 255f, new Color("5ca653") },     // яскраво-зелений
    //{ 130f / 255f, new Color("3d934a") },     // зелений
    //{ 150f / 255f, new Color("4d9a65") },     // світліший темно-зелений
    //{ 170f / 255f, new Color("8ba999") },     // світліший сіро-зелений (початок гір)
    //{ 190f / 255f, new Color("b8bfb6") },     // світліший сірий (гори)
    //{ 210f / 255f, new Color("cfd6d3") },     // білувато-сірий (вершини гір)
    //{ 230f / 255f, new Color("f2f2f2") },     // білий (вершини)
    //{ 255f / 255f, new Color("ffffff") }      // чисто-білий (піки)
    //};
    //
    //private Dictionary<float, Color> _defaultWaterColors = new()
    //{
    //{ 0f / 255f, new Color("b6f0f0") },      // бірюзово-білий (наймілкіша вода)
    //{ 10f / 255f, new Color("89e1e6") },      // світло-бірюзовий
    //{ 25f / 255f, new Color("66cee1") },      // бірюзовий
    //{ 40f / 255f, new Color("40b7d7") },      // світло-синій
    //{ 60f / 255f, new Color("279bcc") },      // синій
    //{ 80f / 255f, new Color("1a7dbe") },      // насичений синій
    //{ 100f / 255f, new Color("1466ad") },     // темно-синій
    //{ 130f / 255f, new Color("0f4a8a") },     // насичений темно-синій
    //{ 160f / 255f, new Color("0a3572") },     // синьо-фіолетовий
    //{ 190f / 255f, new Color("0a2058") },     // темний синьо-фіолетовий
    //{ 220f / 255f, new Color("08134a") },     // фіолетово-синій (найглибша вода)
    //{ 255f / 255f, new Color("08134a") }      // фіолетово-синій (найглибша вода)
    //};
}
