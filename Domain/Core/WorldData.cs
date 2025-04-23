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
}
