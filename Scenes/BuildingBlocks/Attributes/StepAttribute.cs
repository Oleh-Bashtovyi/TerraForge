using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class StepAttribute(float step) : Attribute
{
    public float StepValue { get; } = step;
}