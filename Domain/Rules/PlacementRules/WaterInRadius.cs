using Godot;
using TerrainGenerationApp.Domain.Core;
namespace TerrainGenerationApp.Domain.Rules.PlacementRules;

public class WaterInRadius(float radius) : IPlacementRule
{
    private readonly float _radius = radius;

    public string Description => $"Water in radius {_radius}";

    public bool CanPlaceIn(Vector2 pos, IWorldData worldData)
    {
        // If no terrain data is available, allow placement
        if (worldData?.TerrainData == null)
            return true;

        // Apply scale factor to handle different map sizes
        var scaleFactor = GetScaleFactor(worldData);
        var scaledPos = pos * scaleFactor;

        // Calculate the search radius in grid cells
        int searchRadius = Mathf.CeilToInt(_radius * scaleFactor);

        // Get terrain dimensions
        int terrainWidth = worldData.TerrainData.TerrainMapWidth;
        int terrainHeight = worldData.TerrainData.TerrainMapHeight;

        // Convert position to grid coordinates
        int posX = Mathf.FloorToInt(scaledPos.X);
        int posY = Mathf.FloorToInt(scaledPos.Y);

        // Search in a square around the position
        for (int y = Mathf.Max(0, posY - searchRadius); y <= Mathf.Min(terrainHeight - 1, posY + searchRadius); y++)
        {
            for (int x = Mathf.Max(0, posX - searchRadius); x <= Mathf.Min(terrainWidth - 1, posX + searchRadius); x++)
            {
                // Check if the point is within the circular radius
                float distanceSquared = (x - scaledPos.X) * (x - scaledPos.X) + (y - scaledPos.Y) * (y - scaledPos.Y);
                if (distanceSquared > searchRadius * searchRadius)
                    continue;

                // Convert grid coordinates back to terrain space for depth calculation
                float terrainX = x / scaleFactor;
                float terrainY = y / scaleFactor;

                // Check if there is water at this position
                float depth = worldData.DepthAt(terrainX, terrainY);

                // If depth is positive, there is water at this position
                if (depth > 0)
                {
                    // Found water within the radius, rule is satisfied
                    return true;
                }
            }
        }

        // No water found within the radius
        return false;
    }

    private float GetScaleFactor(IWorldData worldData)
    {
        return 1.0f / worldData.TreesData.PlacementFrequency;
    }
}