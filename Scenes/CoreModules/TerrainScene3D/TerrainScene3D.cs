using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Domain.Visualization;
using TerrainGenerationApp.Scenes.BuildingBlocks.Pools;
using TerrainGenerationApp.Scenes.CoreModules.Camera;
using TerrainGenerationApp.Scenes.LoadedScenes;

namespace TerrainGenerationApp.Scenes.CoreModules.TerrainScene3D;

public partial class TerrainScene3D : Node3D
{
    private MeshInstance3D _mapMesh;
    private MeshInstance3D _waterMesh;
    private Node3D _treesContainer;
    private Node3D _chunksContainer;
    private MovableCamera _movableCamera;
    private DirectionalLight3D _directionalLight;

    private readonly Logger<TerrainScene3D> _logger = new();
    private readonly NodePool<TerrainChunk> _chunkPool = new(LoadedScenes3D.TERRAIN_CHUNK_SCENE, 40);
    private readonly Dictionary<PackedScene, NodePool<MeshInstance3D>> _treePools = new();
    private readonly StandardMaterial3D _chunkMaterial = new();
    private readonly List<Node3D> _currentTrees = new();
    private IWorldData _worldData;
    private IWorldVisualSettings _visualSettings;
    private TerrainMeshSettings _terrainMeshSettings = new();
    private TerrainChunk? _currentTerrainChunk;
    private float[,]? _heightMap;
    private bool _isTerrainGenerated = false;
    private int _curTerrainGridRow = 0;
    private int _curTerrainGridCol = 0;
    private bool _areTreesGenerated = false;
    private int _curTreeGridRow = 0;
    private int _curTreeGridCol = 0;

    public int TreeChunkSize => 30;
    public int TerrainChunkCellsCoverage => 15;
    public int CameraMovementBoundsThreshold => 20; 
    public float TerrainHeightScale => _terrainMeshSettings.HeightScale;
    public float TerrainGridCellSize => _terrainMeshSettings.GridCellSize;
    public int TerrainGridCellResolution => _terrainMeshSettings.GridCellResolution;
    public float TerrainTotalMeshSizeX => TerrainGridCellSize * (_worldData.TerrainData.TerrainMapWidth - 1);
    public float TerrainTotalMeshSizeZ => TerrainGridCellSize * (_worldData.TerrainData.TerrainMapHeight - 1);

    public override void _Ready()
    {
        _waterMesh = GetNode<MeshInstance3D>("%WaterMesh");
        _treesContainer = GetNode<Node3D>("%TreesContainer");
        _chunksContainer = GetNode<Node3D>("%ChunksContainer");
        _movableCamera = GetNode<MovableCamera>("%MovableCamera");
        _directionalLight = GetNode<DirectionalLight3D>("%DirectionalLight3D");

        _chunkMaterial.VertexColorUseAsAlbedo = true;
        _chunkMaterial.RoughnessTexture = null;
        _chunkMaterial.Roughness = 1.0f;
        _chunkMaterial.Metallic = 0.0f;
        _chunkMaterial.MetallicSpecular = 0.5f;
        _chunkMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel;
    }

    public void BindDisplayOptions(IWorldVisualSettings settings)
    {
        _visualSettings = settings;
    }

    public void ClearWorld()
    {
        _logger.Log("Clearing world...");

        foreach (var child in _chunksContainer.GetChildren())
            _chunksContainer.RemoveChild(child);
        _chunkPool.Reset();

        foreach (var child in _treesContainer.GetChildren())
            _treesContainer.RemoveChild(child);
        
        foreach (var pool in _treePools.Values)
            pool.Reset();
        
        _heightMap = null;
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
/*        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        var meshSizeX = TerrainGridCellSize * (_worldData.TerrainData.TerrainMapWidth - 1);
        var meshSizeZ = TerrainGridCellSize * (_worldData.TerrainData.TerrainMapHeight - 1);
        _waterMesh.Position = new Vector3(
            meshSizeX / 2.0f,
            _worldData.SeaLevel * TerrainHeightScale,
            meshSizeZ / 2.0f);
        ((PlaneMesh)_waterMesh.Mesh).Size = new Vector2(meshSizeX, meshSizeZ);*/


/*        // TODO: MOVE NEXT CODE TO OTHER FUNCTION. CURRENTLY, IT IS A TEMPORARY WORKAROUND
        // Init camera limits
        var minX = -CameraMovementBoundsThreshold;
        var maxX = meshSizeX + CameraMovementBoundsThreshold;
        var minZ = -CameraMovementBoundsThreshold;
        var maxZ = meshSizeZ + CameraMovementBoundsThreshold;
        var minY = -CameraMovementBoundsThreshold;
        var maxY = TerrainHeightScale + Math.Max(meshSizeX, meshSizeZ) + CameraMovementBoundsThreshold;
        _movableCamera.SetMovementLimits(minX, maxX, minY, maxY, minZ, maxZ);*/


        // TODO: MOVE NEXT CODE TO OTHER FUNCTION. CURRENTLY, IT IS A TEMPORARY WORKAROUND
        // Set direction light position
/*        var lightPosX = meshSizeX / 2.0f;
        var lightPosY = TerrainHeightScale + Math.Max(meshSizeX, meshSizeZ) + 20;
        var lightPosZ = meshSizeZ / 2.0f;
        _directionalLight.Position = new Vector3(lightPosX, lightPosY, lightPosZ);*/
    }


    public void UpdateWaterMesh()
    {
        _waterMesh.Position = new Vector3(
           TerrainTotalMeshSizeX / 2.0f,
            _worldData.SeaLevel * TerrainHeightScale,
            TerrainTotalMeshSizeZ / 2.0f);

        if (_waterMesh.Mesh is PlaneMesh planeWaterMesh)
        {
            planeWaterMesh.Size = new Vector2(TerrainTotalMeshSizeX, TerrainTotalMeshSizeZ);
        }
    }

    public void UpdateLight()
    {
        // Set direction light position
        var lightPosX = TerrainTotalMeshSizeX / 2.0f;
        var lightPosY = TerrainHeightScale + Math.Max(TerrainTotalMeshSizeX, TerrainTotalMeshSizeZ) + 20;
        var lightPosZ = TerrainTotalMeshSizeZ / 2.0f;
        _directionalLight.Position = new Vector3(lightPosX, lightPosY, lightPosZ);
    }

    public void UpdateCameraMovementBounds()
    {
        var minX = -CameraMovementBoundsThreshold;
        var maxX = TerrainTotalMeshSizeX + CameraMovementBoundsThreshold;
        var minZ = -CameraMovementBoundsThreshold;
        var maxZ = TerrainTotalMeshSizeZ + CameraMovementBoundsThreshold;
        var minY = -CameraMovementBoundsThreshold;
        var maxY = TerrainHeightScale + Math.Max(TerrainTotalMeshSizeX, TerrainTotalMeshSizeZ) + CameraMovementBoundsThreshold;
        _movableCamera.SetMovementLimits(minX, maxX, minY, maxY, minZ, maxZ);
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
        _curTreeGridCol += TreeChunkSize;
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
                _logger.Log("All trees are generated");
                _logger.Log($"Total pools count: {_treePools.Count}");
                foreach (var pool in _treePools)
                {
                    _logger.Log($"Pool: {pool.Key}, size: {pool.Value.PoolSize}, used nodes: {pool.Value.UsedNodes}");
                }
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

        if (_heightMap == null)
        {
            _heightMap = _worldData.TerrainData.GetHeightMapCopy();
        }

        // Calculate bounds for current chunk
        var rowStart = _curTerrainGridRow;
        var rowEnd = Math.Min(rowStart + TerrainChunkCellsCoverage, _heightMap.Height() - 1);
        var colStart = _curTerrainGridCol;
        var colEnd = Math.Min(colStart + TerrainChunkCellsCoverage, _heightMap.Width() - 1);

        // Check if we've reached the end
        if (rowStart >= _heightMap.Height() - 1 || colStart >= _heightMap.Width() - 1 ||
            rowStart == rowEnd || colStart == colEnd)
        {
            _isTerrainGenerated = true;
            return;
        }

        var scene = _chunkPool.GetNode();
        scene.ChunkMaterial = _chunkMaterial;
        scene.GridCellSize = TerrainGridCellSize;
        scene.GridCellResolution = TerrainGridCellResolution;
        scene.HeightScaleFactor = TerrainHeightScale;
        scene.SetBoundaries(rowStart, rowEnd, colStart, colEnd);
        scene.GenerateChunk(_visualSettings, _worldData, _terrainMeshSettings.MeshMapInterpolation);

        // Set the position of the chunk in world space
        var posGrid = new Vector3(colStart * TerrainGridCellSize, 0, rowStart * TerrainGridCellSize);
        scene.Position = posGrid;
        
        _currentTerrainChunk = scene;
    }


    public void RedrawChunks()
    {
        foreach (var item in _chunksContainer.GetChildren())
        {
            if (item is TerrainChunk chunk)
            {
                chunk.RedrawChunk(_visualSettings, _worldData);
            }
        }
        _currentTerrainChunk?.RedrawChunk(_visualSettings, _worldData);
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

        var meshSizeX = TerrainGridCellSize * (_worldData.TerrainData.TerrainMapWidth - 1);
        var meshSizeZ = TerrainGridCellSize * (_worldData.TerrainData.TerrainMapHeight - 1);

        if (_heightMap == null)
        {
            _heightMap = _worldData.TerrainData.GetHeightMapCopy();
        }

        var random = new RandomNumberGenerator();

        foreach (var item in treeData.GetLayers())
        {
            var treeMap = item.TreesMap;
            var treeMapWidth = treeMap.Width();
            var treeMapHeight = treeMap.Height();
            var packedScene = _visualSettings.TreeSettings.GetTreesLayerScene(item.TreeId);
            var treesCount = 0;

            var pool = _treePools.GetValueOrDefault(packedScene);
            if (pool == null)
            {
                pool = new NodePool<MeshInstance3D>(packedScene, 100);
                _treePools.Add(packedScene, pool);
            }


            for (var y = rowStart; y < rowEnd; y++)
            {
                for (var x = colStart; x < colEnd; x++)
                {
                    if (treeMap[y, x])
                    {
                        var posX = x + (float)GD.RandRange(0.35, 0.65);
                        var posY = y + (float)GD.RandRange(0.35, 0.65);
                        var progX = treeMap.WidthProgress(posX);
                        var progY = treeMap.HeightProgress(posY);
                        var height = _heightMap.GetValueUsingIndexProgress(progY, progX);
                        
                        //var treeScene = packedScene.Instantiate() as Node3D;
                        var treeScene = pool.GetNode();

                        var treePosition = new Vector3(
                            progX * meshSizeX,
                            height * TerrainHeightScale,
                            progY * meshSizeZ
                        );
                        treeScene!.Position = treePosition;

                        var rotation = treeScene!.RotationDegrees;
                        rotation.Y = GD.RandRange(0, 360);
                        treeScene!.RotationDegrees = rotation;

                        _currentTrees.Add(treeScene);
                        treesCount++;
                    }
                }
            }

            _logger.Log($"Tree layer: {item.TreeId}, Trees count: {treesCount} Row range: [{rowStart}; {rowEnd}), Col range: [{colStart}; {colEnd})");
        }
    }

    /// <summary>
    /// Binds the mesh settings to the terrain chunk. Note that direct object changes will not be reflected on the generated chunks.
    /// Generate new chunks from scratch to apply the changes. Use <see cref="GenerateNextChunk"/> and
    /// <see cref="ApplyCurrentChunkAsync"/> to generate and add chunks to scene. 
    /// </summary>
    /// <param name="settings"></param>
    public void BindMeshSettings(TerrainMeshSettings settings)
    {
        _terrainMeshSettings = settings;
    }

    /// <summary>
    /// Binds the world data to the terrain chunk. Note that direct object changes will not be reflected on the generated chunks.
    /// Generate new chunks from scratch to apply the changes. Use <see cref="GenerateNextChunk"/> and
    /// <see cref="ApplyCurrentChunkAsync"/> to generate and add chunks to scene. 
    /// </summary>
    /// <param name="data"></param>
    public void BindWorldData(IWorldData data)
    {
        _worldData = data;
    }
}
