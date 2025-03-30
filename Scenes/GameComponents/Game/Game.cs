using Godot;
using System;
using System.Threading.Tasks;
using TerrainGenerationApp.Enums;
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
    private Logger<Game> _logger;
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
        _logger = new();
        _worldData.SetSeaLevel(_mapGenerationMenu.CurSeaLevel);
        _terrainScene2D.SetWorldDataProvider(this);
        _terrainScene2D.SetDisplayOptionsProvider(_displayOptions);

        _treePlacementOptions = _mapGenerationMenu.TreePlacementOptions;
        _treePlacementOptions.OnTreePlacementRuleItemAdded += TreePlacementOptionsOnOnTreePlacementRuleItemAdded;
        //_treePlacementOptions.OnTreePlacementRuleItemRemoved += TreePlacementOptionsOnOnTreePlacementRuleItemRemoved;
        //_treePlacementOptions.OnTreePlacementRulesChanged += TreePlacementOptionsOnOnTreePlacementRulesChanged;


        _mapGenerationMenu.OnWaterLevelChanged += MapGenerationMenuOnOnWaterLevelChanged;
        //_mapGenerationMenu.GenerationParametersChanged += MapGenerationMenuOnGenerationParametersChanged;
        _displayOptions.OnDisplayOptionsChanged += DisplayOptionsOnOnDisplayOptionsChanged;
        _generateMapButton.Pressed += GenerateMapButtonOnPressed;
    }

    private void MapGenerationMenuOnOnWaterLevelChanged(object sender, EventArgs e)
    {
        _worldData.SetSeaLevel(_mapGenerationMenu.CurSeaLevel);
    }

    private void TreePlacementOptionsOnOnTreePlacementRuleItemAdded(object sender, TreePlacementRuleItem e)
    {
        e.OnTreeColorChanged += TreePlacementRuleItemOnTreeColorChanged;
    }

    private void TreePlacementRuleItemOnTreeColorChanged(object sender, TreeColorChangedEventArgs e)
    {
        if (_displayOptions.TreeColors.ContainsKey(e.TreeId))
        {
            _displayOptions.TreeColors[e.TreeId] = e.NewColor;
            _terrainScene2D.HandleTreesImageRedraw();
            _terrainScene2D.UpdateTreesTextureWithImage();
        }
    }

    private void DisplayOptionsOnOnDisplayOptionsChanged()
    {
        _terrainScene2D.HandleAllImagesRedraw();
        _terrainScene2D.HandleAllTexturesUpdate();
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
            _logger.LogMethodStart();

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
            _logger.Log($"<ERROR>: {e.Message}");
        }
        finally
        {
            await EnableGenerationOptions();
            await SetGenerationTipAsync(string.Empty);
            _logger.LogMethodEnd();
            GD.Print("==============================================================");
        }
    }
    private async Task GenerateTerrainAsync(BaseGeneratorOptions generator)
    {
        _logger.LogMethodStart();
        await SetGenerationTipAsync("Generating map...");
        var map = generator.GenerateMap();
        _worldData.SetTerrain(map);
        await RedrawWithResizeTerrainAsync();
        _logger.LogMethodEnd();
    }
    private async Task ApplyInfluenceAsync()
    {
        _logger.LogMethodStart();
        await SetGenerationTipAsync("Applying influence...");
        var map = _worldData.TerrainHeightMap;
        MapHelpers.MultiplyHeight(map, _mapGenerationMenu.CurNoiseInfluence);
        _worldData.SetTerrain(map);
        await RedrawTerrainAsync();
        _logger.LogMethodEnd();
    }
    private async Task ApplyTerrainSmoothingAsync()
    {
        _logger.LogMethodStart();
        await SetGenerationTipAsync("Smoothing...");
        var map = _worldData.TerrainHeightMap;
        for (int i = 0; i < _mapGenerationMenu.CurSmoothCycles; i++)
        {
            _logger.Log($"Smoothing - iteration: {i + 1}/{_mapGenerationMenu.CurSmoothCycles}");
            map = MapHelpers.SmoothMap(map);
            _worldData.SetTerrain(map);
            await RedrawTerrainAsync();
        }
        _logger.LogMethodEnd();
    }
    private async Task ApplyIslandsAsync()
    {
        _logger.LogMethodStart();
        await SetGenerationTipAsync("Making islands...");
        var map = _worldData.TerrainHeightMap;
        map = _mapGenerationMenu.IslandsApplier.ApplyIslands(map);
        _worldData.SetTerrain(map);
        await RedrawTerrainAsync();
        _logger.LogMethodEnd();
    }
    private async Task ApplyDomainWarpingAsync()
    {
        _logger.LogMethodStart();
        await SetGenerationTipAsync("Applying domain warping...");
        var map = _worldData.TerrainHeightMap;
        map = _mapGenerationMenu.DomainWarpingApplier.ApplyWarping(map);
        _worldData.SetTerrain(map);
        await RedrawTerrainAsync();
        _logger.LogMethodEnd();
    }
    private async Task GenerateTrees()
    {
        _logger.LogMethodStart();
        await SetGenerationTipAsync("Generating trees...");
        var trees = _treePlacementOptions.GenerateTrees(_worldData);
        _worldData.SetTreeMaps(trees);
        var treesColors = _treePlacementOptions.GetTreesColors();
        _displayOptions.TreeColors = treesColors;
        await RedrawTreesAsync();
        _logger.LogMethodEnd();
    }
    private async Task DisableGenerationOptions()
    {
        _logger.LogMethodStart();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _mapGenerationMenu.DisableAllOptions();
        _logger.LogMethodEnd();
    }
    private async Task EnableGenerationOptions()
    {
        _logger.LogMethodStart();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _mapGenerationMenu.EnableAllOptions();
        _logger.LogMethodEnd();
    }
    private async Task Clear2DSceneAsync()
    {
        _logger.LogMethodStart();
        _terrainScene2D.HandleAllImagesClear();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.HandleAllTexturesUpdate();
        _logger.LogMethodEnd();
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
        _logger.LogMethodStart();
        _terrainScene2D.HandleTerrainImageRedraw();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.UpdateTerrainTextureWithImage();
        _logger.LogMethodEnd();
    }
    private async Task RedrawTreesAsync()
    {
        _logger.LogMethodStart();
        _terrainScene2D.HandleTreesImageRedraw();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.SetTreesImageToTexture();
        _logger.LogMethodEnd();
    }
    private async Task SetGenerationTipAsync(string tip)
    {
        _logger.Log($"Generation tip: {tip}", LogMark.Start);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.SetTip(tip);
        _logger.Log($"Generation tip: {tip}", LogMark.End);
    }
}
