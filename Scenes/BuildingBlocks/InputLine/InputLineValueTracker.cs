using System.Collections.Generic;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

public class InputLineValueTracker(Dictionary<string, object> lastUsedValues, Dictionary<string, object> currentValues) : IInputLineValueTracker
{
    public void TrackValue(IInputLine inputLine)
    {
        if (inputLine?.Id != null)
        {
            TrackValue(inputLine.Id, inputLine.GetValueAsObject());
        }
    }

    public void TrackValue(string id, object value)
    {
        if (lastUsedValues.TryGetValue(id, out var lastValue))
        {
            if (Equals(lastValue, value))
            {
                return; 
            }
        }

        currentValues[id] = value;
    }
}