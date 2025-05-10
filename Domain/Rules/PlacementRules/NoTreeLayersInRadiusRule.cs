using Godot;
using System.Collections.Generic;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Extensions;

namespace TerrainGenerationApp.Domain.Rules.PlacementRules;

public class NoTreeLayersInRadiusRule(List<string> treeLayerNames, float radius) : IPlacementRule
{
    public NoTreeLayersInRadiusRule(float radius) : this([], radius)
    {
    }

    public string Description => $"No trees from layers [{string.Join(", ", treeLayerNames)}] in radius {radius}";

    public bool CanPlaceIn(Vector2 pos, IWorldData worldData)
    {
        // If no tree data available or specified layers don't exist, allow placement
        if (!worldData.TreesData.HasLayers())
            return true;

        var treesData = worldData.TreesData;
        var otherLayers = new List<bool[,]>();

        foreach (var treeLayer in treeLayerNames)
        {
            var map = treesData.GetLayerMapByName(treeLayer);
            
            if (map != null)
            {
                otherLayers.Add(map);
            }
        }


        var scaleFactor = GetScaleFactor(worldData);
        var scaledPos = pos * scaleFactor;

        // Convert position to grid coordinates
        int posX = Mathf.FloorToInt(scaledPos.X);
        int posY = Mathf.FloorToInt(scaledPos.Y);

        // Calculate the search radius in grid cells
        int searchRadius = Mathf.CeilToInt(radius * scaleFactor);

        // Width and height of tree layers
        int width = worldData.TreesData.LayersWidth;
        int height = worldData.TreesData.LayersHeight;

        // Search in a square around the position
        for (int y = Mathf.Max(0, posY - searchRadius); y <= Mathf.Min(height - 1, posY + searchRadius); y++)
        {
            for (int x = Mathf.Max(0, posX - searchRadius); x <= Mathf.Min(width - 1, posX + searchRadius); x++)
            {
                // Check if the point is within the circular radius
                float distanceSquared = (x - scaledPos.X) * (x - scaledPos.X) + (y - scaledPos.Y) * (y - scaledPos.Y);
                if (distanceSquared > searchRadius * searchRadius)
                    continue;

                // Check if any tree from the specified layers exists at this point
                foreach (var layer in otherLayers)
                {
                    if (y < layer.Height() && x < layer.Width() && layer[y, x])
                    {
                        // Found a tree from the specified layer within the radius
                        return false;
                    }
                }
            }
        }

        // No trees from the specified layers found within the radius
        return true;
    }

    private float GetScaleFactor(IWorldData worldData)
    {
        // Calculate scale factor between terrain size and tree layers size
        // This handles cases where tree maps might be at a different scale than terrain maps
        if (!worldData.TreesData.HasLayers())
            return 1.0f;

        float scaleX = (float)worldData.TreesData.LayersWidth / worldData.TerrainData.TerrainMapWidth;
        float scaleY = (float)worldData.TreesData.LayersHeight / worldData.TerrainData.TerrainMapHeight;

        // Use the average of X and Y scales (they should be the same in most cases)
        return (scaleX + scaleY) / 2.0f;
    }
}