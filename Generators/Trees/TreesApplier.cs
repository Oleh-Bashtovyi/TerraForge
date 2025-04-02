using Godot;
using System;
using System.Collections.Generic;
using TerrainGenerationApp.Extensions;
using TerrainGenerationApp.RadiusRules;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Generators.Trees;


public class TreesApplier : ITreesApplier
{
    public record TreeLayerResult(bool[,] Trees, List<Vector2> Points);


    // FIRST WAY (EASY):
    // 1) Generate map of trees using "Poisson Disc Sampling" with radius rule
    // 2) Clear trees that is not follow placement rules
    // 3) Return layer of trees
    //
    // SECOND WAY (HARDER):
    // 1) Generate map of trees using "Poisson Disc Sampling" with radius rule and placement rules simultaneously
    // !!! Possible lack of coverage in some areas, need several attempts or iteration over every possible point

    public class TreeGenerationResult
    {
        public Dictionary<string, bool[,]> TreeMaps { get; } = new Dictionary<string, bool[,]>();

        public void AddTreeMap(string treeType, bool[,] treeMap)
        {
            TreeMaps[treeType] = treeMap;
        }
    }


    public TreeGenerationResult GenerateTreeMaps(IWorldData worldData)
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, bool[,]> GenerateTreeLayers(IWorldData worldData)
    {
        throw new NotImplementedException();
    }

    public bool[,] GenerateTreeLayer(IWorldData worldData, string layerName)
    {
        throw new NotImplementedException();
    }


    public static Dictionary<string, bool[,]> GenerateTreesMapsFromRules(
        IWorldData worldData,
        TreePlacementRule[] rules,
        float frequency = 1.0f,
        int maxAttempts = 30)
    {
        var result = new Dictionary<string, bool[,]>();

        foreach (var rule in rules)
        {
            if (rule.RadiusRule == null || rule.PlacementRule == null)
            {
                var map = new bool[worldData.MapHeight, worldData.MapWidth];
                result.Add(rule.TreeId, map);
            }

            var res = GenerateTreesLayer(worldData, rule, frequency, maxAttempts);

            result.Add(rule.TreeId, res.Trees);
        }

        return result;
    }


    public class TreesLayer(string id, bool[,] trees)
    {
        public string Id { get; } = id;
        public bool[,] Trees { get; } = trees;
    }










    public TreeLayerResult GenerateTrees(int height, int width, IWorldData worldData, IRadiusRule radiusRule, int maxAttempts = 30)
    {
        var h = height;
        var w = width;
        var trees = new bool[height, width];
        var points = new List<Vector2>();
        var activeList = new List<Vector2>();

        // Spatial partitioning structure for fast neighbor search
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
        points.Add(startPoint);

        while (activeList.Count > 0)
        {
            var randomIndex = random.Next(activeList.Count);
            var currentPoint = activeList[randomIndex];

            var foundValidPoint = false;
            var currentMinDist = radiusRule.GetRadius(currentPoint, worldData);

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

                int newPosX = (int)Math.Floor(newX);
                int newPosY = (int)Math.Floor(newY);

                // Skip if cell is already occupied
                if (trees[newPosY, newPosX])
                    continue;

                // Create exact point
                var newExactPoint = new Vector2(newX, newY);
                var newGridPoint = new Vector2(newPosX, newPosY);

                var newPointMinDist = radiusRule.GetRadius(newGridPoint, worldData);

                // Check for minimum distance from all existing points
                var validPoint = true;
                var cellSize = Math.Max(1, (int)Math.Floor(newPointMinDist / 2.0));

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
                            Vector2 neighborGridPos = new Vector2((int)neighbor.X, (int)neighbor.Y);
                            float neighborMinDist = radiusRule.GetRadius(neighborGridPos, worldData);
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
                    points.Add(newExactPoint);
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

        return new TreeLayerResult(trees, points);
    }












    public static TreesLayer GenerateTreesLayer(
        IWorldData worldData, 
        TreePlacementRule rule,
        float frequency = 1.0f,
        int maxAttempts = 30)
    {
        if (rule.RadiusRule == null || rule.PlacementRule == null)
        {
            throw new ArgumentException("Rule must have both placement and radius rules");
        }

        GD.Print("Frequency: " + frequency);

        var terrainMapHeight = worldData.MapHeight;
        var terrDataMapWidth = worldData.MapWidth;
        var h = (int)(terrainMapHeight * frequency);
        var w = (int)(terrDataMapWidth * frequency);

        var trees = new bool[(int)(terrainMapHeight * frequency), (int)(terrDataMapWidth * frequency)];
        var activeList = new List<Vector2>();
        // Spatial partitioning structure for fast neighbor search
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

        // Main tree placement algorithm
        while (activeList.Count > 0)
        {
            // Select random active point
            int randomIndex = random.Next(activeList.Count);
            Vector2 currentPoint = activeList[randomIndex];

            bool foundValidPoint = false;
            float currentMinDist = rule.GetRadius(currentPoint, worldData);

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

                int newPosX = (int)Math.Floor(newX);
                int newPosY = (int)Math.Floor(newY);

                // Skip if cell is already occupied
                if (trees[newPosY, newPosX])
                    continue;

                // Create exact point
                Vector2 newExactPoint = new Vector2(newX, newY);
                Vector2 newGridPoint = new Vector2(newPosX, newPosY);

                float newPointMinDist = rule.GetRadius(newGridPoint, worldData);

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
                            Vector2 neighborGridPos = new Vector2((int)neighbor.X, (int)neighbor.Y);
                            float neighborMinDist = rule.GetRadius(neighborGridPos, worldData);
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

        GD.Print("Height: " + trees.Height());
        GD.Print("Width: " + trees.Width());

        // Apply placement rules
        for (int y = 0; y < trees.Height(); y++)
        {
            for (int x = 0; x < trees.Width(); x++)
            {
                var pos = new Vector2(x, y) / frequency;
                GD.Print(pos);

                if (trees[y, x] && !rule.CanPlace(pos, worldData))
                {
                    trees[y, x] = false;
                }
            }
        }

        var result = new TreesLayer(rule.TreeId, trees);

        return result;
    }
}