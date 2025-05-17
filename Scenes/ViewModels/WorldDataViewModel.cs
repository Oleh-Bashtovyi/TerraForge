using System;
using System.Collections.Generic;
using TerrainGenerationApp.Domain.Core;

namespace TerrainGenerationApp.Scenes.ViewModels;

public class WorldDataViewModel(WorldData worldData)
{
    private readonly WorldData _worldData = worldData;

    public int TerrainMapHeight => _worldData.TerrainData.TerrainMapHeight;
    public int TerrainMapWidth => _worldData.TerrainData.TerrainMapWidth;
    public IWorldData WorldData => _worldData;

    public event Action TerrainChanged;
    public event Action SeaLevelChanged;
    public event Action TreesChanged;

    public void SetTerrain(float[,] terrainMap, bool calculateSlopes = true)
    {
        _worldData.TerrainData.SetTerrain(terrainMap, calculateSlopes);
        TerrainChanged?.Invoke();
    }

    public void SetSeaLevel(float value)
    {
        _worldData.SetSeaLevel(value);
        SeaLevelChanged?.Invoke();
    }

    public void SetTreesData(List<TreesLayer> treesData)
    {
        _worldData.TreesData.SetLayers(treesData);
        TreesChanged?.Invoke();
    }

    public void SetTreesData(Dictionary<string, bool[,]> treesData)
    {
        _worldData.TreesData.SetLayers(treesData);
        TreesChanged?.Invoke();
    }
}