using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class InputOptionAttribute(string text, int id = -1) : Attribute
{
    public string Text { get; } = text;
    public int Id { get; } = id;
}