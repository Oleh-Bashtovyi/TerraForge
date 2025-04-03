using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Extensions;

namespace TerrainGenerationApp.Data;


public class TreesData
{
    private Dictionary<string, bool[,]> _layers = new();

    public int LayersHeight
    {
        get
        {
            if (_layers.Count == 0)
                return 0;
            return _layers.First().Value.Height();
        }
    }

    public int LayersWidth
    {
        get
        {
            if (_layers.Count == 0)
                return 0;
            return _layers.First().Value.Width();
        }
    }

    public TreesData(Dictionary<string, bool[,]> layers)
    {
        SetLayers(layers);
    }

    public TreesData()
    {
        _layers = new Dictionary<string, bool[,]>();
    }

    public Vector2I GetLayersSize()
    {
        return new Vector2I(LayersWidth, LayersHeight);
    }


    public void SetLayers(Dictionary<string, bool[,]> layers)
    {
        if (!layers.Any())
        {
            _layers.Clear();
            return;
        }

        var height = layers.First().Value.Height();
        var width = layers.First().Value.Width();
        var newDict = new Dictionary<string, bool[,]>();

        foreach (var layer in layers)
        {
            if (layer.Value.Height() != height || layer.Value.Width() != width)
            {
                throw new ArgumentException("All layers must have the same size");
            }

            newDict[layer.Key] = layer.Value;
        }

        _layers = newDict;
    }

    public void SetLayers(List<TreesLayer> layers)
    {
        if (!layers.Any())
        {
            _layers.Clear();
            return;
        }

        var height = layers.First().TreesMap.Height();
        var width = layers.First().TreesMap.Width();
        var newDict = new Dictionary<string, bool[,]>();

        foreach (var layer in layers)
        {
            if (layer.TreesMap.Height() != height || layer.TreesMap.Width() != width)
            {
                throw new ArgumentException("All layers must have the same size");
            }
            newDict[layer.TreeId] = layer.TreesMap;
        }
        _layers = newDict;

    }


    public void ClearLayers()
    {
        _layers.Clear();
    }


    public void AddLayer(string layerName, bool[,] layer)
    {
        if (_layers.ContainsKey(layerName))
        {
            throw new ArgumentException($"Layer with name {layerName} already exists");
        }

        if (_layers.Count == 0)
        {
            _layers[layerName] = layer;
            return;
        }

        if (layer.Height() != LayersHeight || layer.Width() != LayersWidth)
        {
            throw new ArgumentException($"All layers must have the same size. " +
                                        $"Expected: {LayersHeight}x{LayersWidth}, Actual: {layer.Height()}x{layer.Width()}");
        }

        _layers[layerName] = layer;
    }


    public bool[,] GetLayerMap(string layerName)
    {
        if (_layers.TryGetValue(layerName, out bool[,] value))
        {
            return value;
        }

        throw new ArgumentException($"Layer with name {layerName} does not exist");
    }

    public IEnumerable<Vector2I> GetPoints(string layerName)
    {
        var map = GetLayerMap(layerName);

        for (int y = 0; y < map.Height(); y++)
        {
            for (int x = 0; x < map.Width(); x++)
            {
                if (map[y, x])
                {
                    yield return new Vector2I(x, y);
                }
            }
        }
    }

    public List<TreesLayer> GetLayers()
    {
        return _layers
            .Select(kvp => new TreesLayer(kvp.Key, kvp.Value))
            .ToList();
    }

public List<string> GetLayersIds()
    {
        return _layers.Keys.ToList();
    }
}
