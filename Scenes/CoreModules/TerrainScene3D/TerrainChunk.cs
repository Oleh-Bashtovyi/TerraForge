using Godot;
using System;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Visualization;

namespace TerrainGenerationApp.Scenes.CoreModules.TerrainScene3D;

public partial class TerrainChunk : MeshInstance3D
{
    private int _gridCellResolution = 1;
    private float _gridCellSize = 1;
    private float _heightScaleFactor = 1;
    private int _rowStart;
    private int _rowEnd;
    private int _colStart;
    private int _colEnd;
    private Material _chunkMaterial;
    private ArrayMesh generatedMesh;

    public int GridCellResolution
    {
        get => _gridCellResolution;
        set => _gridCellResolution = Math.Max(1, value);
    }
    public float GridCellSize
    {
        get => _gridCellSize;
        set => _gridCellSize = Math.Max(0.1f, value);
    }
    public float HeightScaleFactor
    {
        get => _heightScaleFactor;
        set => _heightScaleFactor = Math.Max(0.1f, value);
    }
    public Material ChunkMaterial
    {
        get => _chunkMaterial;
        set
        {
            _chunkMaterial = value;
            if (Mesh != null)
            {
                Mesh.SurfaceSetMaterial(0, _chunkMaterial);
            }
        }
    }

    public int TotalResolutionWidth => (_colEnd - _colStart) * GridCellResolution + 1;
    public int TotalResolutionHeight => (_rowEnd - _rowStart) * GridCellResolution + 1;
    public float ChunkWidth => (_colEnd - _colStart) * GridCellSize;
    public float ChunkHeight => (_rowEnd - _rowStart) * GridCellSize;

    public void GenerateChunk(IWorldVisualSettings settings, IWorldData worldData, MapExtensions.InterpolationType interpolation)
    {
        if (_rowStart >= _rowEnd || _colStart >= _colEnd)
        {
            return;
        }

        generatedMesh?.Dispose();

        ArrayMesh aMesh;
        var totalResolutionHeight = TotalResolutionHeight - 1; 
        var totalResolutionWidth = TotalResolutionWidth - 1;   
        var chunkHeight = ChunkHeight;
        var chunkWidth = ChunkWidth;

        using var surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        for (int z = 0; z < TotalResolutionHeight; z++)
        {
            for (int x = 0; x < TotalResolutionWidth; x++)
            {
                var percentX = totalResolutionWidth == 0 ? 0 : (float)x / totalResolutionWidth;
                var percentZ = totalResolutionHeight == 0 ? 0 : (float)z / totalResolutionHeight;
                var pointOnMesh = new Vector3(percentX, 0, percentZ);
                var vertex = new Vector3(pointOnMesh.X * chunkWidth, 0, pointOnMesh.Z * chunkHeight);

                var mapPos = new Vector2(
                    Mathf.Lerp(_colStart, _colEnd, percentX),
                    Mathf.Lerp(_rowStart, _rowEnd, percentZ));

                var height = worldData.TerrainData.HeightAt(mapPos, interpolation);
                vertex.Y = height * HeightScaleFactor;

                var color = settings.TerrainSettings.GetColor(mapPos, worldData, false);
                var uv = new Vector2(percentX, percentZ);
                surfaceTool.SetColor(color);
                surfaceTool.SetUV(uv);
                surfaceTool.AddVertex(vertex);
            }
        }

        // create indices for triangles in a clockwise order
        var vert = 0;

        for (int z = 0; z < totalResolutionHeight; z++)
        {
            for (int x = 0; x < totalResolutionWidth; x++)
            {
                surfaceTool.AddIndex(vert + 0);
                surfaceTool.AddIndex(vert + 1);
                surfaceTool.AddIndex(vert + TotalResolutionWidth);
                surfaceTool.AddIndex(vert + TotalResolutionWidth);
                surfaceTool.AddIndex(vert + 1);
                surfaceTool.AddIndex(vert + TotalResolutionWidth + 1);

                vert += 1;
            }
            vert += 1;
        }

        surfaceTool.GenerateNormals();
        surfaceTool.SetMaterial(ChunkMaterial);
        aMesh = surfaceTool.Commit();
        generatedMesh = aMesh;
        Mesh = aMesh;
    }

    public void SetBoundaries(int rowStart, int rowEnd, int colStart, int colEnd)
    {
        _rowStart = rowStart;
        _rowEnd = rowEnd;
        _colStart = colStart;
        _colEnd = colEnd;
    }

    public void RedrawChunk(IWorldVisualSettings settings, IWorldData worldData)
    {
        var arrays = generatedMesh.SurfaceGetArrays(0);
        var colorsArray = arrays[(int)Mesh.ArrayType.Color].AsColorArray();
        var verticesArray = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();

        for (int i = 0; i < verticesArray.Length; i++)
        {
            var vertex = verticesArray[i];
            var percentX = vertex.X / ChunkWidth;
            var percentZ = vertex.Z / ChunkHeight;

            var mapPos = new Vector2(
                Mathf.Lerp(_colStart, _colEnd, percentX),
                Mathf.Lerp(_rowStart, _rowEnd, percentZ));

            var color = settings.TerrainSettings.GetColor(mapPos, worldData, includeSlope: false);
            colorsArray[i] = color;
        }

        arrays[(int)Mesh.ArrayType.Color] = colorsArray;
        generatedMesh = new ArrayMesh();
        generatedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        generatedMesh.SurfaceSetMaterial(0, ChunkMaterial);
        Mesh = generatedMesh;
    }
}
