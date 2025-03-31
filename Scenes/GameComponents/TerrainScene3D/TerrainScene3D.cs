using Godot;
using System.Collections.Generic;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Scenes.GameComponents.TerrainScene3D;

public partial class Terrain : Node3D
{
    private const int MeshResolution = 2;
    private const float HeightScale = 30f;

    private MeshInstance3D _mapMesh;
    private MeshInstance3D _waterMesh;

    private IWorldDataProvider _worldDataProvider;
    private Gradient _terrainGradient;
    private Gradient _waterGradient;

    public override void _Ready()
    {
        _waterMesh = GetNode<MeshInstance3D>("WaterMesh");

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

        var map = _worldDataProvider.WorldData.TerrainHeightMap;
        var h = map.Height();
        var w = map.Width();
        var planeMesh = new PlaneMesh();
        var material = new StandardMaterial3D();
        planeMesh.Size = new Vector2(h, w);
        planeMesh.SubdivideDepth = h;
        planeMesh.SubdivideWidth = w;
        planeMesh.Material = material;
        material.VertexColorUseAsAlbedo = true;

        var surface = new SurfaceTool();
        var data = new MeshDataTool();
        surface.CreateFrom(planeMesh, 0);

        var arrayPlane = surface.Commit();
        data.CreateFromSurface(arrayPlane, 0);

        var halfHeight = h / 2f;
        var halfWidth = w / 2f;

        for (int i = 0; i < data.GetVertexCount(); i++)
        {
            var vertex = data.GetVertex(i);
            var mapX = vertex.X + halfHeight;
            var mapZ = vertex.Z + halfWidth;
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