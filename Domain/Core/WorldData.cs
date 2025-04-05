using Godot;

namespace TerrainGenerationApp.Domain.Core;

public class WorldData : IWorldData
{
    public TerrainData TerrainData { get; }
    public TreesData TreesData { get; }
    public float SeaLevel { get; private set; }

    public WorldData()
    {
        TerrainData = new TerrainData();
        TreesData = new TreesData();
        SeaLevel = 0.3f;
    }


    public void SetSeaLevel(float value)
    {
        SeaLevel = (float)Mathf.Clamp(value, 0.0, 1.0);
    }
}
