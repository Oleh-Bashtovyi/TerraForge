using Godot;
using System;
using System.Threading.Tasks;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;
using TerrainGenerationApp.Domain.Visualization;
using TerrainGenerationApp.Scenes.GameComponents.DisplayOptions;
using TerrainGenerationApp.Scenes.GameComponents.GenerationMenu;
using TerrainGenerationApp.Scenes.GenerationOptions;
using TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement;

namespace TerrainGenerationApp.Scenes.GameComponents.Game;

public partial class Game : Node3D, IWorldDataProvider, IWorldVisualizationSettingsProvider
{
    private TerrainScene2D.TerrainScene2D _terrainScene2D;
    private TerrainScene3D.TerrainScene3D _terrainScene3D;
    private TreePlacementOptions _treePlacementOptions;
    private MapGenerationMenu _mapGenerationMenu;
    private TerrainVisualizationOptions _terrainVisualizationOptions;
    private Button _generateMapButton;
    private Button _applyWaterErosionButton;
    private Button _showIn2DButton;
    private Button _showIn3DButton;

    private readonly Logger<Game> _logger = new();
    private readonly WorldData _worldData = new();
    private readonly WorldVisualizationSettings _worldVisualizationSettings = new();
    private Task? _generationTask;
    private Task? _waterErosionTask;

    public IWorldData WorldData => _worldData;
    public IWorldVisualizationSettings Settings => _worldVisualizationSettings;

    public override void _Ready()
    {
        _terrainScene2D = GetNode<TerrainScene2D.TerrainScene2D>("%TerrainScene2D");
        _terrainScene3D = GetNode<TerrainScene3D.TerrainScene3D>("%TerrainScene3D");
        _mapGenerationMenu = GetNode<MapGenerationMenu>("%MapGenerationMenu");
        _terrainVisualizationOptions = GetNode<TerrainVisualizationOptions>("%MapDisplayOptions");
        _generateMapButton = GetNode<Button>("%GenerateMapButton");
        //_applyWaterErosionButton = GetNode<Button>("%ApplyWaterErosionButton");
        _showIn2DButton = GetNode<Button>("%ShowIn2DButton");
        _showIn3DButton = GetNode<Button>("%ShowIn3DButton");
        _showIn2DButton.Pressed += _showIn2DButton_Pressed;
        _showIn3DButton.Pressed += _showIn3DButton_Pressed;

        _worldData.SetSeaLevel(_mapGenerationMenu.CurSeaLevel);
        _terrainScene3D.SetWorldDataProvider(this);
        _terrainScene2D.SetWorldDataProvider(this);
        _terrainScene2D.SetDisplayOptionsProvider(this);
        _terrainScene3D.SetDisplayOptionsProvider(this);

        _treePlacementOptions = _mapGenerationMenu.TreePlacementOptions;
        _treePlacementOptions.OnTreePlacementRuleItemAdded += TreePlacementOptionsOnOnTreePlacementRuleItemAdded;
        //_treePlacementOptions.OnTreePlacementRuleItemRemoved += TreePlacementOptionsOnOnTreePlacementRuleItemRemoved;
        //_treePlacementOptions.OnTreePlacementRulesChanged += TreePlacementOptionsOnOnTreePlacementRulesChanged;







        // TODO: Populate terrain visualization settings with colors for gradients
        // !!!
        // !!!
        // !!!
        // !!!
        // !!!

        foreach (var item in ColorPallets.DefaultTerrainColors)
        {
            _worldVisualizationSettings.TerrainSettings.AddTerrainGradientPoint(item.Key, item.Value);
        }

        foreach (var item in ColorPallets.DefaultWaterColors)
        {
            _worldVisualizationSettings.TerrainSettings.AddWaterGradientPoint(item.Key, item.Value);
        }





        _mapGenerationMenu.OnWaterLevelChanged += MapGenerationMenuOnOnWaterLevelChanged;
        //_mapGenerationMenu.GenerationParametersChanged += MapGenerationMenuOnGenerationParametersChanged;
        _terrainVisualizationOptions.OnDisplayOptionsChanged += TerrainVisualizationOptionsOnOnTerrainVisualizationOptionsChanged;
        _generateMapButton.Pressed += GenerateMapButtonOnPressed;
        //_applyWaterErosionButton.Pressed += ApplyWaterErosionButtonOnPressed;
    }

    private void _showIn3DButton_Pressed()
    {
        _terrainScene2D.Visible = false;
    }

    private void _showIn2DButton_Pressed()
    {
        _terrainScene2D.Visible = true;
    }

    private void ApplyWaterErosionButtonOnPressed()
    {
        _applyWaterErosionButton.Disabled = true;

        _waterErosionTask = Task.Run(WaterErosionPipelineAsync).ContinueWith(async t =>
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            _applyWaterErosionButton.Disabled = false;
        });
    }

    private async Task WaterErosionPipelineAsync()
    {
        try
        {
            GD.Print("==============================================================");
            _logger.LogMethodStart();

            // TODO: Implement water erosion pipeline

        }
        catch (Exception e)
        {
            _logger.Log($"<ERROR>: {e.Message}");
        }
        finally
        {
            await EnableGenerationOptions();
            await SetGenerationTitleTipAsync(string.Empty);
            _logger.LogMethodEnd();
            GD.Print("==============================================================");
        }
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
        if (_worldData.TreesData.HasLayer(e.TreeId))
        {
            _worldVisualizationSettings.TreeSettings.SetTreesLayerColor(e.TreeId, e.NewColor);
            _terrainScene2D.RedrawTreesImage();
            _terrainScene2D.UpdateTreesTexture();
        }
    }

    private void TerrainVisualizationOptionsOnOnTerrainVisualizationOptionsChanged()
    {
        _terrainScene2D.RedrawAllImages();
        _terrainScene2D.UpdateAllTextures();
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
            WorldData.TerrainData.Clear();
            WorldData.TreesData.ClearLayers();

            await GenerateTerrainAsync(generator);
            await ApplyMapInterpolation();
            await ApplyInfluenceAsync();
            await ApplyTerrainSmoothingAsync();

            if (_mapGenerationMenu.EnableIslands)
                await ApplyIslandsAsync();

            if (_mapGenerationMenu.EnableDomainWarping)
                await ApplyDomainWarpingAsync();

            if (_mapGenerationMenu.EnableTrees)
                await GenerateTrees();

            await GenerateMeshAsync();
        }
        catch (Exception e)
        {
            _logger.Log($"<ERROR>: {e.Message}");
        }
        finally
        {
            await EnableGenerationOptions();
            await SetGenerationTitleTipAsync(string.Empty);
            _logger.LogMethodEnd();
            GD.Print("==============================================================");
        }
    }
    private async Task GenerateTerrainAsync(BaseGeneratorOptions generator)
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Generating map...");
        var map = generator.GenerateMap();
        _worldData.TerrainData.SetTerrain(map);
        await RedrawTerrainAsync();
        _logger.LogMethodEnd();
    }

    private async Task ApplyMapInterpolation()
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Applying interpolation...");
        var map = _worldData.TerrainData.HeightMap;
        _mapGenerationMenu.ApplySelectedInterpolation(map);
        _worldData.TerrainData.SetTerrain(map);
        await RedrawTerrainAsync();
        _logger.LogMethodEnd();
    }
    private async Task ApplyInfluenceAsync()
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Applying influence...");
        var map = _worldData.TerrainData.HeightMap;
        MapHelpers.MultiplyHeight(map, _mapGenerationMenu.CurNoiseInfluence);
        _worldData.TerrainData.SetTerrain(map);
        await RedrawTerrainAsync();
        _logger.LogMethodEnd();
    }
    private async Task ApplyTerrainSmoothingAsync()
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Smoothing...");
        var map = _worldData.TerrainData.HeightMap;
        for (int i = 0; i < _mapGenerationMenu.CurSmoothCycles; i++)
        {
            _logger.Log($"Smoothing - iteration: {i + 1}/{_mapGenerationMenu.CurSmoothCycles}");
            map = MapHelpers.SmoothMap(map);
            _worldData.TerrainData.SetTerrain(map);
            await RedrawTerrainAsync();
        }
        _logger.LogMethodEnd();
    }
    private async Task ApplyIslandsAsync()
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Making islands...");
        var map = _worldData.TerrainData.HeightMap;
        map = _mapGenerationMenu.IslandsApplier.ApplyIslands(map);
        _worldData.TerrainData.SetTerrain(map);
        await RedrawTerrainAsync();
        _logger.LogMethodEnd();
    }
    private async Task ApplyDomainWarpingAsync()
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Applying domain warping...");
        var map = _worldData.TerrainData.HeightMap;
        map = _mapGenerationMenu.DomainWarpingApplier.ApplyWarping(map);
        _worldData.TerrainData.SetTerrain(map);
        await RedrawTerrainAsync();
        _logger.LogMethodEnd();
    }
    private async Task GenerateTrees()
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Generating trees...");
        var trees = _treePlacementOptions.GenerateTrees(_worldData);
        _worldData.TreesData.SetLayers(trees);
        var treesColors = _treePlacementOptions.GetTreesColors();
        var treesModels = _treePlacementOptions.GetTreesModels();

        _worldVisualizationSettings.TreeSettings.ClearTreesLayersColors();
        _worldVisualizationSettings.TreeSettings.ClearTreesLayersScenes();
        _worldVisualizationSettings.TreeSettings.SetTreeLayersColors(treesColors);
        _worldVisualizationSettings.TreeSettings.SetTreeLayersScenes(treesModels);
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
        _terrainScene2D.ClearAllImages();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.UpdateAllTextures();
        _logger.LogMethodEnd();
    }
    private async Task RedrawTerrainAsync()
    {
        _logger.LogMethodStart();
        _terrainScene2D.RedrawTerrainImage();
        _logger.Log("Waiting for Process frame...");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.UpdateTerrainTexture();
        _logger.LogMethodEnd();
    }
    private async Task RedrawTreesAsync()
    {
        _logger.LogMethodStart();
        _terrainScene2D.RedrawTreesImage();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.UpdateTreesTexture();
        _logger.LogMethodEnd();
    }
    private async Task SetGenerationTitleTipAsync(string tip)
    {
        _logger.Log($"Generation title tip: {tip}", LogMark.Start);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.SetTitleTip(tip);
        _logger.Log($"Generation title tip: {tip}", LogMark.End);
    }
    private async Task GenerateMeshAsync()
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Generating mesh...");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene3D.GenerateMesh();
        _logger.LogMethodEnd();
    }
}
