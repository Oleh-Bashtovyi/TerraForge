using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class InputLineTextAttribute(int maxLength = 50) : Attribute
{
    public int MaxLength { get; } = Math.Max(maxLength, 0);
}