using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class TextFormatAttribute(string format) : Attribute
{
    public string Format { get; } = format;
}
