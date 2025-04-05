using Godot;
using System.Collections.Generic;
using System.Reflection;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

namespace TerrainGenerationApp.Scenes.BuildingBlocks;

public static class InputLineManager
{
    public static List<InputLine> CreateInputLinesForObject<T>(T obj, Node parent, string category = null) where T : class
    {
        var inputLines = new List<InputLine>();

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Check if it has LineInputValueAttribute
            var inputValueAttr = property.GetCustomAttribute<LineInputValueAttribute>();

            if (inputValueAttr == null)
                continue;

            if (!string.IsNullOrEmpty(category))
            {
                if (inputValueAttr.Category != category)
                {
                    continue;
                }
            }

            // Create new instance of InputLine
            var inputLine = CreateInputLineForProperty(obj, property);

            if (inputLine != null)
            {
                parent.AddChild(inputLine);
                inputLines.Add(inputLine);
            }
        }

        return inputLines;
    }


    private static InputLine CreateInputLineForProperty<T>(T obj, PropertyInfo property) where T : class
    {
        var inputLine = GD.Load<PackedScene>("res://Scenes/BuildingBlocks/InputLine.tscn").Instantiate<InputLine>();

        // Get all property attributes
        var inputValueAttr = property.GetCustomAttribute<LineInputValueAttribute>();
        var roundedAttr = property.GetCustomAttribute<RoundedAttribute>();
        var stepAttr = property.GetCustomAttribute<StepAttribute>();
        var rangeAttr = property.GetCustomAttribute<InputRangeAttribute>();
        var textFormatAttr = property.GetCustomAttribute<TextFormatAttribute>();


        // Set description
        string description = string.IsNullOrEmpty(inputValueAttr!.Description) ? property.Name : inputValueAttr.Description;
        inputLine.SetDescription(description);

        // Set tooltip if specified
        if (!string.IsNullOrEmpty(inputValueAttr.Tooltip))
        {
            inputLine.TooltipText = inputValueAttr.Tooltip;
        }

        // Set type (integer or float)
        if (roundedAttr != null)
        {
            if (roundedAttr.IsRounded || property.PropertyType == typeof(int))
            {
                inputLine.SetAsInt();
            }
            else
            {
                inputLine.SetAsFloat();
            }
        }
        else
        {
            // By default, determine the type by property
            if (property.PropertyType == typeof(int))
            {
                inputLine.SetAsInt();
            }
            else
            {
                inputLine.SetAsFloat();
            }
        }

        // Set step
        if (stepAttr != null)
        {
            inputLine.SetStep(stepAttr.StepValue);
        }

        // Set range
        if (rangeAttr != null)
        {
            inputLine.SetRange(rangeAttr.MinValue, rangeAttr.MaxValue);
        }

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
        };

        return inputLine;
    }
}
