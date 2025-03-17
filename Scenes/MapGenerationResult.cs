namespace TerrainGenerationApp.Scenes;

public class MapGenerationResult
{
    public float[,] TerrainMap { get; }
    public float[,] SlopesMap { get; }
    public bool[,] TreesPlacement { get; }

    public MapGenerationResult(float[,] terrainMap, float[,] slopesMap, bool[,] treesPlacement)
    {
        TerrainMap = terrainMap;
        SlopesMap = slopesMap;
        TreesPlacement = treesPlacement;
    }
}
