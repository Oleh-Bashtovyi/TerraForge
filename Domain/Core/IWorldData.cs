using System.Collections.Generic;
using Godot;

namespace TerrainGenerationApp.Domain.Core;

public interface IWorldData
{
    public TerrainData TerrainData { get; }
    public TreesData TreesData { get; }
    public float SeaLevel { get; }

    public float DepthAt(float x, float y);
    public float DepthAt(Vector2 pos) => DepthAt(pos.X, pos.Y);

    public string ToJson(Dictionary<string, object> generationConfig = null);
}