using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class LineInputValueAttribute : Attribute
{
    public string Description { get; set; } = string.Empty;
    public string Tooltip { get; set; } = string.Empty;
    public string Category { get; set; }
}