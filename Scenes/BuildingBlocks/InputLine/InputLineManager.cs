using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.LoadedScenes;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

public static class InputLineManager
{
    public static List<InputLine.BaseInputLine> CreateInputLinesForObject<T>
        (T obj, Node container, string category = null) where T : class
    {
        var inputLines = new List<InputLine.BaseInputLine>();

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

/*        if (!string.IsNullOrEmpty(category))
        {
            var categoryTitle = InputLineScenes.OptionsCategoryTitleScene.Instantiate<OptionsCategoryTitle>();
            categoryTitle.SetTitle(category);
            container.AddChild(categoryTitle);
        }*/

        foreach (var property in properties)
        {
            // Check if it has LineInputValueAttribute
            var inputLineAttr = property.GetCustomAttribute<InputLineAttribute>();
            var inputLineSliderAttr = property.GetCustomAttribute<InputLineSliderAttribute>();
            var inputLineComboboxAttr = property.GetCustomAttribute<InputLineComboboxAttribute>();

            if (inputLineAttr == null && inputLineSliderAttr == null)
                continue;

            if (!string.IsNullOrEmpty(category))
            {
                if (inputLineAttr == null || inputLineAttr.Category != category)
                {
                    continue;
                }
            }

            InputLine.BaseInputLine inputLine = null;

            if (property.PropertyType == typeof(bool))
            {
                inputLine = CreateInputLineCheckboxForProperty(obj, property);
            }
            else if (inputLineSliderAttr != null)
            {
                inputLine = CreateInputLineSliderForProperty(obj, property);
            }
            else if (inputLineComboboxAttr != null)
            {
                GD.Print($"Creating combobox for {property.Name}");
                inputLine = CreateInputLineComboboxForProperty(obj, property);
            }
            if (inputLine != null)
            {
                inputLine.SetId(property.Name);
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
        var textFormatAttr = property.GetCustomAttribute<InputLineTextFormatAttribute>();

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
        if (textFormatAttr != null)
        {
            inputLine.SetTextFormat(textFormatAttr.Format);
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






    private static InputLineCombobox CreateInputLineComboboxForProperty<T>(T obj, PropertyInfo property) where T : class
    {
        var inputLine = BuildingBlockLoadedScenes.INPUT_LINE_COMBOBOX.Instantiate<InputLine.InputLineCombobox>();
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

        // Set current value
        if (inputLine.GetOptionsCount() > 0)
        {
            inputLine.SetSelected(0);
        }

        // Subscribe to option change
        inputLine.OnOptionChanged += (args) =>
        {
            if (inputLineComboboxAttr!.Bind == ComboboxBind.Label)
            {
                if (property.PropertyType != typeof(string))
                {
                    throw new ArgumentException($"Property {property.Name} is not a string type.");
                }

                property.SetValue(obj, args.Label);
            }
            else if (inputLineComboboxAttr.Bind == ComboboxBind.Id)
            {
                if (property.PropertyType != typeof(int))
                {
                    throw new ArgumentException($"Property {property.Name} is not an int type.");
                }
                property.SetValue(obj, args.Id);
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

        return inputLine;
    }






    private static InputLine.InputLineCheckbox CreateInputLineCheckboxForProperty<T>(T obj, PropertyInfo property) where T : class
    {
        if (property.PropertyType != typeof(bool))
        {
            throw new ArgumentException($"Property {property.Name} is not an bool type.");
        }

        var inputLine = BuildingBlockLoadedScenes.INPUT_LINE_CHECKBOX.Instantiate<InputLine.InputLineCheckbox>();
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
