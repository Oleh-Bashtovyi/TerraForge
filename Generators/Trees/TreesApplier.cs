using System;
using System.Collections.Generic;
using Godot;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Generators.Trees;


public class TreesApplier : ITreesApplier
{
    public class TreeGenerationResult
    {
        public Dictionary<string, bool[,]> TreeMaps { get; } = new Dictionary<string, bool[,]>();

        public void AddTreeMap(string treeType, bool[,] treeMap)
        {
            TreeMaps[treeType] = treeMap;
        }
    }


    public class TreesLayer(string treeId, List<Vector2> treePositions)
    {
        public string TreeId { get; } = treeId;
        public List<Vector2> TreePositions { get; } = treePositions;
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






    public static TreeGenerationResult GenerateTreesMapsFromRules(
        float[,] heightMap,
        IWorldData worldData,
        TreePlacementRule[] rules,
        int maxAttempts = 30)
    {
        var result = new TreeGenerationResult();

        foreach (var rule in rules)
        {
            // Convert rule to functions that TreesGenerator can use
            Func<Vector2, bool> canPlaceFunc = pos => rule.CanPlace(pos, worldData);
            Func<Vector2, float> minDistanceFunc = pos => rule.GetRadius(pos, worldData);

            // Generate tree map for this rule
            bool[,] treeMap = GenerateTreesMap(heightMap, maxAttempts, canPlaceFunc, minDistanceFunc);
            result.AddTreeMap(rule.TreeId, treeMap);
        }

        return result;
    }

    public static bool[,] GenerateTreesMap(
        float[,] heightMap,
        int maxAttempts = 30,
        Func<Vector2, bool> canPlaceFunc = null,
        Func<Vector2, float> minDistanceFunc = null)
    {
        var map = heightMap;
        var h = map.GetLength(0);
        var w = map.GetLength(1);
        var trees = new bool[h, w];
        var points = new List<Vector2>();
        var activeList = new List<Vector2>();

        // Default distance function
        if (minDistanceFunc == null)
        {
            minDistanceFunc = _ => 10f;
        }

        if (canPlaceFunc == null)
        {
            canPlaceFunc = _ => true;
        }

        // Spatial partitioning structure for fast neighbor search
        var neighborCache = new Dictionary<int, Dictionary<int, List<Vector2>>>();

        // Function to get neighbors from cache
        Func<int, int, List<Vector2>> getNeighbors = (cellX, cellY) =>
        {
            if (neighborCache.ContainsKey(cellX) && neighborCache[cellX].ContainsKey(cellY))
            {
                return neighborCache[cellX][cellY];
            }

            return new List<Vector2>();
        };

        // Function to add point to neighbor cache
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



        // NEW: Try multiple starting points across the map
        const int maxStartingPoints = 50; // Adjust as needed
        int startingPointsFound = 0;

        // Try to find multiple starting points
        for (int attempt = 0; attempt < 2000 && startingPointsFound < maxStartingPoints; attempt++)
        {
            int startX = random.Next(w);
            int startY = random.Next(h);
            var startPoint = new Vector2(startX, startY);

            if (!trees[startY, startX] && canPlaceFunc(startPoint))
            {
                // Check minimum distance from existing points
                bool validPoint = true;
                float startMinDist = minDistanceFunc(startPoint);

                foreach (var existingPoint in points)
                {
                    float dist = CalculateDistance(startPoint, existingPoint);
                    if (dist < Math.Max(startMinDist, minDistanceFunc(existingPoint)))
                    {
                        validPoint = false;
                        break;
                    }
                }

                if (validPoint)
                {
                    points.Add(startPoint);
                    activeList.Add(startPoint);
                    trees[startY, startX] = true;
                    addToNeighborCache(startPoint, startMinDist);
                    startingPointsFound++;
                }
            }
        }





        //// Attempt to find starting points more aggressively
        //bool foundStartingPoint = false;
        //for (int attempt = 0; attempt < 2000 && !foundStartingPoint; attempt++)
        //{
        //    int startX = random.Next(w);
        //    int startY = random.Next(h);
        //    var startPoint = new Vector2(startX, startY);

        //    if (canPlaceFunc(startPoint))
        //    {
        //        points.Add(startPoint);
        //        activeList.Add(startPoint);
        //        trees[startY, startX] = true;

        //        float startMinDist = minDistanceFunc(startPoint);
        //        addToNeighborCache(startPoint, startMinDist);
        //        foundStartingPoint = true;
        //    }
        //}

        // If no valid starting point found, return empty tree map
        /*        if (!foundStartingPoint)
                {
                    return trees;
                }*/

        // Main tree placement algorithm
        while (activeList.Count > 0)
        {
            // Select random active point
            int randomIndex = random.Next(activeList.Count);
            Vector2 currentPoint = activeList[randomIndex];

            bool foundValidPoint = false;
            float currentMinDist = minDistanceFunc(currentPoint);

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

                // Skip if can't place based on rule
                if (!canPlaceFunc(newGridPoint))
                    continue;

                float newPointMinDist = minDistanceFunc(newGridPoint);

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
                            float neighborMinDist = minDistanceFunc(neighborGridPos);
                            float requiredDist = Math.Max(newPointMinDist, neighborMinDist);

                            // Calculate actual distance between points
                            float actualDist = CalculateDistance(newExactPoint, neighbor);

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
                    points.Add(newExactPoint);
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

    // Helper method to calculate Euclidean distance between two points
    private static float CalculateDistance(Vector2 a, Vector2 b)
    {
        return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
    }
}






















/*public static class TreesApplier
{


    public static bool[,] GenerateTreesMap(
        float[,] heightMap,
        int maxAttempts = 30,
        Func<Vector2, bool> canPlaceFunc = null,
        Func<Vector2, float> minDistanceFunc = null)
    {
        var map = heightMap;
        var h = map.GetLength(0);
        var w = map.GetLength(1);
        var trees = new bool[h, w];
        var points = new List<Vector2>();
        var activeList = new List<Vector2>();

        // Default distance function
        if (minDistanceFunc == null)
        {
            minDistanceFunc = _ => 10f;
        }

        if (canPlaceFunc == null)
        {
            canPlaceFunc = _ => true;
        }

        // Структура даних для швидкого пошуку сусідів
        var neighborCache = new Dictionary<int, Dictionary<int, List<Vector2>>>();

        // Функції для роботи з кешем
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
            int cellSize = (int)Math.Floor(minDist / Math.Sqrt(2));

            if (cellSize < 1)
            {
                cellSize = 1;
            }

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

        for (int i = 0; i < 1000; i++)
        {
            // Починаємо з випадкової точки
            int startX = random.Next(w);
            int startY = random.Next(h);
            var startPoint = new Vector2(startX, startY);

            if (canPlaceFunc(startPoint))
            {
                points.Add(startPoint);
                activeList.Add(startPoint);
                trees[startY, startX] = true;

                float startMinDist = minDistanceFunc(new Vector2(startX, startY));
                addToNeighborCache(startPoint, startMinDist);
                break;
            }
        }

        while (activeList.Count > 0)
        {
            // Вибираємо випадкову активну точку
            int randomIndex = random.Next(activeList.Count);
            Vector2 currentPoint = activeList[randomIndex];

            bool foundValidPoint = false;
            float currentMinDist = minDistanceFunc(new Vector2(currentPoint.X, currentPoint.Y));

            // Намагаємось знайти нову точку навколо поточної
            for (int i = 0; i < maxAttempts; i++)
            {
                // Генеруємо випадкову точку на відстані між r і 2r
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                float distance = currentMinDist + (float)random.NextDouble() * currentMinDist;

                float newX = currentPoint.X + distance * (float)Math.Cos(angle);
                float newY = currentPoint.Y + distance * (float)Math.Sin(angle);

                // Перевіряємо чи точка знаходиться в межах карти
                if (newX < 0 || newX >= w || newY < 0 || newY >= h)
                    continue;

                int newPosX = (int)newX;
                int newPosY = (int)newY;

                // Пропускаємо, якщо клітинка вже зайнята
                if (trees[newPosY, newPosX] || !canPlaceFunc(new Vector2(newPosX, newPosY)))
                    continue;

                Vector2 newPoint = new Vector2(newX, newY);
                float newPointMinDist = minDistanceFunc(new Vector2(newPosX, newPosY));

                // Перевіряємо сусідів у кеші для швидкого доступу
                bool validPoint = true;
                int cellSize = (int)Math.Floor(newPointMinDist / Math.Sqrt(2));
                if (cellSize < 1) cellSize = 1;

                int cellX = (int)(newPoint.X / cellSize);
                int cellY = (int)(newPoint.Y / cellSize);

                // Перевіряємо сусідні клітинки в кеші
                for (int offX = -2; offX <= 2 && validPoint; offX++)
                {
                    for (int offY = -2; offY <= 2 && validPoint; offY++)
                    {
                        List<Vector2> neighbors = getNeighbors(cellX + offX, cellY + offY);

                        foreach (Vector2 neighbor in neighbors)
                        {
                            float requiredDist = Math.Max(
                                newPointMinDist,
                                minDistanceFunc(new Vector2(neighbor.X, neighbor.Y))
                            );

                            if (newPoint.DistanceTo(neighbor) < requiredDist)
                            {
                                validPoint = false;
                                break;
                            }
                        }
                    }
                }

                if (validPoint)
                {
                    points.Add(newPoint);
                    activeList.Add(newPoint);
                    trees[newPosY, newPosX] = true;
                    addToNeighborCache(newPoint, newPointMinDist);
                    foundValidPoint = true;
                    break;
                }
            }

            // Якщо не знайдено жодної валідної точки, видаляємо поточну з активного списку
            if (!foundValidPoint)
            {
                activeList.RemoveAt(randomIndex);
            }
        }


        return trees;
    }

}*/

