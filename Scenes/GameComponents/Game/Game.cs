using Godot;
using System;
using System.Collections.Generic;
using TerrainGenerationApp.Scenes.GameComponents.DisplayOptions;
using TerrainGenerationApp.Scenes.GameComponents.GenerationMenu;
using TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Scenes.GameComponents.Game;

public partial class Game : Node3D
{
    private TerrainScene2D.TerrainScene2D _terrainScene2D;
    private TreePlacementOptions _treePlacementOptions;
    private MapGenerationMenu _mapGenerationMenu;
    private MapDisplayOptions _displayOptions;
    private Button _generateMapButton;

    private WorldData _worldData;
    private GodotThread _waterErosionThread;

    private Dictionary<string, Color> _treeColors = new();

    public override void _Ready()
    {
        _terrainScene2D = GetNode<TerrainScene2D.TerrainScene2D>("%TerrainScene2D");
        _mapGenerationMenu = GetNode<MapGenerationMenu>("%MapGenerationMenu");
        _generateMapButton = GetNode<Button>("%GenerateMapButton");
        _displayOptions = GetNode<MapDisplayOptions>("%MapDisplayOptions");
        _worldData = new();
        _worldData.SetSeaLevel(_mapGenerationMenu.CurSeaLevel);
        _terrainScene2D.SetTreeColors(_treeColors);
        _treePlacementOptions = _mapGenerationMenu.TreePlacementOptions;
        _treePlacementOptions.OnTreePlacementRuleItemAdded += TreePlacementOptionsOnOnTreePlacementRuleItemAdded;
        _treePlacementOptions.OnTreePlacementRuleItemRemoved += TreePlacementOptionsOnOnTreePlacementRuleItemRemoved;
        _treePlacementOptions.OnTreePlacementRulesChanged += TreePlacementOptionsOnOnTreePlacementRulesChanged;

        _terrainScene2D.SetDisplayOptions(_displayOptions);
        _mapGenerationMenu.OnWaterLevelChanged += MapGenerationMenuOnOnWaterLevelChanged;
        _mapGenerationMenu.GenerationParametersChanged += MapGenerationMenuOnGenerationParametersChanged;
        _displayOptions.OnDisplayOptionsChanged += DisplayOptionsOnOnDisplayOptionsChanged;
        _generateMapButton.Pressed += GenerateMapButtonOnPressed;
    }

    private void MapGenerationMenuOnOnWaterLevelChanged(object sender, EventArgs e)
    {
        _worldData.SetSeaLevel(_mapGenerationMenu.CurSeaLevel);
        GD.Print("SEA LEVEL CHANGED EVENT HANDLED!");
        _terrainScene2D.RedrawTerrain(_worldData);
    }

    private void MapGenerationMenuOnGenerationParametersChanged(object sender, EventArgs e)
    {
        RegenerateMap();
    }

    private void TreePlacementOptionsOnOnTreePlacementRulesChanged(object sender, EventArgs e)
    {
        GD.Print("---> GAME ---> RuleChangedEvent!");

        if (_mapGenerationMenu.RegenerateOnParametersChanged && _mapGenerationMenu.EnableTrees)
        {
            GD.Print("---> GAME ---> Regenerating trees...");
            var trees = _treePlacementOptions.GenerateTrees(_worldData);
            _worldData.SetTreeMaps(trees);
            _terrainScene2D.RedrawTerrain(_worldData);
            _terrainScene2D.RedrawTreeLayers(_worldData);
        }
    }

    private void TreePlacementOptionsOnOnTreePlacementRuleItemRemoved(object sender, TreePlacementRuleItem e)
    {
        GD.Print("---> GAME ---> ItemRemovedEvent!");
        if (_mapGenerationMenu.RegenerateOnParametersChanged && _mapGenerationMenu.EnableTrees)
        {
            GD.Print("---> GAME ---> Regenerating trees...");
            var trees = _treePlacementOptions.GenerateTrees(_worldData);
            _worldData.SetTreeMaps(trees);
            _terrainScene2D.RedrawTerrain(_worldData);
            _terrainScene2D.RedrawTreeLayers(_worldData);
        }
    }

    private void TreePlacementOptionsOnOnTreePlacementRuleItemAdded(object sender, TreePlacementRuleItem e)
    {
        GD.Print("---> GAME ---> ItemAddedEvent!");

        e.OnTreeColorChanged += TreePlacementRuleItemOnTreeColorChanged;
        e.OnTreeIdChanged += TreePlacementRuleItemOnOnTreeIdChanged;
        _treeColors[e.TreeId] = e.TreeColor;

        if (_mapGenerationMenu.RegenerateOnParametersChanged && _mapGenerationMenu.EnableTrees)
        {
            GD.Print("---> GAME ---> Regenerating trees...");
            var trees = _treePlacementOptions.GenerateTrees(_worldData);
            _worldData.SetTreeMaps(trees);
            _terrainScene2D.RedrawTerrain(_worldData);
            _terrainScene2D.RedrawTreeLayers(_worldData);
        }
    }

    private void TreePlacementRuleItemOnOnTreeIdChanged(object sender, TreeIdChangedEventArgs e)
    {
        if (e.NewTreeId == e.OldTreeId)
        {
            return;
        }

        var color = _treeColors[e.OldTreeId];
        var treeMap = _worldData.TreeMaps[e.OldTreeId];
        _treeColors.Remove(e.OldTreeId);
        _worldData.TreeMaps.Remove(e.OldTreeId);
        _treeColors[e.NewTreeId] = color;
        _worldData.TreeMaps[e.NewTreeId] = treeMap;
    }

    private void TreePlacementRuleItemOnTreeColorChanged(object sender, TreeColorChangedEventArgs e)
    {
        _treeColors[e.TreeId] = e.NewColor;
        _terrainScene2D.RedrawTerrain(_worldData);
        _terrainScene2D.RedrawTreeLayers(_worldData);
    }


    private void DisplayOptionsOnOnDisplayOptionsChanged()
    {
        _terrainScene2D.RedrawTerrain(_worldData);
        _terrainScene2D.RedrawTreeLayers(_worldData);
    }


    private void GenerateMapButtonOnPressed()
    {
        RegenerateMap();
    }

    private void RegenerateMap()
    {
        var generator = _mapGenerationMenu.SelectedGenerator;

        if (generator == null)
        {
            GD.PushError("GENERATOR IS NOT SELECTED");
            return;
        }

        var map = generator.GenerateMap();

        MapHelpers.MultiplyHeight(map, _mapGenerationMenu.CurNoiseInfluence);

        for (int i = 0; i < _mapGenerationMenu.CurSmoothCycles; i++)
        {
            map = MapHelpers.SmoothMap(map);
        }

        if (_mapGenerationMenu.EnableIslands)
        {
            map = _mapGenerationMenu.IslandsApplier.ApplyIslands(map);
        }

        if (_mapGenerationMenu.EnableDomainWarping)
        {
            map = _mapGenerationMenu.DomainWarpingApplier.ApplyWarping(map);
        }

        _worldData.SetTerrain(map);
        _worldData.TreeMaps.Clear();

        var treeMaps = _treePlacementOptions.GenerateTrees(_worldData);
        _worldData.SetTreeMaps(treeMaps);

        HandleFullGeneration();
    }


    private void HandleFullGeneration()
    {
        _terrainScene2D.ResizeMap(_worldData);
        _terrainScene2D.RedrawTerrain(_worldData);
        _terrainScene2D.RedrawTreeLayers(_worldData);
    }



}