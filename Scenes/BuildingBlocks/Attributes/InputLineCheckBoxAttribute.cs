using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

public enum CheckboxType
{
    CheckButton,
    CheckBox
}

[AttributeUsage(AttributeTargets.Property)]
public class InputLineCheckBoxAttribute(CheckboxType checkboxType = CheckboxType.CheckButton) : Attribute
{
    public CheckboxType CheckboxType { get; } = checkboxType;
}