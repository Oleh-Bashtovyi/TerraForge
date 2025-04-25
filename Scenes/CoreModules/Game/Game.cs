using Godot;
using System;
using System.Threading.Tasks;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;
using TerrainGenerationApp.Domain.Visualization;
using TerrainGenerationApp.Scenes.CoreModules.DisplayOptions;
using TerrainGenerationApp.Scenes.CoreModules.GenerationMenu;
using TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement;
using TerrainGenerationApp.Scenes.GeneratorOptions;

namespace TerrainGenerationApp.Scenes.CoreModules.Game;

public partial class Game : Node3D
{
    private TerrainScene2D.TerrainScene2D _terrainScene2D;
    private TerrainScene3D.TerrainScene3D _terrainScene3D;
    private TreePlacementOptions _treePlacementOptions;
    private MapGenerationMenu _mapGenerationMenu;
    private TerrainVisualOptions _terrainVisualOptions;
    private TerrainMeshOptions _terrainMeshOptions;
    private Button _generateMapButton;
    private Button _applyWaterErosionButton;
    private Button _showIn2DButton;
    private Button _showIn3DButton;
    private FileDialog _saveFileDialog;
    private FileDialog _loadFileDialog;
    private MenuButton _fileMenuButton;

    private readonly Logger<Game> _logger = new();
    private readonly WorldData _worldData = new();
    private readonly WorldVisualSettings _worldVisualSettings = new();
    private readonly TerrainMeshSettings _terrainMeshSettings = new();
    private float[,] _curHeightMap;
    private Task? _generationTask;
    private Task? _waterErosionTask;

    public IWorldData WorldData => _worldData;
    public IWorldVisualSettings VisualSettings => _worldVisualSettings;

    public override void _Ready()
    {
        _terrainScene2D = GetNode<TerrainScene2D.TerrainScene2D>("%TerrainScene2D");
        _terrainScene3D = GetNode<TerrainScene3D.TerrainScene3D>("%TerrainScene3D");
        _mapGenerationMenu = GetNode<MapGenerationMenu>("%MapGenerationMenu");
        _terrainVisualOptions = GetNode<TerrainVisualOptions>("%DisplayOptions");
        _generateMapButton = GetNode<Button>("%GenerateMapButton");
        _terrainMeshOptions = GetNode<TerrainMeshOptions>("%MeshOptions");
        _fileMenuButton = GetNode<MenuButton>("%FileMenuButton");
        _showIn2DButton = GetNode<Button>("%ShowIn2DButton");
        _showIn3DButton = GetNode<Button>("%ShowIn3DButton");
        _showIn2DButton.Pressed += _showIn2DButton_Pressed;
        _showIn3DButton.Pressed += _showIn3DButton_Pressed;

        _worldData.SetSeaLevel(_mapGenerationMenu.CurSeaLevel);
        _terrainScene3D.BindWorldData(WorldData);
        _terrainScene2D.BindWorldData(WorldData);
        _terrainScene2D.BindVisualSettings(VisualSettings);
        _terrainScene3D.BindDisplayOptions(VisualSettings);
        _terrainScene3D.BindMeshSettings(_terrainMeshSettings);
        _terrainMeshOptions.BindSettings(_terrainMeshSettings);
        _terrainVisualOptions.BindSettings(_worldVisualSettings.TerrainSettings);

        _treePlacementOptions = _mapGenerationMenu.TreePlacementOptions;
        _treePlacementOptions.OnTreePlacementRuleItemAdded += TreePlacementOptionsOnOnTreePlacementRuleItemAdded;
        //_treePlacementOptions.OnTreePlacementRuleItemRemoved += TreePlacementOptionsOnOnTreePlacementRuleItemRemoved;
        //_treePlacementOptions.OnTreePlacementRulesChanged += TreePlacementOptionsOnOnTreePlacementRulesChanged;

        foreach (var item in ColorPallets.DefaultTerrainColors)
        {
            _worldVisualSettings.TerrainSettings.AddTerrainGradientPoint(item.Key, item.Value);
        }

        foreach (var item in ColorPallets.DefaultWaterColors)
        {
            _worldVisualSettings.TerrainSettings.AddWaterGradientPoint(item.Key, item.Value);
        }

        _fileMenuButton.GetPopup().AddItem("Load", id: 1);
        _fileMenuButton.GetPopup().AddItem("Save", id: 2);
        _fileMenuButton.GetPopup().IdPressed += (long id) =>
        {
            if (id == 1)
            {
                LoadTerrain();
            }
            else if (id == 2)
            {
                SaveTerrain();
            }
        };

        _saveFileDialog = new FileDialog();
        _saveFileDialog.Title = "Select save file location";
        _saveFileDialog.Filters = ["*.json"];
        _saveFileDialog.FileMode = FileDialog.FileModeEnum.SaveFile;
        _saveFileDialog.FileSelected += SaveFileDialogOnFileSelected;

        _loadFileDialog = new FileDialog();
        _loadFileDialog.Title = "Select file to load";
        _loadFileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        _loadFileDialog.Filters = ["*.json"];
        _loadFileDialog.FileSelected += LoadFileDialogOnFileSelected;
        AddChild(_saveFileDialog);
        AddChild(_loadFileDialog);

        _mapGenerationMenu.OnWaterLevelChanged += MapGenerationMenuOnOnWaterLevelChanged;
        _terrainVisualOptions.OnDisplayOptionsChanged += TerrainVisualOptionsOnOnTerrainVisualOptionsChanged;
        _generateMapButton.Pressed += GenerateMapButtonOnPressed;
    }

    private void LoadFileDialogOnFileSelected(string path)
    {
        var loadFile = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        var data = loadFile.GetAsText();

        //_worldData.LoadFromJson(data);
        var parsedData = WorldDataParser.LoadFromJson(data);
        _worldData.TerrainData.SetTerrain(parsedData.HeightMap);
        _worldData.TreesData.SetLayers(parsedData.TreeLayersDict);
        _worldData.SetSeaLevel(parsedData.SeaLevel);
        _mapGenerationMenu.LoadConfigFrom(parsedData.GenerationConfiguration);
        var treesColors = _treePlacementOptions.GetTreesColors();
        var treesModels = _treePlacementOptions.GetTreesModels();
        _worldVisualSettings.TreeSettings.ClearTreesLayersColors();
        _worldVisualSettings.TreeSettings.ClearTreesLayersScenes();
        _worldVisualSettings.TreeSettings.SetTreeLayersColors(treesColors);
        _worldVisualSettings.TreeSettings.SetTreeLayersScenes(treesModels);

        _terrainScene2D.RedrawAllImages();
        _terrainScene2D.UpdateAllTextures();
        _terrainScene3D.ClearWorld();
    }

    private void SaveFileDialogOnFileSelected(string path)
    {
        _logger.Log(path);

        var generationConfig = _mapGenerationMenu.GetLastUsedConfig();

        var data = _worldData.ToJson(generationConfig);

        var saveFile = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        saveFile.StoreLine(data);
    }

    private void SaveTerrain()
    {
        if (!_saveFileDialog.Visible)
        {
            _saveFileDialog.Show();
        }
    }

    private void LoadTerrain()
    {
        if (!_loadFileDialog.Visible)
        {
            _loadFileDialog.Show();
        }
    }



    private void _showIn3DButton_Pressed()
    {
        // This is a workaround to hide the 2D scene, because if we use Visible = false, then
        // the right child of HSplitContainer will occupy the entire screen, hiding the 3D scene.
        var c = _terrainScene2D.Modulate;
        c.A = 0f;
        _terrainScene2D.Modulate = c;
        //_terrainScene2D.Visible = false;
    }

    private void _showIn2DButton_Pressed()
    {
        // This is a workaround to hide the 2D scene, because if we use Visible = false, then
        // the right child of HSplitContainer will occupy the entire screen, hiding the 3D scene.
        var c = _terrainScene2D.Modulate;
        c.A = 1f;
        _terrainScene2D.Modulate = c;
        //_terrainScene2D.Visible = true;
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

    private void TreePlacementRuleItemOnTreeColorChanged(object sender, TreeLayerColorChangedEventArgs e)
    {
        if (_worldData.TreesData.HasLayer(e.LayerId))
        {
            _worldVisualSettings.TreeSettings.SetTreesLayerColor(e.LayerId, e.NewColor);
            _terrainScene2D.RedrawTreesImage();
            _terrainScene2D.UpdateTreesTexture();
        }
    }

    private void TerrainVisualOptionsOnOnTerrainVisualOptionsChanged()
    {
        _terrainScene2D.RedrawAllImages();
        _terrainScene2D.UpdateAllTextures();
        
        _terrainScene3D.RedrawChunks();
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

            await GenerateTerrainAsync();

            _mapGenerationMenu.UpdateCurrentConfigAsLastUsed();
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
        _curHeightMap = generator.GenerateMap();
        _worldData.TerrainData.SetTerrain(_curHeightMap);
        await RedrawTerrainAsync();
        _logger.LogMethodEnd();
    }

    private async Task ApplyMapInterpolation()
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Applying interpolation...");
        _mapGenerationMenu.ApplySelectedInterpolation(_curHeightMap);
        _worldData.TerrainData.SetTerrain(_curHeightMap);
        await RedrawTerrainAsync();
        _logger.LogMethodEnd();
    }
    private async Task ApplyInfluenceAsync()
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Applying influence...");
        MapHelpers.MultiplyHeight(_curHeightMap, _mapGenerationMenu.CurNoiseInfluence);
        _worldData.TerrainData.SetTerrain(_curHeightMap);
        await RedrawTerrainAsync();
        _logger.LogMethodEnd();
    }
    private async Task ApplyTerrainSmoothingAsync()
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Smoothing...");
        for (int i = 0; i < _mapGenerationMenu.CurSmoothCycles; i++)
        {
            _logger.Log($"Smoothing - iteration: {i + 1}/{_mapGenerationMenu.CurSmoothCycles}");
            _curHeightMap = MapHelpers.SmoothMap(_curHeightMap);
            _worldData.TerrainData.SetTerrain(_curHeightMap);
            await RedrawTerrainAsync();
        }
        _logger.LogMethodEnd();
    }
    private async Task ApplyIslandsAsync()
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Making islands...");
        _curHeightMap = _mapGenerationMenu.IslandsApplier.ApplyIslands(_curHeightMap);
        _worldData.TerrainData.SetTerrain(_curHeightMap);
        await RedrawTerrainAsync();
        _logger.LogMethodEnd();
    }
    private async Task ApplyDomainWarpingAsync()
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Applying domain warping...");
        _curHeightMap = _mapGenerationMenu.DomainWarpingApplier.ApplyWarping(_curHeightMap);
        _worldData.TerrainData.SetTerrain(_curHeightMap);
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
        _worldVisualSettings.TreeSettings.ClearTreesLayersColors();
        _worldVisualSettings.TreeSettings.ClearTreesLayersScenes();
        _worldVisualSettings.TreeSettings.SetTreeLayersColors(treesColors);
        _worldVisualSettings.TreeSettings.SetTreeLayersScenes(treesModels);
        await RedrawTreesAsync();
        _logger.LogMethodEnd();
    }
    private async Task DisableGenerationOptions()
    {
        _logger.LogMethodStart();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _mapGenerationMenu.DisableOptions();
        _logger.LogMethodEnd();
    }
    private async Task EnableGenerationOptions()
    {
        _logger.LogMethodStart();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _mapGenerationMenu.EnableOptions();
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
    private async Task GenerateTerrainAsync()
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Generating chunks...");
        await ClearChunks();
        await _terrainScene3D.InitWater();

        while (_terrainScene3D.IsTerrainChunksGenerating())
        {
            _terrainScene3D.GenerateNextChunk();
            await _terrainScene3D.ApplyCurrentChunkAsync();
        }

        while (_terrainScene3D.IsTreeChunksGenerating())
        {
            _terrainScene3D.GenerateNextTreeChunk();
            await _terrainScene3D.ApplyCurrentTreeChunkAsync();
        }

        _logger.LogMethodEnd();
    }

    private async Task ClearChunks()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene3D.ClearWorld();
    }
}
