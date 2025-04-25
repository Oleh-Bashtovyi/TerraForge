using System.Collections.Generic;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Containers;

public interface ILastUsedConfigProvider
{
    public Dictionary<string, object> GetLastUsedConfig();
    public void UpdateCurrentConfigAsLastUsed();
    public void LoadConfigFrom(Dictionary<string, object> config);
}