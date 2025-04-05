using Godot;
using System;
using System.Collections.Generic;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Utils;

namespace TerrainGenerationApp.Domain.Visualization;

public class TreeVisualizationSettings
{
    private readonly Logger<TerrainVisualizationSettings> _logger = new();
    private readonly Dictionary<string, Color> _treesLayersColors = new();
    private readonly Dictionary<string, PackedScene> _treesLayersScenes = new();

    public static readonly Color DefaultTreeColor = Colors.Red;

    public TreeVisualizationSettings()
    {

    }

    public void RedrawTreesImage(Image image, IWorldData worldData)
    {
        var imageSize = image.GetSize();
        var mapSize = worldData.TreesData.GetLayersSize();

        if (mapSize != imageSize)
        {
            _logger.LogError($"Image size {imageSize} does not match map size {mapSize}");
            throw new ArgumentException($"Trees image size {imageSize} does not match actual layers map size: {mapSize}");
        }

        var treeMaps = worldData.TreesData.GetLayers();
        var h = worldData.TreesData.LayersHeight;
        var w = worldData.TreesData.LayersWidth;

        foreach (var item in treeMaps)
        {
            var map = item.TreesMap;
            var id = item.TreeId;
            var treesColor = GetTreesLayerColor(id);
            var treesCount = 0;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (map[y, x])
                    {
                        treesCount++;
                        image.SetPixel(x, y, treesColor);
                        if (y > 0) image.SetPixel(x, y - 1, treesColor);
                        if (x > 0) image.SetPixel(x - 1, y, treesColor);
                        if (x < w - 1) image.SetPixel(x + 1, y, treesColor);
                        if (y < h - 1) image.SetPixel(x, y + 1, treesColor);
                    }
                }
            }

            if (treesCount == 0)
            {
                _logger.Log($"Tree layer: {id} has ZERO trees");
            }
            else
            {
                _logger.Log($"Tree layer: {id}, trees count: {treesCount}, color: {treesColor}");
            }
        }
    }


    public Color GetTreesLayerColor(string treeLayerId)
    {
        return _treesLayersColors.GetValueOrDefault(treeLayerId, DefaultTreeColor);
    }

    public PackedScene? GetTreesLayerScene(string treeLayerId)
    {
        return _treesLayersScenes.GetValueOrDefault(treeLayerId, null);
    }

    public void ClearTreesLayersColors()
    {
        _treesLayersColors.Clear();
    }

    public void ClearTreesLayersScenes()
    {
        _treesLayersScenes.Clear();
    }
}