using System.Collections.Generic;

namespace TerrainGenerationApp.Domain.Core;

public interface IWorldData
{
    public TerrainData TerrainData { get; }
    public TreesData TreesData { get; }
    public float SeaLevel { get; }

    public string ToJson(Dictionary<string, object> generationConfig = null);
}