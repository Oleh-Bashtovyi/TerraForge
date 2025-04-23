using System.Collections.Generic;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

public class LastUsedInputLineValueTracker
{
    private readonly Dictionary<string, object> _lastUsedValues = new();
    private readonly Dictionary<string, object> _currentValues = new();
    private readonly InputLineValueTracker _currentValueTracker;

    public IInputLineValueTracker ValueTracker => _currentValueTracker;

    public LastUsedInputLineValueTracker()
    {
        _currentValueTracker = new InputLineValueTracker(_currentValues);
    }

    public void UpdateLastUsedValues()
    {
        _lastUsedValues.Clear();
        foreach (var pair in _currentValues)
        {
            _lastUsedValues[pair.Key] = pair.Value;
        }
    }

    public IReadOnlyDictionary<string, object> GetLastUsedValues()
    {
        return _lastUsedValues;
    }

    public IReadOnlyDictionary<string, object> GetCurrentValues()
    {
        return _currentValues;
    }
}