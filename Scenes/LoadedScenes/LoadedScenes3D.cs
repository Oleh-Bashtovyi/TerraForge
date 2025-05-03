using Godot;

namespace TerrainGenerationApp.Scenes.LoadedScenes;

public class LoadedScenes3D
{
    public static readonly PackedScene TERRAIN_CHUNK_SCENE =
        ResourceLoader.Load<PackedScene>("res://Scenes/CoreModules/TerrainScene3D/TerrainChunk.tscn");
}