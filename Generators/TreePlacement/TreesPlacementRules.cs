using Godot;
using TerrainGenerationApp.Generators.TreePlacement.CanPlaceTreeRules;
using TerrainGenerationApp.Generators.TreePlacement.TreeRadiusRules;

public interface ICurTerrainInfo
{
	float[,] CurTerrainMap { get; }
	float[,] CurSlopesMap { get; }
	float CurWaterLevel { get; }
}


public class TreesPlacementRule
{
	public string TreeType { get; }
	public ICanPlaceTreeRule CanPlaceTreeRule { get; }
	public ITreesRadiusRule TreesRadiusRule { get; }

	public TreesPlacementRule(string treeType, ICanPlaceTreeRule canPlaceTreeRule, ITreesRadiusRule treesRadiusRule)
	{
		TreeType = treeType;
		CanPlaceTreeRule = canPlaceTreeRule;
		TreesRadiusRule = treesRadiusRule;
	}

	public bool CanPlace(Vector2 pos, ICurTerrainInfo terrainInfo)
	{
		return CanPlaceTreeRule.CanPlaceIn(pos, terrainInfo);
	}

	public float GetRadius(Vector2 pos, ICurTerrainInfo terrainInfo)
	{
		return TreesRadiusRule.GetRadius(pos, terrainInfo);
	}
}
