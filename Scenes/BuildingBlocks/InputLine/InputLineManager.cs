using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.LoadedScenes;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;


/// <summary>
/// What a horrible code. This is a mess. Maybe I should refactor it.
/// </summary>
public static class InputLineManager
{
    public static List<BaseInputLine> CreateInputLinesForObject<T>
        (T obj, Node container, string category = null) where T : class
    {
        var inputLines = new List<InputLine.BaseInputLine>();

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var inputLineAttr = property.GetCustomAttribute<InputLineAttribute>();

            if (inputLineAttr == null)
                continue;

            // Populate inputs for specific category
            if (!string.IsNullOrEmpty(category) && inputLineAttr.Category != category)
            {
                continue;
            }

            BaseInputLine inputLine = null;

            var inputLineSliderAttr = property.GetCustomAttribute<InputLineSliderAttribute>();
            var inputLineComboboxAttr = property.GetCustomAttribute<InputLineComboboxAttribute>();
            var inputLineTextAttr = property.GetCustomAttribute<InputLineTextAttribute>();
            var inputLineCheckboxAttr = property.GetCustomAttribute<InputLineCheckBoxAttribute>();

            if (inputLineSliderAttr != null)
            {
                inputLine = CreateInputLineSliderForProperty(obj, property);
            }
            else if (inputLineComboboxAttr != null)
            {
                inputLine = CreateInputLineComboboxForProperty(obj, property);
            }
            else if (inputLineTextAttr != null)
            {
                inputLine = CreateInputLineTextForProperty(obj, property);
            }
            else if (inputLineCheckboxAttr != null)
            {
                inputLine = CreateInputLineCheckboxForProperty(obj, property);
            }

            if (inputLine != null)
            {
                if (!string.IsNullOrEmpty(inputLineAttr?.Id))
                {
                    inputLine.SetId(inputLineAttr.Id);
                }
                else
                {
                    inputLine.SetId(property.Name);
                }
                inputLine.Visible = inputLineAttr.IsVisible;
                container.AddChild(inputLine);
                inputLines.Add(inputLine);
            }
        }

        return inputLines;
    }


    private static InputLine.InputLineSlider CreateInputLineSliderForProperty<T>(T obj, PropertyInfo property) where T : class
    {
        var inputLine = BuildingBlockLoadedScenes.INPUT_LINE_SLIDER.Instantiate<InputLine.InputLineSlider>();

        // Get all property attributes
        var inputLineAttr = property.GetCustomAttribute<InputLineAttribute>();
        var inputLineSliderAttr = property.GetCustomAttribute<InputLineSliderAttribute>();

        if (inputLineSliderAttr == null)
        {
            throw new ArgumentException($"InputLineSliderAttribute is not specified for property {property.Name}");
        }

        // Set description
        string description = string.IsNullOrEmpty(inputLineAttr!.Description) ? property.Name : inputLineAttr.Description;
        inputLine.SetDescription(description);

        // Set tooltip if specified
        if (!string.IsNullOrEmpty(inputLineAttr.Tooltip))
        {
            inputLine.TooltipText = inputLineAttr.Tooltip;
        }

        if (property.PropertyType == typeof(int))
        {
            inputLine.SetAsInt();
        }
        else
        {
            inputLine.SetRounded(inputLineSliderAttr.Round);
        }

        // Set step
        inputLine.SetStep(inputLineSliderAttr.Step);

        // Set range
        inputLine.SetRange(inputLineSliderAttr.Min, inputLineSliderAttr.Max);

        // Set text format
        if (!string.IsNullOrEmpty(inputLineSliderAttr.Format))
        {
            inputLine.SetTextFormat(inputLineSliderAttr.Format);
        }

        // Set current value
        object propertyValue = property.GetValue(obj);
        float value = 0;

        if (propertyValue != null)
        {
            if (propertyValue is int intValue)
            {
                value = intValue;
            }
            else if (propertyValue is float floatValue)
            {
                value = floatValue;
            }
            else if (propertyValue is double doubleValue)
            {
                value = (float)doubleValue;
            }
            else if (propertyValue is string stringValue)
            {
                var v = float.TryParse(stringValue, out float parsedValue);
                value = parsedValue;
            }
        }
        inputLine.SetValue(value);

        // Subscribe to value change
        inputLine.OnValueChanged += (newValue) =>
        {
            if (property.PropertyType == typeof(int))
            {
                property.SetValue(obj, (int)newValue);
            }
            else if (property.PropertyType == typeof(float))
            {
                property.SetValue(obj, newValue);
            }
            else if (property.PropertyType == typeof(double))
            {
                property.SetValue(obj, (double)newValue);
            }
            else if (property.PropertyType == typeof(string))
            {
                property.SetValue(obj, newValue.ToString());
            }
        };

        return inputLine;
    }



    private static InputLineText CreateInputLineTextForProperty<T>(T obj, PropertyInfo property) where T : class
    {
        var inputLine = BuildingBlockLoadedScenes.INPUT_LINE_TEXT.Instantiate<InputLineText>();
        var inputLineAttr = property.GetCustomAttribute<InputLineAttribute>();
        var inputLineTextAttr = property.GetCustomAttribute<InputLineTextAttribute>();

        // Set description
        string description = string.IsNullOrEmpty(inputLineAttr!.Description) ? property.Name : inputLineAttr.Description;
        inputLine.SetDescription(description);

        // Set tooltip if specified
        if (!string.IsNullOrEmpty(inputLineAttr.Tooltip))
        {
            inputLine.TooltipText = inputLineAttr.Tooltip;
        }

        inputLine.SetTextLength(inputLineTextAttr!.MaxLength);

        // Set current value
        object propertyValue = property.GetValue(obj);
        string value = string.Empty;
        if (propertyValue != null)
        {
            if (propertyValue is string stringValue)
            {
                value = stringValue;
            }
            else
            {
                value = propertyValue.ToString();
            }
        }
        inputLine.SetText(value);

        // Subscribe to value change
        inputLine.OnTextChanged += (newValue) =>
        {
            if (property.PropertyType == typeof(int))
            {
                if (!int.TryParse(newValue, out int intValue))
                {
                    inputLine.MarkError();
                    return;
                }
                inputLine.UnmarkError();
                property.SetValue(obj, intValue);
            }
            else if (property.PropertyType == typeof(float))
            {
                if (!float.TryParse(newValue, out float floatValue))
                {
                    inputLine.MarkError();
                    return;
                }
                inputLine.UnmarkError();
                property.SetValue(obj, floatValue);
            }
            else if (property.PropertyType == typeof(double))
            {
                if (!double.TryParse(newValue, out double doubleValue))
                {
                    inputLine.MarkError();
                    return;
                }
                inputLine.UnmarkError();
                property.SetValue(obj, doubleValue);
            }
            else if (property.PropertyType == typeof(bool))
            {
                if (!bool.TryParse(newValue, out bool boolValue))
                {
                    inputLine.MarkError();
                    return;
                }
                inputLine.UnmarkError();
                property.SetValue(obj, boolValue);
            }
            else if (property.PropertyType == typeof(string))
            {
                property.SetValue(obj, newValue);
                inputLine.UnmarkError();
            }
            else
            {
                throw new ArgumentException($"Property {property.Name} is not a supported type for InputLineText.");
            }
        };

        return inputLine;
    }



    private static InputLineCombobox CreateInputLineComboboxForProperty<T>(T obj, PropertyInfo property) where T : class
    {
        var inputLine = BuildingBlockLoadedScenes.INPUT_LINE_COMBOBOX.Instantiate<InputLineCombobox>();
        var inputLineAttr = property.GetCustomAttribute<InputLineAttribute>();
        var inputLineComboboxAttr = property.GetCustomAttribute<InputLineComboboxAttribute>();

        // Set description
        string description = string.IsNullOrEmpty(inputLineAttr!.Description) ? property.Name : inputLineAttr.Description;
        inputLine.SetDescription(description);

        // Set tooltip if specified
        if (!string.IsNullOrEmpty(inputLineAttr.Tooltip))
        {
            inputLine.TooltipText = inputLineAttr.Tooltip;
        }

        var options = property.GetCustomAttributes<InputOptionAttribute>();

        foreach (var option in options)
        {
            GD.Print($"Adding option {option.Text} with id {option.Id}");
            inputLine.AddOption(option.Text, option.Id);    
        }

        var propertyType = property.PropertyType;

        // Subscribe to option change
        inputLine.OnOptionChanged += (args) =>
        {
            if (inputLineComboboxAttr!.Bind == ComboboxBind.Label)
            {
                if (propertyType != typeof(string))
                {
                    throw new ArgumentException($"Property {property.Name} is not a string type.");
                }

                property.SetValue(obj, args.Label);
            }
            else if (inputLineComboboxAttr.Bind == ComboboxBind.Id)
            {
                if (propertyType.IsEnum)
                {
                    GD.Print($"Property {property.Name} is an enum type. Trying to set value");

                    var enumType = property.PropertyType;
                    var enumValue = Enum.ToObject(enumType, args.Id);
                    if (!Enum.IsDefined(propertyType, enumValue))
                    {
                        GD.PrintErr($"Value {enumValue} is not defined in {property.PropertyType.Name}.");
                        throw new ArgumentOutOfRangeException(nameof(enumValue), $"Value {enumValue} is not defined in {property.PropertyType.Name}.");
                    }
                    property.SetValue(obj, enumValue);
                }
                else if (propertyType != typeof(int))
                {
                    throw new ArgumentException($"Property {property.Name} is not an int type.");
                }
                else
                {
                    property.SetValue(obj, args.Id);
                }

            }
            else if (inputLineComboboxAttr.Bind == ComboboxBind.Index)
            {
                if (property.PropertyType != typeof(int))
                {
                    throw new ArgumentException($"Property {property.Name} is not an int type.");
                }

                property.SetValue(obj, args.Index);
            }
        };

        // Set current value
        if (inputLineComboboxAttr!.Selected > -1)
        {
            GD.Print($"Selecting {inputLineComboboxAttr.Selected} to {property.Name}");
            inputLine.SetSelected(inputLineComboboxAttr.Selected);
        }

        return inputLine;
    }






    private static InputLineCheckbox CreateInputLineCheckboxForProperty<T>(T obj, PropertyInfo property) where T : class
    {
        if (property.PropertyType != typeof(bool))
        {
            throw new ArgumentException($"Property {property.Name} is not an bool type.");
        }

        var inputLine = BuildingBlockLoadedScenes.INPUT_LINE_CHECKBOX.Instantiate<InputLineCheckbox>();
        var inputLineAttr = property.GetCustomAttribute<InputLineAttribute>();

        // Set description
        string description = string.IsNullOrEmpty(inputLineAttr!.Description) ? property.Name : inputLineAttr.Description;
        inputLine.SetDescription(description);

        // Set tooltip if specified
        if (!string.IsNullOrEmpty(inputLineAttr.Tooltip))
        {
            inputLine.TooltipText = inputLineAttr.Tooltip;
        }

        // Set current value
        object propertyValue = property.GetValue(obj);
        bool value = false;
        if (propertyValue != null)
        {
            if (propertyValue is bool boolValue)
            {
                value = boolValue;
            }
        }

        inputLine.SetValue(value);

        // Subscribe to value change
        inputLine.OnValueChanged += (newValue) =>
        {
            property.SetValue(obj, newValue);
        };

        return inputLine;
    }
}
