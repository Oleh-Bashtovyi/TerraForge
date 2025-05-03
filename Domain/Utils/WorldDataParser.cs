using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Extensions;

namespace TerrainGenerationApp.Domain.Utils;


public class WorldFileData
{
    public float[,] HeightMap { get; set; }
    public float SeaLevel { get; set; }
    public Dictionary<string, bool[,]> TreeLayersDict { get; set; }
    public Dictionary<string, object> GenerationConfiguration { get; set; }
}

public static class WorldDataParser
{
    public static WorldFileData LoadFromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        using var jsonDoc = JsonDocument.Parse(json);
        var root = jsonDoc.RootElement;

        var seaLevel = root.GetProperty("SeaLevel").GetSingle();
        var mapHeight = root.GetProperty("MapHeight").GetInt32();
        var mapWidth = root.GetProperty("MapWidth").GetInt32();
        var heightMap = root.GetProperty("HeightMap").EnumerateArray()
            .Select(e => e.GetSingle())
            .ToTwoDimensionArray(mapHeight, mapWidth);
        var treeLayersDict = new Dictionary<string, bool[,]>();
        var treeLayersWidth = root.GetProperty("TreeLayersWidth").GetInt32();
        var treeLayersHeight = root.GetProperty("TreeLayersHeight").GetInt32();
        var treeLayers = root.GetProperty("TreesLayer").EnumerateArray();

        // Перетворення GenerationConfig у Dictionary<string, object>
        var generationConfig = ConvertJsonElementToDictionary(root.GetProperty("GenerationConfig"));

        foreach (var layer in treeLayers)
        {
            var layerName = layer.GetProperty("LayerName").GetString();
            var treeMapArray = layer.GetProperty("Map").EnumerateArray()
                .Select(e => e.GetBoolean())
                .ToTwoDimensionArray(treeLayersHeight, treeLayersWidth);
            if (!string.IsNullOrEmpty(layerName))
                treeLayersDict.Add(layerName, treeMapArray);
        }

        var worldFileData = new WorldFileData();
        worldFileData.HeightMap = heightMap;
        worldFileData.SeaLevel = seaLevel;
        worldFileData.GenerationConfiguration = generationConfig;
        worldFileData.TreeLayersDict = treeLayersDict;

        return worldFileData;
    }

    private static Dictionary<string, object> ConvertJsonElementToDictionary(JsonElement element)
    {
        var result = new Dictionary<string, object>();

        foreach (var property in element.EnumerateObject())
        {
            string propertyName = property.Name;
            JsonElement propertyValue = property.Value;

            result[propertyName] = ConvertJsonElementToObject(propertyValue);
        }

        return result;
    }

    private static object ConvertJsonElementToObject(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                return ConvertJsonElementToDictionary(element);

            case JsonValueKind.Array:
                return element.EnumerateArray()
                    .Select(ConvertJsonElementToObject)
                    .ToList();

            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                if (element.TryGetInt32(out int intValue))
                    return intValue;
                if (element.TryGetSingle(out float floatValue))
                    return floatValue;
                if (element.TryGetDouble(out double doubleValue))
                    return doubleValue;
                return element.GetRawText();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null;

            default:
                return element.GetRawText();
        }
    }
}