using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Domain.Visualization;

namespace TerrainGenerationApp.Scenes.CoreModules.TerrainScene3D;

public partial class TerrainScene3D : Node3D
{
    private MeshInstance3D _mapMesh;
    private MeshInstance3D _waterMesh;
    private Node3D _treesContainer;
    private Node3D _chunksContainer;
     
    private readonly Logger<TerrainScene3D> _logger = new();
    private TerrainChunk? _currentTerrainChunk;
    private bool _isTerrainGenerated = false;
    private int _curTerrainGridRow = 0;
    private int _curTerrainGridCol = 0;
    private List<Node3D> _currentTrees = new();
    private bool _areTreesGenerated = false;
    private int _curTreeGridRow = 0;
    private int _curTreeGridCol = 0;

    public int TreeChunkSize = 30;

    public int TerrainChunkCellsCoverage = 15;
    public float TerrainHeightScale = 20f;
    public float TerrainGridCellSize = 0.8f;
    public int TerrainGridCellResolution = 2;

    public StandardMaterial3D ChunkMaterial = new StandardMaterial3D();

    private PackedScene TerrainChunkScene =
        ResourceLoader.Load<PackedScene>("res://Scenes/CoreModules/TerrainScene3D/TerrainChunk.tscn");


    private IWorldData _worldData;
    private IWorldVisualSettings _visualSettings;

    public override void _Ready()
    {
        _waterMesh = GetNode<MeshInstance3D>("%WaterMesh");
        _treesContainer = GetNode<Node3D>("%TreesContainer");
        _chunksContainer = GetNode<Node3D>("%ChunksContainer");

        ChunkMaterial.VertexColorUseAsAlbedo = true;
        ChunkMaterial.RoughnessTexture = null;
        ChunkMaterial.Roughness = 1.0f;
        ChunkMaterial.Metallic = 0.0f;
        ChunkMaterial.MetallicSpecular = 0.5f;
        ChunkMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel;
    }

    public void SetDisplayOptionsProvider(IWorldVisualSettings settings)
    {
        _visualSettings = settings;
    }

    public void ClearChunks()
    {
        _logger.Log("Clearing world...");

        foreach (var child in _chunksContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var child in _treesContainer.GetChildren())
        {
            child.QueueFree();
        }

        _curTerrainGridRow = 0;
        _curTerrainGridCol = 0;
        _curTreeGridRow = 0;
        _curTreeGridCol = 0;
        _isTerrainGenerated = false;
        _areTreesGenerated = false;
        _currentTerrainChunk = null;
        _currentTrees.Clear();
    }

    public bool IsTerrainChunksGenerating()
    {
        return !_isTerrainGenerated;
    }

    public bool IsTreeChunksGenerating()
    {
        return !_areTreesGenerated;
    }

    public async Task InitWater()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        var meshSizeX = TerrainGridCellSize * _worldData.TerrainData.TerrainMapWidth;
        var meshSizeZ = TerrainGridCellSize * _worldData.TerrainData.TerrainMapHeight;
        _waterMesh.Position = new Vector3(
            meshSizeX / 2.0f,
            _worldData.SeaLevel * TerrainHeightScale,
            meshSizeZ / 2.0f);
        ((PlaneMesh)_waterMesh.Mesh).Size = new Vector2(meshSizeX, meshSizeZ);
    }


    public async Task ApplyCurrentTreeChunkAsync()
    {
        _logger.Log($"Applying trees count: {_currentTrees.Count}");

        if (_currentTrees.Count > 0)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            foreach (var tree in _currentTrees)
            {
                _treesContainer.AddChild(tree);
            }

            _currentTrees.Clear();
        }

        // Update for next chunk
        _curTreeGridCol += TerrainChunkCellsCoverage;
        var treesData = _worldData.TreesData;
        // If we reached the end of the row, move to the next row
        if (_curTreeGridCol >= treesData.LayersWidth - 1)
        {
            _curTreeGridCol = 0;
            _curTreeGridRow += TreeChunkSize;

            // If we reached the end of all rows, mark as completed
            if (_curTreeGridRow >= treesData.LayersHeight - 1)
            {
                _areTreesGenerated = true;
            }
        }
    }

    public async Task ApplyCurrentChunkAsync()
    {
        if (_currentTerrainChunk == null || _isTerrainGenerated)
        {
            return;
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _chunksContainer.AddChild(_currentTerrainChunk);
        _currentTerrainChunk = null;

        // Update for next chunk
        _curTerrainGridCol += TerrainChunkCellsCoverage;

        // If we reached the end of the row, move to the next row
        if (_curTerrainGridCol >= _worldData.TerrainData.TerrainMapWidth - 1)
        {
            _curTerrainGridCol = 0;
            _curTerrainGridRow += TerrainChunkCellsCoverage;

            // If we reached the end of all rows, mark as completed
            if (_curTerrainGridRow >= _worldData.TerrainData.TerrainMapHeight - 1)
            {
                _isTerrainGenerated = true;
            }
        }
    }

    public void GenerateNextChunk()
    {
        if (_isTerrainGenerated)
        {
            return;
        }

        if (_currentTerrainChunk != null)
        {
            _logger.LogError("Previous chunk is not applied");
            throw new InvalidOperationException("Previous chunk is not applied");
        }

        var map = _worldData.TerrainData.HeightMap;

        // Calculate bounds for current chunk
        var rowStart = _curTerrainGridRow;
        var rowEnd = Math.Min(rowStart + TerrainChunkCellsCoverage, map.Height() - 1);
        var colStart = _curTerrainGridCol;
        var colEnd = Math.Min(colStart + TerrainChunkCellsCoverage, map.Width() - 1);

        // Check if we've reached the end
        if (rowStart >= map.Height() - 1 || colStart >= map.Width() - 1 ||
            rowStart == rowEnd || colStart == colEnd)
        {
            _isTerrainGenerated = true;
            return;
        }

        // Create and setup the new chunk
        var scene = TerrainChunkScene.Instantiate<TerrainChunk>();
        scene.ChunkMaterial = ChunkMaterial;
        scene.GridCellSize = TerrainGridCellSize;
        scene.GridCellResolution = TerrainGridCellResolution;
        scene.HeightScaleFactor = TerrainHeightScale;

        scene.GenerateChunk(map,
            rowStart,
            rowEnd,
            colStart,
            colEnd,
            _visualSettings,
            _worldData);

        // Set the position of the chunk in world space
        var posGrid = new Vector3(colStart * TerrainGridCellSize, 0, rowStart * TerrainGridCellSize);

        scene.Position = posGrid;

        _currentTerrainChunk = scene;
    }





    public void GenerateNextTreeChunk()
    {
        if (_areTreesGenerated)
        {
            return;
        }

        if (_currentTrees.Count > 0)
        {
            throw new InvalidOperationException("Previous tree chunk is not applied");
        }

        var treeData = _worldData.TreesData;

        // Calculate bounds for current chunk
        var rowStart = _curTreeGridRow;
        var rowEnd = Math.Min(rowStart + TreeChunkSize, treeData.LayersHeight - 1);
        var colStart = _curTreeGridCol;
        var colEnd = Math.Min(colStart + TreeChunkSize, treeData.LayersWidth - 1);

        // Check if we've reached the end
        if (rowStart >= treeData.LayersHeight - 1 || colStart >= treeData.LayersWidth - 1 ||
            rowStart == rowEnd || colStart == colEnd)
        {
            _areTreesGenerated = true;
            return;
        }

        var meshSizeX = TerrainGridCellSize * _worldData.TerrainData.TerrainMapWidth;
        var meshSizeZ = TerrainGridCellSize * _worldData.TerrainData.TerrainMapHeight;
        var map = _worldData.TerrainData.HeightMap;

        foreach (var item in treeData.GetLayers())
        {
            var treeMap = item.TreesMap;
            var treeMapWidth = treeMap.Width();
            var treeMapHeight = treeMap.Height();
            var packedScene = _visualSettings.TreeSettings.GetTreesLayerScene(item.TreeId);

            for (var y = rowStart; y < rowEnd; y++)
            {
                for (var x = colStart; x < colEnd; x++)
                {
                    if (treeMap[y, x])
                    {
                        var posX = x + 0.5f;
                        var posY = y + 0.5f;
                        var progX = treeMap.WidthProgress(posX);
                        var progY = treeMap.HeightProgress(posY);
                        var height = map.GetValueUsingIndexProgress(progY, progX);
                        var treeScene = packedScene.Instantiate() as Node3D;


                        var treePosition = new Vector3(
                            progX * meshSizeX,
                            height * TerrainHeightScale,
                            progY * meshSizeZ
                        );

                        // 0.25 - 0.75
                        //var shiftX = random.NextDouble() * 0.5 + 0.25;
                        //var shiftZ = random.NextDouble() * 0.5 + 0.25;
                        //treePosition.X += (float)(cellSizeX * shiftX);
                        //treePosition.Z += (float)(cellSizeZ * shiftZ);
                        treeScene!.Position = treePosition;
                        _currentTrees.Add(treeScene);
                        //_treesContainer.AddChild(treeScene);
                    }
                }
            }
        }
    }

    public void SetWorldData(IWorldData data)
    {
        _worldData = data;
    }
}
