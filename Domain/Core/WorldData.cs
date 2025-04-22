using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TerrainGenerationApp.Domain.Extensions;

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

	public string ToJson(Dictionary<string, object> generationConfig = null)
	{
		var terrainHeightMap = TerrainData.GetHeightMapCopy();
		var treeLayers = TreesData.GetLayers();
		var seaLevel = SeaLevel;

		var data = new
		{
			MapHeight = terrainHeightMap.Height(),
			MapWidth = terrainHeightMap.Width(),
			SeaLevel = seaLevel,
			GenerationConfig = generationConfig,
			HeightMap = terrainHeightMap.ToOneDimensionArray(),
			TreeLayersHeight = TreesData.LayersHeight,
			TreeLayersWidth = TreesData.LayersWidth,
			TreesLayer = treeLayers.Select(x => new
			{
				LayerName = x.TreeId,
				Map = x.TreesMap.ToOneDimensionArray()
			})
		};

		var options = new JsonSerializerOptions
		{
			WriteIndented = true
		};

		var result = JsonSerializer.Serialize(data, options);

		return result;
	}


	public void LoadFromJson(string json)
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
		
		foreach (var layer in treeLayers)
		{
			var layerName = layer.GetProperty("LayerName").GetString();
			var treeMapArray = layer.GetProperty("Map").EnumerateArray()
				.Select(e => e.GetBoolean())
				.ToTwoDimensionArray(treeLayersHeight, treeLayersWidth);

			if (!string.IsNullOrEmpty(layerName))
				treeLayersDict.Add(layerName, treeMapArray);
		}

		SeaLevel = seaLevel;
		TerrainData.SetTerrain(heightMap);
		TreesData.SetLayers(treeLayersDict);
	}
}
