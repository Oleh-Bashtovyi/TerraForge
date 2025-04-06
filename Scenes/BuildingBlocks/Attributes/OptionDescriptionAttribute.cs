using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class OptionDescriptionAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}