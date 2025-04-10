using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

public class InputLineVectorXAttribute
{
    public string Description { get; } = string.Empty;
    public float Min { get; } = -1000;
    public float Max { get; } = 1000;
    public float Step { get; } = 1.0f;
    public bool Round { get; } = false;
    public string Format { get; } = "0.##";
}

public class InputLineVectorYAttribute
{
    public string Description { get; } = string.Empty;
    public float Min { get; } = -1000;
    public float Max { get; } = 1000;
    public float Step { get; } = 1.0f;
    public bool Round { get; } = false;
    public string Format { get; } = "0.##";
}



public class SomeAttribute : Attribute
{

}

public class SomeOtherAttribute : Attribute
{
    public int SomeField { get; }
}
