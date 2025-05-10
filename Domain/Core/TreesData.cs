using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Domain.Extensions;

namespace TerrainGenerationApp.Domain.Core;

public class TreesData
{
    private Dictionary<string, TreesLayer> _layers = new();

    public int LayersHeight
    {
        get
        {
            if (_layers.Count == 0)
                return 0;
            return _layers.First().Value.TreesMap.Height();
        }
    }

    public int LayersWidth
    {
        get
        {
            if (_layers.Count == 0)
                return 0;
            return _layers.First().Value.TreesMap.Width();
        }
    }

    public TreesData(Dictionary<string, bool[,]> layers)
    {
        SetLayers(layers);
    }

    public TreesData()
    {
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
        var newDict = new Dictionary<string, TreesLayer>();

        foreach (var dictLayer in layers)
        {
            if (dictLayer.Value.Height() != height || dictLayer.Value.Width() != width)
            {
                throw new ArgumentException("All layers must have the same size");
            }

            var layer = new TreesLayer(dictLayer.Key, dictLayer.Value);
            newDict[dictLayer.Key] = layer;
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
        var newDict = new Dictionary<string, TreesLayer>();

        foreach (var layer in layers)
        {
            if (layer.TreesMap.Height() != height || layer.TreesMap.Width() != width)
            {
                throw new ArgumentException("All layers must have the same size");
            }
            newDict[layer.TreeId] = layer;
        }
        _layers = newDict;

    }

    public void ClearLayers()
    {
        _layers.Clear();
    }

    public bool HasLayers()
    {
        return _layers.Count > 0;
    }

    public bool HasLayerWithId(string layerName)
    {
        return _layers.ContainsKey(layerName);
    }

    public bool HasLayerWithName(string layerName)
    {
        return _layers.Any(x => x.Value.LayerName == layerName);
    }

    public void AddLayer(string layerId, bool[,] layer, string? layerName = null)
    {
        if (_layers.ContainsKey(layerId))
        {
            throw new ArgumentException($"Layer with id {layerId} already exists");
        }

        if (_layers.Count == 0)
        {
            _layers[layerId] = new TreesLayer(layerId, layer, layerName);
            return;
        }

        if (layer.Height() != LayersHeight || layer.Width() != LayersWidth)
        {
            throw new ArgumentException($"All layers must have the same size. " +
                                        $"Expected: {LayersHeight}x{LayersWidth}, Actual: {layer.Height()}x{layer.Width()}");
        }

        _layers[layerId] = new TreesLayer(layerId, layer, layerName);
    }


    public bool[,] GetLayerMapById(string layerId)
    {
        if (_layers.TryGetValue(layerId, out TreesLayer value))
        {
            return value.TreesMap;
        }

        throw new ArgumentException($"Layer with id {layerId} does not exist");
    }

    public bool[,]? GetLayerMapByName(string layerName)
    {
        layerName = layerName.ToLower();
        return _layers.FirstOrDefault(x => x.Value.LayerName?.ToLower() == layerName).Value?.TreesMap;
    }

    public IEnumerable<Vector2I> GetPoints(string layerName)
    {
        var map = GetLayerMapById(layerName);

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
        return _layers.Select(x => x.Value).ToList();
    }

    public List<string> GetLayersIds()
    {
        return _layers.Keys.ToList();
    }
}
