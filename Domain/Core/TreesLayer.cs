namespace TerrainGenerationApp.Domain.Core;

public record TreesLayer(string TreeId, bool[,] TreesMap, string? LayerName = null);