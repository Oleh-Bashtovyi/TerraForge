using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TerrainGenerationApp.Scenes.GameComponents.DisplayOptions;
using TerrainGenerationApp.Scenes.GameComponents.GenerationMenu;
using TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Scenes.GameComponents.Game;

public partial class Game : Node3D
{
    private Display2D.TerrainScene2D _terrainScene2D;
    private TreePlacementOptions _treePlacementOptions;
    private MapGenerationMenu _mapGenerationMenu;
    private MapDisplayOptions _displayOptions;
    private Button _generateMapButton;

    private WorldData _worldData;
    private GodotThread _waterErosionThread;
    private GodotThread _generationThread;


    private Dictionary<string, Color> _treeColors = new();

    public override void _Ready()
    {
        _terrainScene2D = GetNode<Display2D.TerrainScene2D>("%TerrainScene2D");
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



    private Semaphore _generationSemaphore = new Semaphore();
/*    private void GenerationPipeline()
    {
        var generator = _mapGenerationMenu.SelectedGenerator;
        
        if (generator == null)
        {
            return;
        }
        GD.Print("!!!> GENERATION PIPELINE STARTED!");
        GD.Print("==============================================================");
        GD.Print("Clearing display screen...");
        CallThreadSafe(nameof(Clear2DScene));
        _generationSemaphore.Wait();

        GD.Print("Generating map...");
        CallThreadSafe(nameof(SetGenerationTip), "Generating map...");
        var map = generator.GenerateMap();
        _worldData.SetTerrain(map);
        CallThreadSafe(nameof(RedrawTerrain));
        _generationSemaphore.Wait();
        
        GD.Print("Applying influence...");
        CallThreadSafe(nameof(SetGenerationTip), "Applying influence...");
        MapHelpers.MultiplyHeight(map, _mapGenerationMenu.CurNoiseInfluence);
        _worldData.SetTerrain(map);
        CallThreadSafe(nameof(RedrawTerrain));
        _generationSemaphore.Wait();
        
        GD.Print("Smoothing...");
        CallThreadSafe(nameof(SetGenerationTip), "Smoothing...");
        for (int i = 0; i < _mapGenerationMenu.CurSmoothCycles; i++)
        {
            GD.Print($"Smooth iterations: {i + 1}/{_mapGenerationMenu.CurSmoothCycles}");
            map = MapHelpers.SmoothMap(map);
            _worldData.SetTerrain(map);
            CallThreadSafe(nameof(RedrawTerrain));
            _generationSemaphore.Wait();
        }

        if (_mapGenerationMenu.EnableIslands)
        {
            GD.Print("Making islands...");
            CallThreadSafe(nameof(SetGenerationTip), "Making islands...");
            map = _mapGenerationMenu.IslandsApplier.ApplyIslands(map);
            _worldData.SetTerrain(map);
            CallThreadSafe(nameof(RedrawTerrain));
            _generationSemaphore.Wait();
        }

        if (_mapGenerationMenu.EnableDomainWarping)
        {
            GD.Print("Applying domain warping...");
            CallThreadSafe(nameof(SetGenerationTip), "Applying domain warping...");
            map = _mapGenerationMenu.DomainWarpingApplier.ApplyWarping(map);
            _worldData.SetTerrain(map);
            CallThreadSafe(nameof(RedrawTerrain));
            _generationSemaphore.Wait();
        }
    }*/




    //private void Clear2DScene()
    //{
    //    _terrainScene2D.PaintTerrainMapInBlack();
    //    _generationSemaphore.Post();
    //    //_terrainScene2D.ClearTrees();
    //}
    //private void RedrawTerrain()
    //{
    //    _terrainScene2D.ResizeMap(_worldData);
    //    //_terrainScene2D.RedrawTerrain(_worldData);
    //    _generationSemaphore.Post();
    //}
    //private void RedrawTreeLayers()
    //{
    //    //_terrainScene2D.RedrawTreeLayers(_worldData);
    //    _generationSemaphore.Post();
    //}
    //private void SetGenerationTip(string tip)
    //{
    //    _terrainScene2D.SetTip(tip);
    //}






    private void MapGenerationMenuOnOnWaterLevelChanged(object sender, EventArgs e)
    {
        _worldData.SetSeaLevel(_mapGenerationMenu.CurSeaLevel);
        GD.Print("SEA LEVEL CHANGED EVENT HANDLED!");
        //_terrainScene2D.RedrawTerrain(_worldData);
    }

    private void MapGenerationMenuOnGenerationParametersChanged(object sender, EventArgs e)
    {
        //RegenerateMap();
    }

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
        _treeColors[e.TreeId] = e.TreeColor;

        if (_mapGenerationMenu.RegenerateOnParametersChanged && _mapGenerationMenu.EnableTrees)
        {
            GD.Print("---> GAME ---> Regenerating trees...");
            var trees = _treePlacementOptions.GenerateTrees(_worldData);
            _worldData.SetTreeMaps(trees);
            //_terrainScene2D.RedrawTerrain(_worldData);
            //_terrainScene2D.RedrawTreeLayers(_worldData);
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
            GD.Print($"<START><ASYNC> - {nameof(GenerationPipelineAsync)}");

            var generator = _mapGenerationMenu.SelectedGenerator;

            if (generator == null)
            {
                return;
            }

            await DisableGenerationOptions();
            await Clear2DSceneAsync();

            GD.Print($"<PROCESS><ASYNC> - {nameof(GenerationPipelineAsync)} - Generating terrain");
            await SetGenerationTipAsync("Generating map...");   
            var map = generator.GenerateMap();
            _worldData.SetTerrain(map);
            await RedrawWithResizeTerrainAsync();

            GD.Print($"<PROCESS><ASYNC> - {nameof(GenerationPipelineAsync)} - Applying influence");
            await SetGenerationTipAsync("Applying influence...");
            MapHelpers.MultiplyHeight(map, _mapGenerationMenu.CurNoiseInfluence);
            _worldData.SetTerrain(map);
            await RedrawTerrainAsync();

            GD.Print($"<PROCESS><ASYNC> - {nameof(GenerationPipelineAsync)} - Smoothing");
            await SetGenerationTipAsync("Smoothing...");
            for (int i = 0; i < _mapGenerationMenu.CurSmoothCycles; i++)
            {
                GD.Print($"<PROCESS><ASYNC> - {nameof(GenerationPipelineAsync)} - Smoothing - iteration: " +
                         $"{i + 1}/{_mapGenerationMenu.CurSmoothCycles}");
                map = MapHelpers.SmoothMap(map);
                _worldData.SetTerrain(map);
                await RedrawTerrainAsync();
            }

            if (_mapGenerationMenu.EnableIslands)
            {
                GD.Print($"<PROCESS><ASYNC> - {nameof(GenerationPipelineAsync)} - Applying islands");
                await SetGenerationTipAsync("Making islands...");
                map = _mapGenerationMenu.IslandsApplier.ApplyIslands(map);
                _worldData.SetTerrain(map);
                await RedrawTerrainAsync();
            }

            if (_mapGenerationMenu.EnableDomainWarping)
            {
                GD.Print($"<PROCESS><ASYNC> - {nameof(GenerationPipelineAsync)} - Applying domain warping");
                await SetGenerationTipAsync("Applying domain warping...");
                map = _mapGenerationMenu.DomainWarpingApplier.ApplyWarping(map);
                _worldData.SetTerrain(map);
                await RedrawTerrainAsync();
            }
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
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.ClearTerrainImage();
        _terrainScene2D.UpdateTerrainTextureWithImage();
        GD.Print($"<END><ASYNC> - {nameof(Clear2DSceneAsync)}");
    }
    private async Task RedrawWithResizeTerrainAsync()
    {
        GD.Print($"<START><ASYNC> - {nameof(RedrawWithResizeTerrainAsync)}");
        _terrainScene2D.ResizeTerrainImage(_worldData);
        _terrainScene2D.UpdateTerrainImage(_worldData);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.SetTerrainTextureWithImage();
        GD.Print($"<END><ASYNC> - {nameof(RedrawWithResizeTerrainAsync)}");
    }
    private async Task RedrawTerrainAsync()
    {
        GD.Print($"<START><ASYNC> - {nameof(RedrawTerrainAsync)}");
        _terrainScene2D.UpdateTerrainImage(_worldData);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.UpdateTerrainTextureWithImage();
        GD.Print($"<END><ASYNC> - {nameof(RedrawTerrainAsync)}");
    }
    private async Task SetGenerationTipAsync(string tip)
    {
        GD.Print($"<START><ASYNC> - <{nameof(SetGenerationTipAsync)}> - Generation tip: {tip}");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.SetTip(tip);
        GD.Print($"<END><ASYNC> - <{nameof(SetGenerationTipAsync)}> - Generation tip: {tip}");
    }
}