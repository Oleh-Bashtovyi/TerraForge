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
        //surfaceTool.GenerateTangents();
        surfaceTool.SetMaterial(ChunkMaterial);
        aMesh = surfaceTool.Commit();
        Mesh = aMesh;
    }
}
