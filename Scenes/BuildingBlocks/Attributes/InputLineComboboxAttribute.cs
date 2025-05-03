using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

public enum ComboboxBind
{
    Index, Id, Label
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class InputLineComboboxAttribute(int selected = -1, ComboboxBind bind = ComboboxBind.Index) : Attribute
{
    public ComboboxBind Bind { get; } = bind;
    public int Selected { get; } = selected;
}
