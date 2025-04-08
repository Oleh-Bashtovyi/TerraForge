using System;
using System.Data;
using Godot;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Visualization;

namespace TerrainGenerationApp.Scenes.CoreModules.TerrainScene3D;

public partial class TerrainChunk : MeshInstance3D
{
    private int _gridCellResolution = 1;
    private float _gridCellSize = 1;
    private float _heightScaleFactor = 1;
    private Material _chunkMaterial;

    public int GridCellResolution
    {
        get => _gridCellResolution;
        set
        {
            _gridCellResolution = Math.Max(1, value);
        }
    }
    public float GridCellSize
    {
        get => _gridCellSize;
        set
        {
            _gridCellSize = Math.Max(0.1f, value);
        }
    }
    public float HeightScaleFactor
    {
        get => _heightScaleFactor;
        set
        {
            _heightScaleFactor = Math.Max(0.1f, value);
        }
    }
    public Material ChunkMaterial
    {
        get => _chunkMaterial;
        set
        {
            _chunkMaterial = value;
/*            if (Mesh != null)
            {
                Mesh.SurfaceSetMaterial(0, _material);
            }*/
        }
    }


/*    public void GenerateChunk(float[,] map, float rowStart, float rowEnd, float colStart, float colEnd,
    IWorldVisualizationSettings settings,
    IWorldData worldData)
    {
        // Create a base plane mesh first
        var planeMesh = new PlaneMesh();
        var material = new StandardMaterial3D();

        // Set up the plane
        planeMesh.Size = new Vector2(ChunkSize, ChunkSize);
        planeMesh.SubdivideWidth = Resolution;
        planeMesh.SubdivideDepth = Resolution;

        // Setup material
        material.VertexColorUseAsAlbedo = true;

        // Use MeshDataTool to modify the vertices - similar to your original code
        var surface = new SurfaceTool();
        var data = new MeshDataTool();

        surface.CreateFrom(planeMesh, 0);
        var arrayPlane = surface.Commit();
        data.CreateFromSurface(arrayPlane, 0);

        // Process each vertex
        for (int i = 0; i < data.GetVertexCount(); i++)
        {
            var vertex = data.GetVertex(i);

            // Map vertex position to terrain coordinates
            float normalizedX = (vertex.X / ChunkSize) + 0.5f;
            float normalizedZ = (vertex.Z / ChunkSize) + 0.5f;

            var mapPos = new Vector2(
                Mathf.Lerp(rowStart, rowEnd, normalizedZ),
                Mathf.Lerp(colStart, colEnd, normalizedX));

            // Set height based on map data
            var height = map.GetValueAt(mapPos);
            vertex.Y = height * HeightScaleFactor;

            // Set vertex color from visualization settings
            data.SetVertexColor(i, settings.TerrainSettings.GetColor(mapPos, worldData, false));
            data.SetVertex(i, vertex);
        }

        // Commit changes back to mesh
        arrayPlane.ClearSurfaces();
        data.CommitToSurface(arrayPlane);

        // Finalize
        surface.Begin(Mesh.PrimitiveType.Triangles);
        surface.CreateFrom(arrayPlane, 0);
        surface.SetMaterial(material);
        Mesh = surface.Commit();

        // Clean up
        data.Dispose();
        surface.Dispose();
        arrayPlane.Dispose();
    }*/


    public void GenerateChunk(
        float[,] map, 
        int rowStart, 
        int rowEnd, 
        int colStart, 
        int colEnd,
        IWorldVisualSettings settings,
        IWorldData worldData)
    {
        if (rowStart == rowEnd || colStart == colEnd)
        {
            return;
        }

        ArrayMesh aMesh;
        var totalResolutionHeight = (rowEnd - rowStart) * GridCellResolution; // + 1?
        var totalResolutionWidth = (colEnd - colStart) * GridCellResolution;  // + 1?
        var chunkHeight = (rowEnd - rowStart) * GridCellSize;
        var chunkWidth = (colEnd - colStart) * GridCellSize;

        using var surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        GD.Print($"Total height resolution: {totalResolutionHeight}");
        GD.Print($"Total width resolution: {totalResolutionWidth}");

        for (int z = 0; z < totalResolutionHeight + 1; z++)
        {
            for (int x = 0; x < totalResolutionWidth + 1; x++)
            {
                var percentX = (float)x / totalResolutionWidth;
                var percentZ = (float)z / totalResolutionHeight;
                var pointOnMesh = new Vector3(percentX, 0, percentZ);
                var vertex = new Vector3(pointOnMesh.X * chunkWidth, 0, pointOnMesh.Z * chunkHeight);

                var mapPos = new Vector2(
                    Mathf.Lerp(colStart, colEnd, percentX),
                    Mathf.Lerp(rowStart, rowEnd, percentZ));

                var height = map.GetValueAt(mapPos);
                vertex.Y = height * HeightScaleFactor;

                var color = settings.TerrainSettings.GetColor(mapPos, worldData, false);
                var uv = new Vector2(percentX, percentZ);
                surfaceTool.SetColor(color);
                surfaceTool.SetUV(uv);
                surfaceTool.AddVertex(vertex);
            }
        }

        // created indices for triangle clockwise
        var vert = 0;

        for (int z = 0; z < totalResolutionHeight; z++)
        {
            for (int x = 0; x < totalResolutionWidth; x++)
            {
                surfaceTool.AddIndex(vert + 0);
                surfaceTool.AddIndex(vert + 1);
                surfaceTool.AddIndex(vert + totalResolutionWidth + 1);
                surfaceTool.AddIndex(vert + totalResolutionWidth + 1);
                surfaceTool.AddIndex(vert + 1);
                surfaceTool.AddIndex(vert + totalResolutionWidth + 2);

                vert += 1;
            }
            vert += 1;
        }
        /*        surfaceTool.GenerateNormals();
                //surfaceTool.GenerateTangents();*/

        // Generate smooth normals
        //surfaceTool.GenerateNormals(true); // Set to true for smooth normals
        surfaceTool.GenerateNormals(); // Set to true for smooth normals
        surfaceTool.GenerateTangents();
        surfaceTool.SetMaterial(ChunkMaterial);
        aMesh = surfaceTool.Commit();
        Mesh = aMesh;
    }
}












/*using Godot;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Visualization;

namespace TerrainGenerationApp.Scenes.CoreModules.TerrainScene3D;

public partial class TerrainChunk : MeshInstance3D
{
    private const float CENTER_OFFSET = 0.5f;

    public int Resolution { get; set; } = 30;
    public float HeightScaleFactor { get; set; } = 10;
    public float ChunkSize { get; set; } = 100;


    public void GenerateChunk(float[,] map, float rowStart, float rowEnd, float colStart, float colEnd,
    IWorldVisualizationSettings settings,
    IWorldData worldData)
    {
        // Create a base plane mesh first
        var planeMesh = new PlaneMesh();
        var material = new StandardMaterial3D();

        // Set up the plane
        planeMesh.Size = new Vector2(ChunkSize, ChunkSize);
        planeMesh.SubdivideWidth = Resolution;
        planeMesh.SubdivideDepth = Resolution;

        // Setup material
        material.VertexColorUseAsAlbedo = true;

        // Use MeshDataTool to modify the vertices - similar to your original code
        var surface = new SurfaceTool();
        var data = new MeshDataTool();

        surface.CreateFrom(planeMesh, 0);
        var arrayPlane = surface.Commit();
        data.CreateFromSurface(arrayPlane, 0);

        // Process each vertex
        for (int i = 0; i < data.GetVertexCount(); i++)
        {
            var vertex = data.GetVertex(i);

            // Map vertex position to terrain coordinates
            float normalizedX = (vertex.X / ChunkSize) + 0.5f;
            float normalizedZ = (vertex.Z / ChunkSize) + 0.5f;

            var mapPos = new Vector2(
                Mathf.Lerp(rowStart, rowEnd, normalizedZ),
                Mathf.Lerp(colStart, colEnd, normalizedX));

            // Set height based on map data
            var height = map.GetValueAt(mapPos);
            vertex.Y = height * HeightScaleFactor;

            // Set vertex color from visualization settings
            data.SetVertexColor(i, settings.TerrainSettings.GetColor(mapPos, worldData, false));
            data.SetVertex(i, vertex);
        }

        // Commit changes back to mesh
        arrayPlane.ClearSurfaces();
        data.CommitToSurface(arrayPlane);

        // Finalize
        surface.Begin(Mesh.PrimitiveType.Triangles);
        surface.CreateFrom(arrayPlane, 0);
        surface.SetMaterial(material);
        Mesh = surface.Commit();

        // Clean up
        data.Dispose();
        surface.Dispose();
        arrayPlane.Dispose();
    }


    public void GenerateChunk_OLD(float[,] map, float rowStart, float rowEnd, float colStart, float colEnd, 
        IWorldVisualizationSettings settings,
        IWorldData worldData)
    {
        GD.Print($"Resolution: {Resolution}, HeightFactor: {HeightScaleFactor}, Chunk size: {ChunkSize}");
        ArrayMesh aMesh;
        using var surfaceTool = new SurfaceTool();
        var material = new StandardMaterial3D();
        material.VertexColorUseAsAlbedo = true;
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        for (int z = 0; z < Resolution + 1; z++)
        {
            for (int x = 0; x < Resolution + 1; x++)
            {
                var percent = new Vector2(x, z) / Resolution;
                var pointOnMesh = new Vector3((percent.X - CENTER_OFFSET), 0, (percent.Y - CENTER_OFFSET));
                var vertex = pointOnMesh * ChunkSize;

                var mapPos = new Vector2(
                    Mathf.Lerp(rowStart, rowEnd, percent.Y),
                    Mathf.Lerp(colStart, colEnd, percent.X));
                var height = map.GetValueAt(mapPos);

                vertex.Y = height * HeightScaleFactor;
                var color = settings.TerrainSettings.GetColor(mapPos, worldData, false);
                var uv = new Vector2();
                uv.X = percent.X;
                uv.Y = percent.Y;
                surfaceTool.SetColor(color);
                surfaceTool.SetUV(uv);
                surfaceTool.AddVertex(vertex);
            }
        }

        // created indices for triangle clockwise
        var vert = 0;

        for (int z = 0; z < Resolution; z++)
        {
            for (int x = 0; x < Resolution; x++)
            {
                surfaceTool.AddIndex(vert + 0);
                surfaceTool.AddIndex(vert + 1);
                surfaceTool.AddIndex(vert + Resolution + 1);
                surfaceTool.AddIndex(vert + Resolution + 1);
                surfaceTool.AddIndex(vert + 1);
                surfaceTool.AddIndex(vert + Resolution + 2);

                vert += 1;
            }
            vert += 1;
        }
        //surfaceTool.GenerateNormals();
        //surfaceTool.GenerateTangents();
        material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        surfaceTool.SetMaterial(material);
        aMesh = surfaceTool.Commit();
        Mesh = aMesh;
    }
}*/