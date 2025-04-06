using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class InputLineSliderAttribute(float min, float max, float step = 1.0f, bool round = false) : Attribute
{
    public float Min { get; } = min;
    public float Max { get; } = max;
    public float Step { get; } = step;
    public bool Round { get; } = round;
}
