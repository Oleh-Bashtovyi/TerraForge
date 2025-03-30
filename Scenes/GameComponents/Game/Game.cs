using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TerrainGenerationApp.Scenes.GameComponents.DisplayOptions;
using TerrainGenerationApp.Scenes.GameComponents.GenerationMenu;
using TerrainGenerationApp.Scenes.GenerationOptions;
using TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Scenes.GameComponents.Game;

public partial class Game : Node3D, IWorldDataProvider
{
    private TerrainScene2D.TerrainScene2D _terrainScene2D;
    private TreePlacementOptions _treePlacementOptions;
    private MapGenerationMenu _mapGenerationMenu;
    private MapDisplayOptions _displayOptions;
    private Button _generateMapButton;

    private WorldData _worldData;
    private GodotThread _waterErosionThread;
    private GodotThread _generationThread;

    public IWorldData WorldData => _worldData;


    public override void _Ready()
    {
        _terrainScene2D = GetNode<TerrainScene2D.TerrainScene2D>("%TerrainScene2D");
        _mapGenerationMenu = GetNode<MapGenerationMenu>("%MapGenerationMenu");
        _displayOptions = GetNode<MapDisplayOptions>("%MapDisplayOptions");
        _generateMapButton = GetNode<Button>("%GenerateMapButton");
        _worldData = new();
        _worldData.SetSeaLevel(_mapGenerationMenu.CurSeaLevel);
        _terrainScene2D.SetWorldDataProvider(this);
        _terrainScene2D.SetDisplayOptionsProvider(_displayOptions);

        _treePlacementOptions = _mapGenerationMenu.TreePlacementOptions;
        _treePlacementOptions.OnTreePlacementRuleItemAdded += TreePlacementOptionsOnOnTreePlacementRuleItemAdded;
        _treePlacementOptions.OnTreePlacementRuleItemRemoved += TreePlacementOptionsOnOnTreePlacementRuleItemRemoved;
        _treePlacementOptions.OnTreePlacementRulesChanged += TreePlacementOptionsOnOnTreePlacementRulesChanged;


        //_mapGenerationMenu.OnWaterLevelChanged += MapGenerationMenuOnOnWaterLevelChanged;
        //_mapGenerationMenu.GenerationParametersChanged += MapGenerationMenuOnGenerationParametersChanged;
        _displayOptions.OnDisplayOptionsChanged += DisplayOptionsOnOnDisplayOptionsChanged;
        _generateMapButton.Pressed += GenerateMapButtonOnPressed;
    }




/*    private void MapGenerationMenuOnOnWaterLevelChanged(object sender, EventArgs e)
    {
        _worldData.SetSeaLevel(_mapGenerationMenu.CurSeaLevel);
        GD.Print("SEA LEVEL CHANGED EVENT HANDLED!");
        //_terrainScene2D.RedrawTerrain(_worldData);
    }*/

/*    private void MapGenerationMenuOnGenerationParametersChanged(object sender, EventArgs e)
    {
        //RegenerateMap();
    }*/

    private void TreePlacementOptionsOnOnTreePlacementRulesChanged(object sender, EventArgs e)
    {
        GD.Print("---> GAME ---> RuleChangedEvent!");

        if (_mapGenerationMenu.RegenerateOnParametersChanged && _mapGenerationMenu.EnableTrees)
        {
            GD.Print("---> GAME ---> Regenerating trees...");
            var trees = _treePlacementOptions.GenerateTrees(_worldData);
            _worldData.SetTreeMaps(trees);
            //_terrainScene2D.RedrawTerrain(_worldData);
            //_terrainScene2D.RedrawTreeLayers(_worldData);
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
            //_terrainScene2D.RedrawTerrain(_worldData);
            //_terrainScene2D.RedrawTreeLayers(_worldData);
        }
    }

    private void TreePlacementOptionsOnOnTreePlacementRuleItemAdded(object sender, TreePlacementRuleItem e)
    {
        GD.Print("---> GAME ---> ItemAddedEvent!");

        e.OnTreeColorChanged += TreePlacementRuleItemOnTreeColorChanged;
        e.OnTreeIdChanged += TreePlacementRuleItemOnOnTreeIdChanged;
        //_treeColors[e.TreeId] = e.TreeColor;

        if (_mapGenerationMenu.RegenerateOnParametersChanged && _mapGenerationMenu.EnableTrees)
        {
/*            GD.Print("---> GAME ---> Regenerating trees...");
            var trees = _treePlacementOptions.GenerateTrees(_worldData);
            _worldData.SetTreeMaps(trees);*/
            //_terrainScene2D.RedrawTerrain(_worldData);
            //_terrainScene2D.RedrawTreeLayers(_worldData);
        }
    }

    private void TreePlacementRuleItemOnOnTreeIdChanged(object sender, TreeIdChangedEventArgs e)
    {
/*        if (e.NewTreeId == e.OldTreeId)
        {
            return;
        }

        var color = _treeColors[e.OldTreeId];
        var treeMap = _worldData.TreeMaps[e.OldTreeId];
        _treeColors.Remove(e.OldTreeId);
        _worldData.TreeMaps.Remove(e.OldTreeId);
        _treeColors[e.NewTreeId] = color;
        _worldData.TreeMaps[e.NewTreeId] = treeMap;*/
    }

    private void TreePlacementRuleItemOnTreeColorChanged(object sender, TreeColorChangedEventArgs e)
    {


        //_treeColors[e.TreeId] = e.NewColor;
        //_terrainScene2D.RedrawTerrain(_worldData);
        //_terrainScene2D.RedrawTreeLayers(_worldData);
    }


    private void DisplayOptionsOnOnDisplayOptionsChanged()
    {
        //_terrainScene2D.RedrawTerrain(_worldData);
        //_terrainScene2D.RedrawTreeLayers(_worldData);
    }


    private void GenerateMapButtonOnPressed()
    {
        _generateMapButton.Disabled = true;
        var task = Task.Run(GenerationPipelineAsync);
        task.ContinueWith(async t =>
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            _generateMapButton.Disabled = false;
        });
    }




    private async Task GenerationPipelineAsync()
    {
        try
        {
            GD.Print("==============================================================");
            Log("SOME VALUE THAT LOGGED");
            GD.Print($"<START><ASYNC> - {nameof(GenerationPipelineAsync)}");

            var generator = _mapGenerationMenu.SelectedGenerator;
            if (generator == null) return;

            await DisableGenerationOptions();
            await Clear2DSceneAsync();

            await GenerateTerrainAsync(generator);
            await ApplyInfluenceAsync();
            await ApplyTerrainSmoothingAsync();

            if (_mapGenerationMenu.EnableIslands)
                await ApplyIslandsAsync();

            if (_mapGenerationMenu.EnableDomainWarping)
                await ApplyDomainWarpingAsync();

            if (_mapGenerationMenu.EnableTrees)
                await GenerateTrees();
        }
        catch (Exception e)
        {
            GD.PrintErr($"<PROCESS><ASYNC> - {nameof(GenerationPipelineAsync)} - ERROR: {e.Message}");
        }
        finally
        {
            await EnableGenerationOptions();
            await SetGenerationTipAsync(string.Empty);
            GD.Print($"<END><ASYNC> - {nameof(GenerationPipelineAsync)}");
            GD.Print("==============================================================");
        }
    }

    private async Task GenerateTerrainAsync(BaseGeneratorOptions generator)
    {
        GD.Print($"<PROCESS><ASYNC> - {nameof(GenerationPipelineAsync)} - Generating terrain");
        await SetGenerationTipAsync("Generating map...");
        var map = generator.GenerateMap();
        _worldData.SetTerrain(map);
        await RedrawWithResizeTerrainAsync();
    }
    private async Task ApplyInfluenceAsync()
    {
        GD.Print($"<PROCESS><ASYNC> - {nameof(GenerationPipelineAsync)} - Applying influence");
        await SetGenerationTipAsync("Applying influence...");
        var map = _worldData.TerrainHeightMap;
        MapHelpers.MultiplyHeight(map, _mapGenerationMenu.CurNoiseInfluence);
        _worldData.SetTerrain(map);
        await RedrawTerrainAsync();
    }
    private async Task ApplyTerrainSmoothingAsync()
    {
        GD.Print($"<PROCESS><ASYNC> - {nameof(GenerationPipelineAsync)} - Smoothing");
        await SetGenerationTipAsync("Smoothing...");
        var map = _worldData.TerrainHeightMap;
        for (int i = 0; i < _mapGenerationMenu.CurSmoothCycles; i++)
        {
            GD.Print($"<PROCESS><ASYNC> - {nameof(GenerationPipelineAsync)} - Smoothing - iteration: {i + 1}/{_mapGenerationMenu.CurSmoothCycles}");
            map = MapHelpers.SmoothMap(map);
            _worldData.SetTerrain(map);
            await RedrawTerrainAsync();
        }
    }
    private async Task ApplyIslandsAsync()
    {
        GD.Print($"<PROCESS><ASYNC> - {nameof(GenerationPipelineAsync)} - Applying islands");
        await SetGenerationTipAsync("Making islands...");
        var map = _worldData.TerrainHeightMap;
        map = _mapGenerationMenu.IslandsApplier.ApplyIslands(map);
        _worldData.SetTerrain(map);
        await RedrawTerrainAsync();
    }
    private async Task ApplyDomainWarpingAsync()
    {
        GD.Print($"<PROCESS><ASYNC> - {nameof(ApplyDomainWarpingAsync)} - Applying domain warping");
        await SetGenerationTipAsync("Applying domain warping...");
        var map = _worldData.TerrainHeightMap;
        map = _mapGenerationMenu.DomainWarpingApplier.ApplyWarping(map);
        _worldData.SetTerrain(map);
        await RedrawTerrainAsync();
    }
    private async Task GenerateTrees()
    {
        GD.Print($"<PROCESS><ASYNC> - {nameof(GenerateTrees)} - Generating trees");
        await SetGenerationTipAsync("Generating trees...");
        var trees = _treePlacementOptions.GenerateTrees(_worldData);
        _worldData.SetTreeMaps(trees);
        var treesColors = _treePlacementOptions.GetTreesColors();
        _displayOptions.TreeColors = treesColors;
        await RedrawTreesAsync();
    }

    private async Task DisableGenerationOptions()
    {
        GD.Print($"<START><ASYNC> - {nameof(DisableGenerationOptions)}");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _mapGenerationMenu.DisableAllOptions();
        GD.Print($"<END><ASYNC> - {nameof(DisableGenerationOptions)}");
    }
    private async Task EnableGenerationOptions()
    {
        GD.Print($"<START><ASYNC> - {nameof(EnableGenerationOptions)}");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _mapGenerationMenu.EnableAllOptions();
        GD.Print($"<END><ASYNC> - {nameof(EnableGenerationOptions)}");
    }
    private async Task Clear2DSceneAsync()
    {
        GD.Print($"<START><ASYNC> - {nameof(Clear2DSceneAsync)}");
        _terrainScene2D.ClearTerrainImage();
        _terrainScene2D.ClearTreesImage();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.UpdateTerrainTextureWithImage();
        _terrainScene2D.UpdateTreesTextureWithImage();
        GD.Print($"<END><ASYNC> - {nameof(Clear2DSceneAsync)}");
    }


    private async Task RedrawWithResizeTerrainAsync()
    {
        GD.Print($"<START><ASYNC> - {nameof(RedrawWithResizeTerrainAsync)}");
        _terrainScene2D.HandleTerrainImageRedraw();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.SetTerrainImageToTexture();
        GD.Print($"<END><ASYNC> - {nameof(RedrawWithResizeTerrainAsync)}");
    }
    private async Task RedrawTerrainAsync()
    {
        GD.Print($"<START><ASYNC> - {nameof(RedrawTerrainAsync)}");
        _terrainScene2D.HandleTerrainImageRedraw();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.UpdateTerrainTextureWithImage();
        GD.Print($"<END><ASYNC> - {nameof(RedrawTerrainAsync)}");
    }
    private async Task RedrawTreesAsync()
    {
        GD.Print($"<START><ASYNC> - {nameof(RedrawTreesAsync)}");
        _terrainScene2D.HandleTreesImageRedraw();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.UpdateTreesTextureWithImage();
        GD.Print($"<END><ASYNC> - {nameof(RedrawTreesAsync)}");
    }
    private async Task SetGenerationTipAsync(string tip)
    {
        GD.Print($"<START><ASYNC> - <{nameof(SetGenerationTipAsync)}> - Generation tip: {tip}");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.SetTip(tip);
        GD.Print($"<END><ASYNC> - <{nameof(SetGenerationTipAsync)}> - Generation tip: {tip}");
    }

    private void Log(string text, [CallerMemberName] string callerName = "")
    {
        GD.Print($"<{nameof(Game)}><{callerName}> - {text}");
    }
}