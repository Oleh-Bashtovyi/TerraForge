using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class InputLineAttribute : Attribute
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Tooltip { get; set; } = string.Empty;
}