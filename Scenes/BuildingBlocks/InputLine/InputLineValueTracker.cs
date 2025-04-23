using System.Collections.Generic;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

public class InputLineValueTracker(Dictionary<string, object> valuesDictionary) : IInputLineValueTracker
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
        valuesDictionary[id] = value;
    }
}