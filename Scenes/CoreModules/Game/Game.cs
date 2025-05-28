using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;
using TerrainGenerationApp.Domain.Visualization;
using TerrainGenerationApp.Scenes.CoreModules.DisplayOptions;
using TerrainGenerationApp.Scenes.CoreModules.GenerationMenu;
using TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement;
using Environment = System.Environment;

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
    private Button _stopGenerationButton;
    private FileDialog _saveFileDialog;
    private FileDialog _loadFileDialog;
    private MenuButton _fileMenuButton;
    private Label _generationTitleTip;

    private readonly Logger<Game> _logger = new();
    private readonly WorldData _worldData = new();
    private readonly WorldVisualSettings _worldVisualSettings = new();
    private readonly TerrainMeshSettings _terrainMeshSettings = new();
    private CancellationTokenSource _cancellationTokenSource = new();
    private float[,] _curHeightMap;
    private bool _terrainRegenerationRequired = true;
    private Task? _generationTask;

    public IWorldData WorldData => _worldData;
    public IWorldVisualSettings VisualSettings => _worldVisualSettings;

    public override void _Ready()
    {
        _InitScenesReferences();
        _InitFileMenuButton();
        _InitSaveFileDialog();
        _InitLoadFileDialog();
        _InitButtonEvents();
        _InitMapColoring();
        _InitBindModels();
        _InitOptionEvents();
        OnGenerationParametersChanged();
    }

    private void _InitScenesReferences()
    {
        _terrainScene2D       = GetNode<TerrainScene2D.TerrainScene2D>("%TerrainScene2D");
        _terrainScene3D       = GetNode<TerrainScene3D.TerrainScene3D>("%TerrainScene3D");
        _mapGenerationMenu    = GetNode<MapGenerationMenu>("%MapGenerationMenu");
        _terrainVisualOptions = GetNode<TerrainVisualOptions>("%DisplayOptions");
        _generateMapButton    = GetNode<Button>("%GenerateMapButton");
        _terrainMeshOptions   = GetNode<TerrainMeshOptions>("%MeshOptions");
        _fileMenuButton       = GetNode<MenuButton>("%FileMenuButton");
        _showIn2DButton       = GetNode<Button>("%ShowIn2DButton");
        _showIn3DButton       = GetNode<Button>("%ShowIn3DButton");
        _stopGenerationButton = GetNode<Button>("%StopGenerationMapButton");
        _generationTitleTip   = GetNode<Label>("%GenerationStatusLabel");
        _treePlacementOptions = _mapGenerationMenu.TreePlacementOptions;
    }

    private void _InitFileMenuButton()
    {
        _fileMenuButton.GetPopup().AddItem("Load", id: 1);
        _fileMenuButton.GetPopup().AddItem("Save", id: 2);
        _fileMenuButton.GetPopup().IdPressed += (long id) =>
        {
            if (id == 1)
            {
                _loadFileDialog.Show();
            }
            else if (id == 2)
            {
                _saveFileDialog.Show();
            }
        };
    }

    private void _InitSaveFileDialog()
    {
        _saveFileDialog = new FileDialog();
        _saveFileDialog.Title = "Select save file location";
        _saveFileDialog.Filters = ["*.json", "*.png"];
        _saveFileDialog.Access = FileDialog.AccessEnum.Filesystem;
        _saveFileDialog.FileMode = FileDialog.FileModeEnum.SaveFile;
        _saveFileDialog.CurrentPath = Environment.GetFolderPath(
            Environment.SpecialFolder.MyDocuments,
            Environment.SpecialFolderOption.Create
        );

        _saveFileDialog.FileSelected += (path) => {
            
            _logger.Log($"trying to save data to: {path}");
            var extension = path.GetExtension();

            if (extension == "json")
            {
                var generationConfig = _mapGenerationMenu.GetLastUsedConfig();
                var worldData = _worldData.ToJson(generationConfig);
                var saveFile = FileAccess.Open(path, FileAccess.ModeFlags.Write);
                saveFile.StoreLine(worldData);
            }
            else if (extension == "png")
            {
                var image = _terrainScene2D.GetTerrainImage();
                image.SavePng(path);
            }
            else
            {
                _logger.Log($"Unsupported file extension: {path.GetExtension()}");
            }
        };

        AddChild(_saveFileDialog);
    }

    private void _InitLoadFileDialog()
    {
        _loadFileDialog = new FileDialog();
        _loadFileDialog.Title = "Select file to load";
        _loadFileDialog.Access = FileDialog.AccessEnum.Filesystem;
        _loadFileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        _loadFileDialog.Filters = ["*.json"];
        _loadFileDialog.CurrentPath = Environment.GetFolderPath(
            Environment.SpecialFolder.MyDocuments,
            Environment.SpecialFolderOption.Create
        );

        _loadFileDialog.FileSelected += (path) =>
        {
            var loadFile = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            var data = loadFile.GetAsText();
            var parsedData = WorldDataParser.LoadFromJson(data);
            _terrainRegenerationRequired = true;

            _logger.Log($"LOADING DATA....");
            _logger.Log($"Map size: {parsedData.HeightMap.Height()} x {parsedData.HeightMap.Width()}");
            _logger.Log($"Sea level: {parsedData.SeaLevel}");
            _logger.Log($"Tree layers count: {parsedData.TreeLayersDict.Count}");
            _worldData.TerrainData.SetTerrain(parsedData.HeightMap);
            _worldData.TreesData.SetLayers(parsedData.TreeLayersDict);
            _logger.Log($"Tree layers count after load: {_worldData.TreesData.GetLayersIds().Count}");
            _worldData.SetSeaLevel(parsedData.SeaLevel);
            _mapGenerationMenu.LoadConfigFrom(parsedData.GenerationConfiguration);
            var treesColors = _treePlacementOptions.GetTreesColors();
            var treesModels = _treePlacementOptions.GetTreesModels();
            _worldVisualSettings.TreeSettings.SetTreeLayersColors(treesColors);
            _worldVisualSettings.TreeSettings.SetTreeLayersScenes(treesModels);

            _terrainScene2D.RedrawAllImages();
            _terrainScene2D.UpdateAllTextures();
            _terrainScene3D.ClearWorld();
        };

        AddChild(_loadFileDialog);
    }

    private void _InitButtonEvents()
    {
        _showIn2DButton.Pressed += () =>
        {
            _showIn3DButton.Show();
            _showIn2DButton.Hide();
            ShowMap2D();
        };
        _showIn3DButton.Pressed += () =>
        {
            _showIn3DButton.Hide();
            _showIn2DButton.Show();
            HideMap2D();
        };
        _stopGenerationButton.Pressed += () =>
        {
            _logger.Log("Stopping generation pipeline...");
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
/*            _generationTask?.Wait();
            _generateMapButton.Disabled = false;
            _stopGenerationButton.Disabled = true;*/
        };
        _generateMapButton.Pressed += StartGenerationPipeline;
    }

    private void _InitBindModels()
    {
        _worldData.SetSeaLevel(_mapGenerationMenu.CurSeaLevel);
        _terrainScene3D.BindWorldData(WorldData);
        _terrainScene2D.BindWorldData(WorldData);
        _terrainScene2D.BindVisualSettings(VisualSettings);
        _terrainScene3D.BindDisplayOptions(VisualSettings);
        _terrainScene3D.BindMeshSettings(_terrainMeshSettings);
        _terrainMeshOptions.BindSettings(_terrainMeshSettings);
        _terrainVisualOptions.BindSettings(_worldVisualSettings.TerrainSettings);
    }

    private void _InitMapColoring()
    {
        foreach (var item in ColorPallets.DefaultTerrainColors)
        {
            _worldVisualSettings.TerrainSettings.AddTerrainGradientPoint(item.Key, item.Value);
        }

        foreach (var item in ColorPallets.DefaultWaterColors)
        {
            _worldVisualSettings.TerrainSettings.AddWaterGradientPoint(item.Key, item.Value);
        }
        foreach (var item in ColorPallets.MoistTerrainColors)
        {
            _worldVisualSettings.TerrainSettings.AddDryTerrainGradientPoint(item.Key, item.Value);
        }
    }

    private void _InitOptionEvents()
    {
        _treePlacementOptions.OnTreePlacementRuleItemAdded += (sender, e) =>
        {
            e.OnTreeColorChanged += TreePlacementRuleItemOnTreeColorChanged;
        };

        _mapGenerationMenu.GenerationParametersChanged += OnGenerationParametersChanged;
        
        _terrainMeshOptions.OnMeshOptionsChanged += () =>
        {
            _terrainRegenerationRequired = true;
        };

        _mapGenerationMenu.OnWaterLevelChanged += (newSeaLevel) =>
        {
            _worldData.SetSeaLevel(newSeaLevel);
            _terrainScene3D.UpdateWaterMesh();
            _terrainScene2D.RedrawTerrainImage();
            _terrainScene2D.UpdateTerrainTexture();
        };

/*        _mapGenerationMenu.OnRedrawOnParametersChangedChanged += (redraw) =>
        {
            OnGenerationParametersChanged();
        };*/
        
        _terrainVisualOptions.OnDisplayOptionsChanged += () =>
        {
            _terrainScene2D.RedrawAllImages();
            _terrainScene2D.UpdateAllTextures();
            _terrainScene3D.RedrawChunks();
        };
    }


    private void OnGenerationParametersChanged()
    {
        if (!_mapGenerationMenu.RedrawOnParametersChanged)
        {
            return;
        }

        _logger.Log("Parameters changed, trying to regenerate");
        _terrainRegenerationRequired = true;

        var generator = _mapGenerationMenu.SelectedGenerator;
        if (generator == null) return;

        WorldData.TerrainData.Clear();
        WorldData.TreesData.Clear();
        var map = generator.GenerateMap();

        _mapGenerationMenu.ApplySelectedInterpolation(map);
        MapHelpers.MultiplyHeight(map, _mapGenerationMenu.CurNoiseInfluence);
        
        for (int i = 0; i < _mapGenerationMenu.CurSmoothCycles; i++)
        {
            map = MapHelpers.SmoothMap(map);
        }

        if (_mapGenerationMenu.EnableIslands)
            map = _mapGenerationMenu.IslandsApplier.ApplyIslands(map);

        if (_mapGenerationMenu.EnableDomainWarping)
            map = _mapGenerationMenu.DomainWarpingApplier.ApplyWarping(map);

        WorldData.TerrainData.SetTerrain(map);
        
        if (_mapGenerationMenu.EnableMoisture)
            _worldData.TerrainData.SetMoistureMap(_mapGenerationMenu.MoistureOptions.MoistureMap);

        _terrainScene2D.RedrawAllImages();
        _terrainScene2D.UpdateAllTextures();
    }
    private void TreePlacementRuleItemOnTreeColorChanged(object sender, TreeLayerColorChangedEventArgs e)
    {
        if (_worldData.TreesData.HasLayerWithId(e.LayerId))
        {
            _worldVisualSettings.TreeSettings.SetTreesLayerColor(e.LayerId, e.NewColor);
            _terrainScene2D.RedrawTreesImage();
            _terrainScene2D.UpdateTreesTexture();
        }
    }

    private void HideMap2D()
    {
        // This is a workaround to hide the 2D scene, because if we use Visible = false, then
        // the right child of HSplitContainer will occupy the entire screen, hiding the 3D scene.
        var c = _terrainScene2D.Modulate;
        c.A = 0f;
        _terrainScene2D.Modulate = c;
        //_terrainScene2D.Visible = false;
    }

    private void ShowMap2D()
    {
        // This is a workaround to show the 2D scene, because if we use Visible = false, then
        // the right child of HSplitContainer will occupy the entire screen, hiding the 3D scene.
        var c = _terrainScene2D.Modulate;
        c.A = 1f;
        _terrainScene2D.Modulate = c;
        //_terrainScene2D.Visible = true;
    }

    private void StartGenerationPipeline()
    {
        _generateMapButton.Disabled = true;
        _stopGenerationButton.Disabled = false;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;
        //_generationTask = Task.Run(async () => GenerationPipelineAsync(cancellationToken));
        _generationTask = Task.Run(() => GenerationPipelineAsync(cancellationToken));
        _generationTask.ContinueWith(async t =>
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            _generateMapButton.Disabled = false;
            _stopGenerationButton.Disabled = true;
        });
    }

    private async Task GenerationPipelineAsync(CancellationToken cancellationToken)
    {
        try
        {
            GD.Print("==============================================================");
            _logger.LogMethodStart();

            var generator = _mapGenerationMenu.SelectedGenerator;
            if (generator == null) return;

            await DisableGenerationOptionsAsync();

            await Clear2DSceneAsync();
            WorldData.TreesData.Clear();

            if (_terrainRegenerationRequired)
            {
                WorldData.TerrainData.Clear();
                await ClearTerrain3D();
                await GenerateTerrainAsync();
                await ApplyMapInterpolation();
                await ApplyInfluenceAsync();
                cancellationToken.ThrowIfCancellationRequested();

                await ApplyTerrainSmoothingAsync(cancellationToken);

                if (_mapGenerationMenu.EnableMoisture)
                    await ApplyMoistureAsync();
                cancellationToken.ThrowIfCancellationRequested();

                if (_mapGenerationMenu.EnableIslands)
                    await ApplyIslandsAsync();
                cancellationToken.ThrowIfCancellationRequested();

                if (_mapGenerationMenu.EnableDomainWarping)
                    await ApplyDomainWarpingAsync();
                cancellationToken.ThrowIfCancellationRequested();
            }
            else
            {
                await ClearTress3D();
                await RedrawTerrainAsync();
            }
            cancellationToken.ThrowIfCancellationRequested();

            if (_mapGenerationMenu.EnableTrees)
                await GenerateTrees(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (_terrainRegenerationRequired)
                await GenerateTerrain3D(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            await GenerateTrees3D(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            _mapGenerationMenu.UpdateCurrentConfigAsLastUsed();
            _terrainRegenerationRequired = false;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
        finally
        {
            await EnableGenerationOptionsAsync();
            await SetGenerationTitleTipAsync(string.Empty);
            _logger.LogMethodEnd();
            GD.Print("==============================================================");
        }
    }

    private async Task ApplyMoistureAsync()
    {
        _logger.Log("Generating moisture...");
        await SetGenerationTitleTipAsync("Generating moisture...");
        var moistureMap = _mapGenerationMenu.MoistureOptions.MoistureMap;
        _worldData.TerrainData.SetMoistureMap(moistureMap);
        await RedrawTerrainAsync();
    }

    private async Task GenerateTerrainAsync()
    {
        _logger.Log("Generating terrain...");
        await SetGenerationTitleTipAsync("Generating map...");
        _curHeightMap = _mapGenerationMenu.SelectedGenerator.GenerateMap();
        await SetAndRedrawTerrainAsync(_curHeightMap);
    }

    private async Task ApplyMapInterpolation()
    {
        _logger.Log("Applying interpolation...");
        await SetGenerationTitleTipAsync("Applying interpolation...");
        _mapGenerationMenu.ApplySelectedInterpolation(_curHeightMap);
        await SetAndRedrawTerrainAsync(_curHeightMap);
    }

    private async Task ApplyInfluenceAsync()
    {
        _logger.Log("Applying height influence");
        await SetGenerationTitleTipAsync("Applying influence...");
        MapHelpers.MultiplyHeight(_curHeightMap, _mapGenerationMenu.CurNoiseInfluence);
        await SetAndRedrawTerrainAsync(_curHeightMap);
    }

    private async Task ApplyTerrainSmoothingAsync(CancellationToken cancellationToken)
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Smoothing...");

        for (int i = 0; i < _mapGenerationMenu.CurSmoothCycles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.Log($"Smoothing - iteration: {i + 1}/{_mapGenerationMenu.CurSmoothCycles}");
            _curHeightMap = MapHelpers.SmoothMap(_curHeightMap);
            await SetAndRedrawTerrainAsync(_curHeightMap);
        }
    }

    private async Task GenerateTrees(CancellationToken cancellationToken)
    {
        _logger.LogMethodStart();
        await SetGenerationTitleTipAsync("Generating trees...");
        _worldData.TreesData.Clear();
        _treePlacementOptions.GenerateTrees(_worldData, cancellationToken);

        var treesColors = _treePlacementOptions.GetTreesColors();
        var treesModels = _treePlacementOptions.GetTreesModels();
        _worldVisualSettings.TreeSettings.ClearTreesLayersColors();
        _worldVisualSettings.TreeSettings.ClearTreesLayersScenes();
        _worldVisualSettings.TreeSettings.SetTreeLayersColors(treesColors);
        _worldVisualSettings.TreeSettings.SetTreeLayersScenes(treesModels);
        await RedrawTreesAsync();
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

    private async Task ClearTress3D()
    {
        _logger.Log("Clearing trees 3D");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene3D.ClearTrees();
    }

    private async Task ClearTerrain3D()
    {
        _logger.Log("Clearing terrain 3D");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene3D.ClearWorld();
    }

    private async Task SetGenerationTitleTipAsync(string tip)
    {
        _logger.Log($"Set generation tip: {tip}");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _generationTitleTip.Text = tip;
    }

    private async Task GenerateTerrain3D(CancellationToken cancellationToken)
    {
        _logger.Log("Generating terrain chunks");
        await SetGenerationTitleTipAsync("Generating terrain chunks...");
        await ClearChunks();
        await UpdateTerrain3DMisc();

        while (_terrainScene3D.IsTerrainChunksGenerating())
        {
            cancellationToken.ThrowIfCancellationRequested();
            _terrainScene3D.GenerateNextChunk();
            await _terrainScene3D.ApplyCurrentChunkAsync();
        }
    }

    private async Task GenerateTrees3D(CancellationToken cancellationToken)
    {
        _logger.Log("Generating tree chunks");
        await SetGenerationTitleTipAsync("Generating tree chunks...");
        
        while (_terrainScene3D.IsTreeChunksGenerating())
        {
            cancellationToken.ThrowIfCancellationRequested();
            _terrainScene3D.GenerateNextTreeChunk();
            await _terrainScene3D.ApplyCurrentTreeChunkAsync();
        }
    }

    private async Task UpdateTerrain3DMisc()
    {
        _logger.Log("Updating terrain 3d misc...");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene3D.UpdateWaterMesh();
        _terrainScene3D.UpdateCameraMovementBounds();
        _terrainScene3D.UpdateLight();
    }

    private async Task ClearChunks()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene3D.ClearWorld();
    }

    private async Task SetAndRedrawTerrainAsync(float[,] terrain)
    {
        _logger.Log("Setting and redrawing terrain layer 2D");
        _worldData.TerrainData.SetTerrain(terrain);
        _terrainScene2D.RedrawTerrainImage();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.UpdateTerrainTexture();
    }

    private async Task RedrawTerrainAsync()
    {
        _logger.Log("Redrawing terrain layer 2D");
        _terrainScene2D.RedrawTerrainImage();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.UpdateTerrainTexture();
    }

    private async Task RedrawTreesAsync()
    {
        _logger.Log("Redrawing trees layer 2D");
        _terrainScene2D.RedrawTreesImage();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _terrainScene2D.UpdateTreesTexture();
    }

    private async Task ApplyIslandsAsync()
    {
        _logger.Log("Making islands...");
        await SetGenerationTitleTipAsync("Making islands...");
        _curHeightMap = _mapGenerationMenu.IslandsApplier.ApplyIslands(_curHeightMap);
        await SetAndRedrawTerrainAsync(_curHeightMap);
    }

    private async Task ApplyDomainWarpingAsync()
    {
        _logger.Log("Applying domain warping...");
        await SetGenerationTitleTipAsync("Applying domain warping...");
        _curHeightMap = _mapGenerationMenu.DomainWarpingApplier.ApplyWarping(_curHeightMap);
        await SetAndRedrawTerrainAsync(_curHeightMap);
    }

    private async Task DisableGenerationOptionsAsync()
    {
        _logger.Log("Disabling options");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _mapGenerationMenu.DisableOptions();
        _fileMenuButton.Disabled = true;
        _saveFileDialog.Hide();
        _loadFileDialog.Hide();
    }

    private async Task EnableGenerationOptionsAsync()
    {
        _logger.Log("Enabling options");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _mapGenerationMenu.EnableOptions();
        _fileMenuButton.Disabled = false;
    }
}
