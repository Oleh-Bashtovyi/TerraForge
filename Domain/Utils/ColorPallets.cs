using Godot;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TerrainGenerationApp.Domain.Utils;

public static class ColorPallets
{
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

                { 1.0f, new Color("dedede") } // whitish (mountain peaks)
        });


    public static readonly IReadOnlyDictionary<float, Color> MoistTerrainColors = new ReadOnlyDictionary<float, Color>(
        new Dictionary<float, Color>()
        {
            { 0f, new Color("e6c275") },     
            { 0.04f, new Color("e3b55b") },  
            { 0.08f, new Color("d9a743") },  
            { 0.21f, new Color("bd8c2c") },  
            { 0.33f, new Color("9e7325") },  
            { 0.48f, new Color("8c7e6d") },  
            { 0.57f, new Color("bfb8ad") },  
            { 1.0f, new Color("dedede") }    
        });


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
