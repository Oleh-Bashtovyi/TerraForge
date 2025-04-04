using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Data.Structure;
using TerrainGenerationApp.Extensions;
using TerrainGenerationApp.Rules.RadiusRules;

namespace TerrainGenerationApp.Generators.Trees;


public class TreesApplier : ITreesApplier
{
    //public record TreesLayer(string TreeId, bool[,] TreesMap);

    public List<TreesLayer> GenerateTreesLayers(IWorldData worldData, IEnumerable<TreePlacementRule> rules, float frequency = 1.0f)
    {
        var dict = new Dictionary<string, bool[,]>();

        foreach (var rule in rules)
        {
            if (rule.RadiusRule == null || rule.PlacementRule == null)
            {
                continue;
            }

            var id = rule.TreeId;

            if (dict.ContainsKey(id))
            {
                throw new ArgumentException($"Layer with name {id} already exists");
            }

            var trees = GenerateTreesLayer(worldData, rule.RadiusRule, frequency, 30);

            for (int y = 0; y < trees.Height(); y++)
            {
                for (int x = 0; x < trees.Width(); x++)
                {
                    if (trees[y, x])
                    {
                        var pos = new Vector2(x, y) / frequency;

                        if (!rule.CanPlace(pos, worldData))
                        {
                            trees[y, x] = false;
                        }
                    }
                }
            }

            dict[id] = trees;
        }

        return dict.Select(kvp => new TreesLayer(kvp.Key, kvp.Value)).ToList();
    }



    public bool[,] GenerateTreesLayer(
        IWorldData worldData, 
        IRadiusRule radiusRule,
        float frequency = 1.0f,
        int maxAttempts = 30)
    {
        var terrainMapHeight = worldData.TerrainData.TerrainMapHeight;
        var terrDataMapWidth = worldData.TerrainData.TerrainMapWidth;
        var h = Mathf.RoundToInt(terrainMapHeight * frequency);
        var w = Mathf.RoundToInt(terrDataMapWidth * frequency);

        var trees = new bool[h, w];
        var activeList = new List<Vector2>();
        var neighborCache = new Dictionary<int, Dictionary<int, List<Vector2>>>();

        Func<int, int, List<Vector2>> getNeighbors = (cellX, cellY) =>
        {
            if (neighborCache.ContainsKey(cellX) && neighborCache[cellX].ContainsKey(cellY))
            {
                return neighborCache[cellX][cellY];
            }

            return new List<Vector2>();
        };

        Action<Vector2, float> addToNeighborCache = (point, minDist) =>
        {
            // Use a smaller cell size to ensure proper neighbor checks
            int cellSize = Math.Max(1, (int)Math.Floor(minDist / 2.0));

            int cellX = (int)(point.X / cellSize);
            int cellY = (int)(point.Y / cellSize);

            if (!neighborCache.ContainsKey(cellX))
            {
                neighborCache[cellX] = new Dictionary<int, List<Vector2>>();
            }

            if (!neighborCache[cellX].ContainsKey(cellY))
            {
                neighborCache[cellX][cellY] = new List<Vector2>();
            }

            neighborCache[cellX][cellY].Add(point);
        };

        var random = new Random();
        var startX = random.Next(w);
        var startY = random.Next(h);
        var startPoint = new Vector2(startX, startY);
        trees[startY, startX] = true;
        activeList.Add(startPoint);

        while (activeList.Count > 0)
        {
            // Select random active point
            var randomIndex = random.Next(activeList.Count);
            var currentPoint = activeList[randomIndex];
            var currentPointNormalized = currentPoint / frequency;
            var currentMinDist = radiusRule.GetRadius(currentPointNormalized, worldData);
            var foundValidPoint = false;

            // Try to find new valid point around current one
            for (int i = 0; i < maxAttempts; i++)
            {
                // Generate random angle and distance between r and 2r
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                float distance = currentMinDist + (float)random.NextDouble() * currentMinDist;
                float newX = currentPoint.X + distance * (float)Math.Cos(angle);
                float newY = currentPoint.Y + distance * (float)Math.Sin(angle);

                // Check if point is within map boundaries
                if (newX < 0 || newX >= w || newY < 0 || newY >= h)
                    continue;

                int newPosX = Mathf.FloorToInt(newX);
                int newPosY = Mathf.FloorToInt(newY);

                // Skip if cell is already occupied
                if (trees[newPosY, newPosX])
                    continue;

                // Create exact point
                var newExactPoint = new Vector2(newX, newY);
                var newGridPoint = new Vector2(newPosX, newPosY);
                var newGridPointNormalized = newGridPoint / frequency;
                var newPointMinDist = radiusRule.GetRadius(newGridPointNormalized, worldData);

                // Check for minimum distance from all existing points
                bool validPoint = true;
                int cellSize = Math.Max(1, (int)Math.Floor(newPointMinDist / 2.0));

                int cellX = (int)(newExactPoint.X / cellSize);
                int cellY = (int)(newExactPoint.Y / cellSize);

                // Check neighboring cells in cache - expanded search radius for better coverage
                int searchRadius = 3;
                for (int offX = -searchRadius; offX <= searchRadius && validPoint; offX++)
                {
                    for (int offY = -searchRadius; offY <= searchRadius && validPoint; offY++)
                    {
                        List<Vector2> neighbors = getNeighbors(cellX + offX, cellY + offY);

                        foreach (Vector2 neighbor in neighbors)
                        {
                            // Get the minimum distance required between the two points
                            var neighborGridPos = new Vector2((int)neighbor.X, (int)neighbor.Y);
                            var neighborGridPosNormalized = neighborGridPos / frequency;

                            float neighborMinDist = radiusRule.GetRadius(neighborGridPosNormalized, worldData);
                            float requiredDist = Math.Max(newPointMinDist, neighborMinDist);

                            // Calculate actual distance between points
                            float actualDist = newExactPoint.DistanceTo(neighbor);

                            // If distance is less than required, point is invalid
                            if (actualDist < requiredDist)
                            {
                                validPoint = false;
                                break;
                            }
                        }
                    }
                }

                if (validPoint)
                {
                    activeList.Add(newExactPoint);
                    trees[newPosY, newPosX] = true;
                    addToNeighborCache(newExactPoint, newPointMinDist);
                    foundValidPoint = true;
                    break;
                }
            }

            // If no valid point found, remove current point from active list
            if (!foundValidPoint)
            {
                activeList.RemoveAt(randomIndex);
            }
        }

        return trees;
    }
}