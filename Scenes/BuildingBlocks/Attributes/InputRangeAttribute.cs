using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class InputRangeAttribute(float minValue, float maxValue) : Attribute
{
    public float MinValue { get; } = minValue;
    public float MaxValue { get; } = maxValue;
}
