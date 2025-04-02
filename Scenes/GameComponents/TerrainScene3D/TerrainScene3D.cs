using System;
using Godot;
using System.Collections.Generic;
using TerrainGenerationApp.Utilities;
using TerrainGenerationApp.Extensions;

namespace TerrainGenerationApp.Scenes.GameComponents.TerrainScene3D;

public partial class TerrainScene3D : Node3D
{
	private readonly PackedScene _treeScene = GD.Load<PackedScene>("res://Scenes/Objects/Tree_1.tscn");


    private const int MeshResolution = 10;
    private const float MeshSizeScale = 1f;
	private const float HeightScale = 20f;

	private MeshInstance3D _mapMesh;
	private MeshInstance3D _waterMesh;
    private Node3D _treesContainer;

	private IWorldDataProvider _worldDataProvider;
    private IDisplayOptionsProvider _displayOptionsProvider;
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


    public void SetDisplayOptionsProvider(IDisplayOptionsProvider provider)
    {
        _displayOptionsProvider = provider;
    }



    public int ChunkSize = 25;
    private Node3D _chunksContainer;

/*    public void GenerateMesh()
    {
        // Clear existing chunks
        foreach (var child in _chunksContainer.GetChildren())
        {
            child.QueueFree();
        }

        // Clear trees
        var trees = _treesContainer.GetChildren();
        foreach (var tree in trees)
        {
            tree.QueueFree();
        }

        var map = _worldDataProvider.WorldData.TerrainHeightMap;
        var h = map.Height();
        var w = map.Width();
        var meshSizeX = w * MeshSizeScale;
        var meshSizeZ = h * MeshSizeScale;

        // Set up water
        _waterMesh.Position = new Vector3(0, _worldDataProvider.WorldData.SeaLevel * HeightScale, 0);
        ((PlaneMesh)_waterMesh.Mesh).Size = new Vector2(meshSizeX, meshSizeZ);

        // Generate terrain in chunks
        for (int chunkZ = 0; chunkZ < h; chunkZ += ChunkSize)
        {
            for (int chunkX = 0; chunkX < w; chunkX += ChunkSize)
            {
                // Calculate actual chunk size (handle edge chunks)
                int actualChunkWidth = Math.Min(ChunkSize, w - chunkX);
                int actualChunkHeight = Math.Min(ChunkSize, h - chunkZ);

                if (actualChunkWidth <= 0 || actualChunkHeight <= 0)
                    continue;

                GenerateChunk(chunkX, chunkZ, actualChunkWidth, actualChunkHeight);
            }
        }
    }

    private void GenerateChunk(int startX, int startZ, int chunkWidth, int chunkHeight)
    {
        var map = _worldDataProvider.WorldData.TerrainHeightMap;

        // Calculate world positions
        float worldStartX = startX * MeshSizeScale - (map.Width() * MeshSizeScale / 2f);
        float worldStartZ = startZ * MeshSizeScale - (map.Height() * MeshSizeScale / 2f);
        float chunkWorldSizeX = chunkWidth * MeshSizeScale;
        float chunkWorldSizeZ = chunkHeight * MeshSizeScale;

        // Create arrays for mesh data (more efficient than using MeshDataTool)
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var colors = new List<Color>();
        var uvs = new List<Vector2>();
        var indices = new List<int>();

        // Generate a grid of vertices for this chunk
        for (int z = 0; z <= chunkHeight; z++)
        {
            for (int x = 0; x <= chunkWidth; x++)
            {
                // Calculate actual map coordinates
                int mapX = startX + x;
                int mapZ = startZ + z;

                if (mapX >= map.Width() || mapZ >= map.Height())
                    continue;

                // Get height and position
                float height = map.GetValueAt(mapZ, mapX);
                float worldX = x * MeshSizeScale;
                float worldZ = z * MeshSizeScale;

                // Add vertex
                vertices.Add(new Vector3(worldX, height * HeightScale, worldZ));

                // Simple normal (will be recalculated later)
                normals.Add(Vector3.Up);

                // Vertex color based on height
                colors.Add(GetTerrainColor(height));

                // UV coordinates
                uvs.Add(new Vector2((float)x / chunkWidth, (float)z / chunkHeight));
            }
        }

        // Generate triangles
        int stride = chunkWidth + 1;
        for (int z = 0; z < chunkHeight; z++)
        {
            for (int x = 0; x < chunkWidth; x++)
            {
                int bottomLeft = z * stride + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = (z + 1) * stride + x;
                int topRight = topLeft + 1;

                // First triangle
                indices.Add(bottomLeft);
                indices.Add(topLeft);
                indices.Add(bottomRight);

                // Second triangle
                indices.Add(bottomRight);
                indices.Add(topLeft);
                indices.Add(topRight);
            }
        }

        // Calculate proper normals
        RecalculateNormals(vertices, indices, normals);

        // Create the mesh
        var mesh = new ArrayMesh();
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        arrays[(int)Mesh.ArrayType.Color] = colors.ToArray();
        arrays[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();

        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        // Create material
        var material = new StandardMaterial3D();
        material.VertexColorUseAsAlbedo = true;
        mesh.SurfaceSetMaterial(0, material);

        // Create mesh instance
        var meshInstance = new MeshInstance3D();
        meshInstance.Mesh = mesh;
        meshInstance.Position = new Vector3(worldStartX, 0, worldStartZ);

        // Add collision if needed
        // Uncomment the following line if you need collision
        // meshInstance.CreateTrimeshCollision();

        _chunksContainer.AddChild(meshInstance);
    }

    // Helper method to recalculate normals
    private void RecalculateNormals(List<Vector3> vertices, List<int> indices, List<Vector3> normals)
    {
        // Reset all normals to zero
        for (int i = 0; i < normals.Count; i++)
        {
            normals[i] = Vector3.Zero;
        }

        // Calculate normals for each triangle
        for (int i = 0; i < indices.Count; i += 3)
        {
            int i1 = indices[i];
            int i2 = indices[i + 1];
            int i3 = indices[i + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            Vector3 normal = (v2 - v1).Cross(v3 - v1).Normalized();

            normals[i1] += normal;
            normals[i2] += normal;
            normals[i3] += normal;
        }

        // Normalize all normals
        for (int i = 0; i < normals.Count; i++)
        {
            normals[i] = normals[i].Normalized();
        }
    }*/









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
        foreach (var item in _worldDataProvider.WorldData.TreeMaps)
        {
            var treeMap = item.Value;
            var treeMapWidth = treeMap.Width();
            var treeMapHeight = treeMap.Height();
            float cellSizeX = meshSizeX / treeMapWidth;
            float cellSizeZ = meshSizeZ / treeMapHeight;
            var random = new Random();
            var packedScene = _displayOptionsProvider.TreeModels[item.Key];
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
