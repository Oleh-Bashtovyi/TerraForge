using Godot;
using System;
using TerrainGenerationApp.Scenes;

public partial class Game : Node3D
{
	private TerrainScene2D _terrainScene2D;
	private MapGenerationMenu _mapGenerationMenu;
	private MapDisplayOptions _displayOptions;

    private Button _generateMapButton;

	public override void _Ready()
	{
		_terrainScene2D = GetNode<TerrainScene2D>("%TerrainScene2D");
		_mapGenerationMenu = GetNode<MapGenerationMenu>("%MapGenerationMenu");
        _generateMapButton = GetNode<Button>("%GenerateMapButton");
        _displayOptions = GetNode<MapDisplayOptions>("%MapDisplayOptions");

        _generateMapButton.Pressed += () =>
        {
            _mapGenerationMenu.GenerateMap();
        };

        _terrainScene2D.SetDisplayOptions(_displayOptions);
        _terrainScene2D.SetGenerationMenu(_mapGenerationMenu);
        //_mapGenerationMenu.OnMapGenerated += (result) =>
        //{
        //	_terrainScene2D.SetTerrainResult(result);
        //};
    }




}
