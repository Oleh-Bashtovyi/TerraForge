using Godot;
using System;
using System.Collections.Generic;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Domain.Visualization;

namespace TerrainGenerationApp.Scenes.CoreModules.TerrainScene3D;

public partial class TerrainScene3D : Node3D
{
    private const int MeshResolution = 10;
    private const float MeshSizeScale = 1f;
	private const float HeightScale = 20f;

	private MeshInstance3D _mapMesh;
	private MeshInstance3D _waterMesh;
    private Node3D _treesContainer;

	private IWorldDataProvider _worldDataProvider;
    private IWorldVisualizationSettingsProvider _visualizationSettingsProvider;
    private Gradient _terrainGradient;
	private Gradient _waterGradient;

	public override void _Ready()
	{
		_waterMesh = GetNode<MeshInstance3D>("%WaterMesh");
        _treesContainer = GetNode<Node3D>("%TreesContainer");
        _chunksContainer = GetNode<Node3D>("%ChunksContainer");

        _terrainGradient = CreateGradient(ColorPallets.DefaultTerrainColors);
		_waterGradient = CreateGradient(ColorPallets.DefaultWaterColors);
	}

	private Gradient CreateGradient(IReadOnlyDictionary<float, Color> colorPallet)
	{
		var gradient = new Gradient();
		gradient.RemovePoint(1);
		foreach (var heightColor in colorPallet)
		{
			gradient.AddPoint(heightColor.Key, heightColor.Value);
		}
		return gradient;
	}


    public void SetDisplayOptionsProvider(IWorldVisualizationSettingsProvider provider)
    {
        _visualizationSettingsProvider = provider;
    }



    public int ChunkSize = 25;
    private Node3D _chunksContainer;


    public void GenerateMesh()
    {
        if (_mapMesh != null)
        {
            _mapMesh.QueueFree();
            _mapMesh = null;
        }
        var trees = _treesContainer.GetChildren();
        foreach (var tree in trees)
        {
            tree.QueueFree();
        }

        var map = _worldDataProvider.WorldData.TerrainData.HeightMap;
        var h = map.Height();
        var w = map.Width();
        var planeMesh = new PlaneMesh();
        var material = new StandardMaterial3D();
        var meshSizeX = w * MeshSizeScale;
        var meshSizeZ = h * MeshSizeScale;
        var halfMeshSizeX = meshSizeX / 2f;
        var halfMeshSizeZ = meshSizeZ / 2f;

        planeMesh.Size = new Vector2(meshSizeX, meshSizeZ);
        planeMesh.SubdivideWidth = w * MeshResolution;
        planeMesh.SubdivideDepth = h * MeshResolution;
        planeMesh.Material = material;
        material.VertexColorUseAsAlbedo = true;

        using var surface = new SurfaceTool();
        using var data = new MeshDataTool();
        surface.CreateFrom(planeMesh, 0);

        using var arrayPlane = surface.Commit();
        data.CreateFromSurface(arrayPlane, 0);

        GD.Print(data.GetVertexCount());

        for (int i = 0; i < data.GetVertexCount(); i++)
        {
            var vertex = data.GetVertex(i);
            var posX = (vertex.X + halfMeshSizeX) / meshSizeX * (h - 1);
            var posZ = (vertex.Z + halfMeshSizeZ) / meshSizeZ * (w - 1);
            var height = map.GetValueAt(posZ, posX);
            vertex.Y = height * HeightScale;

            data.SetVertexColor(i, GetTerrainColor(height));
            data.SetVertex(i, vertex);
        }

        arrayPlane.ClearSurfaces();

        data.CommitToSurface(arrayPlane);
        surface.Begin(Mesh.PrimitiveType.Triangles);
        surface.CreateFrom(arrayPlane, 0);

        var mesh = new MeshInstance3D();
        mesh.Mesh = surface.Commit();
        //mesh.CreateTrimeshCollision();
        _mapMesh = mesh;
        _waterMesh.Position = new Vector3(0, _worldDataProvider.WorldData.SeaLevel * HeightScale, 0);
        ((PlaneMesh)_waterMesh.Mesh).Size = new Vector2(meshSizeX, meshSizeZ);

        AddChild(_mapMesh);

        // Generate trees
        foreach (var item in _worldDataProvider.WorldData.TreesData.GetLayers())
        {
            var treeMap = item.TreesMap;
            var treeMapWidth = treeMap.Width();
            var treeMapHeight = treeMap.Height();
            float cellSizeX = meshSizeX / treeMapWidth;
            float cellSizeZ = meshSizeZ / treeMapHeight;
            var random = new Random();
            var packedScene = _visualizationSettingsProvider.Settings.TreeSettings.GetTreesLayerScene(item.TreeId);
            //var packedScene = _treeScene;

            for (var y = 0; y < treeMapHeight; y++)
            {
                for (var x = 0; x < treeMapWidth; x++)
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
                            progX * meshSizeX - halfMeshSizeX,
                            height * HeightScale,
                            progY * meshSizeZ - halfMeshSizeZ
                        );

                        // 0.25 - 0.75
                        //var shiftX = random.NextDouble() * 0.5 + 0.25;
                        //var shiftZ = random.NextDouble() * 0.5 + 0.25;
                        //treePosition.X += (float)(cellSizeX * shiftX);
                        //treePosition.Z += (float)(cellSizeZ * shiftZ);
                        treeScene!.Position = treePosition;

                        _treesContainer.AddChild(treeScene);
                    }
                }
            }
        }
    }


    public void SetWorldDataProvider(IWorldDataProvider provider)
	{
		_worldDataProvider = provider;
	}


	private Color GetTerrainColor(float height)
	{
		var seaLevel = _worldDataProvider.WorldData.SeaLevel;

		if (height < seaLevel)
		{
			return _waterGradient.Sample(seaLevel - height);
		}
		else
		{
			return _terrainGradient.Sample(height - seaLevel);
		}
	}
}
