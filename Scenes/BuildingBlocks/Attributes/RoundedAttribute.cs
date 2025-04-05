using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class RoundedAttribute : Attribute
{
    public bool IsRounded { get; set; } = true;
}