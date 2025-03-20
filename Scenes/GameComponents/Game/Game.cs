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
        _treePlacementOptions = _mapGenerationMenu.TreePlacementOptions;
        _treePlacementOptions.OnTreePlacementRuleItemAdded += TreePlacementOptionsOnOnTreePlacementRuleItemAdded;
        _treePlacementOptions.OnTreePlacementRuleItemRemoved += TreePlacementOptionsOnOnTreePlacementRuleItemRemoved;
        _treePlacementOptions.OnTreePlacementRulesChanged += TreePlacementOptionsOnOnTreePlacementRulesChanged;

        _displayOptions.OnDisplayOptionsChanged += DisplayOptionsOnOnDisplayOptionsChanged;
        _generateMapButton.Pressed += GenerateMapButtonOnPressed;
    }

    private void TreePlacementOptionsOnOnTreePlacementRulesChanged(object sender, EventArgs e)
    {
        if (_mapGenerationMenu.RegenerateOnParametersChanged && _mapGenerationMenu.EnableTrees)
        {
            var trees = _mapGenerationMenu.TreesApplier.GenerateTreeLayers(_worldData);
            _worldData.SetTreeMaps(trees);
            _terrainScene2D.RedrawTreeLayers(_worldData);
        }
    }

    private void TreePlacementOptionsOnOnTreePlacementRuleItemRemoved(object sender, TreePlacementRuleItem e)
    {
        if (_mapGenerationMenu.RegenerateOnParametersChanged && _mapGenerationMenu.EnableTrees)
        {
            var trees = _mapGenerationMenu.TreesApplier.GenerateTreeLayers(_worldData);
            _worldData.SetTreeMaps(trees);
            _terrainScene2D.RedrawTreeLayers(_worldData);
        }
    }

    private void TreePlacementOptionsOnOnTreePlacementRuleItemAdded(object sender, TreePlacementRuleItem e)
    {
        e.OnTreeColorChanged += TreePlacementRuleItemOnTreeColorChanged;
        e.OnTreeIdChanged += TreePlacementRuleItemOnOnTreeIdChanged;
        _treeColors[e.TreeId] = e.GetColor;

        if (_mapGenerationMenu.RegenerateOnParametersChanged && _mapGenerationMenu.EnableTrees)
        {
            var trees = _mapGenerationMenu.TreesApplier.GenerateTreeLayers(_worldData);
            _worldData.SetTreeMaps(trees);
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
    }


    private void DisplayOptionsOnOnDisplayOptionsChanged()
    {
        _terrainScene2D.RedrawTerrain(_worldData);
    }


    private void GenerateMapButtonOnPressed()
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

        HandleFullGeneration();
    }


    private void HandleFullGeneration()
    {
        _terrainScene2D.ResizeMap(_worldData);
        _terrainScene2D.RedrawTerrain(_worldData);
    }



}