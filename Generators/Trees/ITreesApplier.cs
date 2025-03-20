using System.Collections.Generic;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Generators.Trees;

public interface ITreesApplier
{
    public Dictionary<string, bool[,]> GenerateTreeLayers(IWorldData worldData);
    public bool[,] GenerateTreeLayer(IWorldData worldData, string treeId);
}