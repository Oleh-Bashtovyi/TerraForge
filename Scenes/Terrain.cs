using Godot;

namespace TerrainGenerationApp.Scenes;

public partial class Terrain : Node3D
{
    private const int MeshResolution = 2;

    private MeshInstance3D _mapMesh;
    private MeshInstance3D _waterMesh;
    private float[,] _curDisplayMap;
    private float CurWaterLevel;
    private Color[,] ColorMap;














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
            data.SetVertexColor(i, get_terrain_color(map[mapX, mapZ], _curSlopesMap[mapX, mapZ]));
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









    private void _generate_mesh()
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
    }
}