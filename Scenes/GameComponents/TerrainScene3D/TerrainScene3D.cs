using System;
using Godot;
using System.Collections.Generic;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Scenes.GameComponents.TerrainScene3D;

public partial class TerrainScene3D : Node3D
{
	private readonly PackedScene _treeScene = GD.Load<PackedScene>("res://Scenes/Objects/Tree_1.tscn");


    private const int MeshResolution = 10;
    private const float MeshSizeScale = 10f;
	private const float HeightScale = 100f;

	private MeshInstance3D _mapMesh;
	private MeshInstance3D _waterMesh;
    private Node3D _treesContainer;

	private IWorldDataProvider _worldDataProvider;
	private Gradient _terrainGradient;
	private Gradient _waterGradient;

	public override void _Ready()
	{
		_waterMesh = GetNode<MeshInstance3D>("%WaterMesh");
        _treesContainer = GetNode<Node3D>("%TreesContainer");

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

        var map = _worldDataProvider.WorldData.TerrainHeightMap;
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

        var surface = new SurfaceTool();
        var data = new MeshDataTool();
        surface.CreateFrom(planeMesh, 0);

        var arrayPlane = surface.Commit();
        data.CreateFromSurface(arrayPlane, 0);

        for (int i = 0; i < data.GetVertexCount(); i++)
        {
            var vertex = data.GetVertex(i);
            var posX = (vertex.X + halfMeshSizeX) / meshSizeX * h;
            var posZ = (vertex.Z + halfMeshSizeZ) / meshSizeZ * w;
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
        mesh.CreateTrimeshCollision();
        _mapMesh = mesh;
        _waterMesh.Position = new Vector3(0, _worldDataProvider.WorldData.SeaLevel * HeightScale, 0);
        ((PlaneMesh)_waterMesh.Mesh).Size = new Vector2(meshSizeX, meshSizeZ);

        AddChild(_mapMesh);

/*        var random = new Random();
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                if (random.Next(0, 3) == 2)
                {
                    var treeScene = _treeScene.Instantiate() as Node3D;

                    var treePosition = new Vector3(
                        (x / (float)w) * meshSizeX - halfMeshSizeX,
                        map.GetValueAt(y, x) * HeightScale - 3,
                        (y / (float)h) * meshSizeZ - halfMeshSizeZ
                    );
                    treeScene!.Position = treePosition;

                    _treesContainer.AddChild(treeScene);
                }
            }
        }*/


        // Generate trees
        foreach (var item in _worldDataProvider.WorldData.TreeMaps)
        {
            var treeMap = item.Value;
            var treeMapWidth = treeMap.Width();
            var treeMapHeight = treeMap.Height();

            for (var y = 0; y < treeMapHeight; y++)
            {
                for (var x = 0; x < treeMapWidth; x++)
                {
                    if (treeMap[y, x])
                    {
                        var treeScene = _treeScene.Instantiate() as Node3D;

                        var treePosition = new Vector3(
                            (x / (float)w) * meshSizeX - halfMeshSizeX,
                            map.GetValueAt(y, x) * HeightScale,
                            (y / (float)h) * meshSizeZ - halfMeshSizeZ
                        );
                        treeScene!.Position = treePosition;

                        _treesContainer.AddChild(treeScene);
                    }
                }
            }
        }







        /*		if (_mapMesh != null)
                {
                    _mapMesh.QueueFree();
                    _mapMesh = null;
                }
                var trees = _treesContainer.GetChildren();
                foreach (var tree in trees)
                {
                    tree.QueueFree();
                }

                var map = _worldDataProvider.WorldData.TerrainHeightMap;
                var h = map.Height();
                var w = map.Width();
                var planeMesh = new PlaneMesh();
                var material = new StandardMaterial3D();
                var meshSizeX = h * MeshResolution;
                var meshSizeZ = w * MeshResolution;
                planeMesh.Size = new Vector2(meshSizeX, meshSizeZ);
                planeMesh.SubdivideDepth = h * MeshResolution;
                planeMesh.SubdivideWidth = w * MeshResolution;
                planeMesh.Material = material;
                material.VertexColorUseAsAlbedo = true;

                var surface = new SurfaceTool();
                var data = new MeshDataTool();
                surface.CreateFrom(planeMesh, 0);

                var arrayPlane = surface.Commit();
                data.CreateFromSurface(arrayPlane, 0);

                var halfHeight = meshSizeX / 2f;
                var halfWidth = meshSizeZ / 2f;

                for (int i = 0; i < data.GetVertexCount(); i++)
                {
                    var vertex = data.GetVertex(i);
                    var mapX = (vertex.X + halfHeight) / meshSizeX * h;
                    var mapZ = (vertex.Z + halfWidth) / meshSizeZ * w;
                    var height = map.GetValueAt(mapX, mapZ);
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
                mesh.CreateTrimeshCollision();
                _mapMesh = mesh;
                _waterMesh.Position = new Vector3(0, _worldDataProvider.WorldData.SeaLevel * HeightScale, 0);
                ((PlaneMesh)_waterMesh.Mesh).Size = new Vector2(h, w);

                AddChild(_mapMesh);

                var random = new Random();
                for (var y = 0; y < h; y++)
                {
                    for (var x = 0; x < w; x++)
                    {
                        if (random.Next(0, 3) == 2)
                        {
                            var treeScene = _treeScene.Instantiate() as Node3D;

                            var treePosition = new Vector3(
                                (x / (float)w) * meshSizeX - halfWidth,
                                _worldDataProvider.WorldData.GetHeightAt(y, x) * HeightScale,
                                (y / (float)h) * meshSizeZ - halfHeight
                            );
                            treeScene!.Position = treePosition;

                            _treesContainer.AddChild(treeScene);
                        }
                    }
                }*/


        // Generate trees
        //foreach (var item in _worldDataProvider.WorldData.TreeMaps)
        //{
        //    var treeMap = item.Value;
        //    var treeMapWidth = treeMap.Width();
        //    var treeMapHeight = treeMap.Height();

        //    for (var y = 0; y < treeMapHeight; y++)
        //    {
        //        for (var x = 0; x < treeMapWidth; x++)
        //        {
        //            if (treeMap[y, x])
        //            {
        //                var treeScene = _treeScene.Instantiate() as Node3D;

        //                var treePosition = new Vector3(
        //                (x / (float)treeMapWidth) * meshSizeX - halfWidth,
        //                _worldDataProvider.WorldData.GetHeightAt(y, x) * HeightScale,
        //                (y / (float)treeMapHeight) * meshSizeZ - halfHeight
        //                );
        //                treeScene!.Position = treePosition;

        //                _treesContainer.AddChild(treeScene);
        //            }
        //        }
        //    }
        //}
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



	/*    private void _generate_mesh()
		{
			if (_mapMesh != null)
			{
				_mapMesh.QueueFree();
				_mapMesh = null;
			}

			var map = _curDisplayMap;
			var planeMesh = new PlaneMesh();
			planeMesh.Size = new Vector2(map.GetLength(0), map.GetLength(1));
			planeMesh.SubdivideDepth = map.GetLength(0);
			planeMesh.SubdivideWidth = map.GetLength(1);
			planeMesh.Material = new StandardMaterial3D();
			((StandardMaterial3D)planeMesh.Material).VertexColorUseAsAlbedo = true;

			var surface = new SurfaceTool();
			var data = new MeshDataTool();
			surface.CreateFrom(planeMesh, 0);

			var arrayPlane = surface.Commit();
			data.CreateFromSurface(arrayPlane, 0);

			var mapWidth = map.GetLength(1);
			var mapHeight = map.GetLength(0);

			for (int i = 0; i < data.GetVertexCount(); i++)
			{
				Vector3 vertex = data.GetVertex(i);
				int mapX = ((int)vertex.X + (int)(mapHeight / 2)) % mapHeight;
				int mapZ = ((int)vertex.Z + (int)(mapWidth / 2)) % mapWidth;
				vertex.Y = map[mapX, mapZ] * 30;
				data.SetVertexColor(i, ColorMap[mapX, mapZ]);
				data.SetVertex(i, vertex);
			}

			arrayPlane.ClearSurfaces();

			data.CommitToSurface(arrayPlane);
			surface.Begin(Mesh.PrimitiveType.Triangles);
			surface.CreateFrom(arrayPlane, 0);

			var mesh = new MeshInstance3D();
			mesh.Mesh = surface.Commit();
			mesh.CreateTrimeshCollision();
			_mapMesh = mesh;
			_waterMesh.Position = new Vector3(0, CurWaterLevel * 30, 0);
			((PlaneMesh)_waterMesh.Mesh).Size = new Vector2(mapHeight, mapWidth);

			AddChild(_mapMesh);
		}*/
}
