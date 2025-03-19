using Godot;
using System;

namespace TerrainGenerationApp.Scenes;

public partial class Terrain : Node3D
{
    private const int MeshResolution = 2;

    private MeshInstance3D _mapMesh;
    private MeshInstance3D _waterMesh;
    private float[,] _curDisplayMap;
    private float CurWaterLevel;
    private Color[,] ColorMap;
















    // Заміняємо Image на двовимірний масив float
    [Export]
    public float TerrainWidth { get; set; } = 100.0f;

    [Export]
    public float TerrainLength { get; set; } = 100.0f;

    [Export]
    public float HeightScale { get; set; } = 20.0f;

    [Export]
    public int Resolution { get; set; } = 128;

    // Масив висот
    private float[,] heightMap;

    // Посилання на компоненти терену
    private StaticBody3D terrainBody;
    private MeshInstance3D terrainMesh;
    private CollisionShape3D terrainCollision;

    public override void _Ready()
    {
        // Ініціалізуємо heightMap, якщо він не був встановлений ззовні
        if (heightMap == null)
        {
            GD.Print("Initializing default height map...");
            InitializeDefaultHeightMap();
        }

        GenerateTerrain();
    }

    // Ініціалізуємо за замовчуванням плоску мапу
    private void InitializeDefaultHeightMap()
    {
        heightMap = new float[Resolution, Resolution];
        for (int z = 0; z < Resolution; z++)
        {
            for (int x = 0; x < Resolution; x++)
            {
                heightMap[x, z] = 0.0f;
            }
        }
    }

    // Встановлення нової мапи висот ззовні
    public void SetHeightMap(float[,] newHeightMap)
    {
        if (newHeightMap == null)
        {
            GD.PrintErr("Cannot set null height map!");
            return;
        }

        int width = newHeightMap.GetLength(0);
        int height = newHeightMap.GetLength(1);

        // Перевіряємо розміри
        if (width != Resolution || height != Resolution)
        {
            GD.PrintErr($"Height map dimensions ({width}x{height}) don't match resolution ({Resolution}x{Resolution})");
            return;
        }

        heightMap = newHeightMap;
        RedrawTerrain();
    }

    // Метод для оновлення значення висоти в певній точці
    public void UpdateHeight(int x, int z, float height)
    {
        if (x >= 0 && x < Resolution && z >= 0 && z < Resolution)
        {
            heightMap[x, z] = height;
        }
    }

    // Метод для повторної генерації терену після змін
    public void RedrawTerrain()
    {
        // Видаляємо старий терен, якщо він є
        if (terrainBody != null)
        {
            terrainBody.QueueFree();
        }

        // Генеруємо новий терен
        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        // Перевіряємо, чи ініціалізована мапа висот
        if (heightMap == null)
        {
            GD.PrintErr("Height map not initialized!");
            return;
        }

        // Створюємо структуру вузлів для терену
        terrainBody = new StaticBody3D();
        terrainBody.Name = "Terrain";
        AddChild(terrainBody);

        terrainMesh = new MeshInstance3D();
        terrainMesh.Name = "TerrainMesh";
        terrainBody.AddChild(terrainMesh);

        terrainCollision = new CollisionShape3D();
        terrainCollision.Name = "TerrainCollision";
        terrainBody.AddChild(terrainCollision);

        // Створюємо меш
        SurfaceTool surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        // Обчислюємо кроки
        float xStep = TerrainWidth / Resolution;
        float zStep = TerrainLength / Resolution;

        // Генеруємо вершини
        for (int z = 0; z < Resolution - 1; z++)
        {
            for (int x = 0; x < Resolution - 1; x++)
            {
                // Обчислюємо позиції для цього квадрата
                float xPos = x * xStep - TerrainWidth / 2;
                float zPos = z * zStep - TerrainLength / 2;
                float xPosNext = (x + 1) * xStep - TerrainWidth / 2;
                float zPosNext = (z + 1) * zStep - TerrainLength / 2;

                // Отримуємо висоти з мапи висот
                float heightBL = heightMap[x, z] * HeightScale;
                float heightBR = heightMap[x + 1, z] * HeightScale;
                float heightTL = heightMap[x, z + 1] * HeightScale;
                float heightTR = heightMap[x + 1, z + 1] * HeightScale;

                // Обчислюємо вершини
                Vector3 bottomLeft = new Vector3(xPos, heightBL, zPos);
                Vector3 bottomRight = new Vector3(xPosNext, heightBR, zPos);
                Vector3 topLeft = new Vector3(xPos, heightTL, zPosNext);
                Vector3 topRight = new Vector3(xPosNext, heightTR, zPosNext);

                // Отримуємо кольори для кожної вершини
                Color colorBL = getTerrainColor(heightBL, xPos, zPos);
                Color colorBR = getTerrainColor(heightBR, xPosNext, zPos);
                Color colorTL = getTerrainColor(heightTL, xPos, zPosNext);
                Color colorTR = getTerrainColor(heightTR, xPosNext, zPosNext);

                // Обчислюємо нормалі
                Vector3 normal1 = CalculateNormal(bottomLeft, topLeft, bottomRight);
                Vector3 normal2 = CalculateNormal(topLeft, topRight, bottomRight);

                // Створюємо перший трикутник (нижній-лівий, верхній-лівий, нижній-правий)
                surfaceTool.SetNormal(normal1);
                surfaceTool.SetColor(colorBL);
                surfaceTool.SetUV(new Vector2((float)x / Resolution, (float)z / Resolution));
                surfaceTool.AddVertex(bottomLeft);

                surfaceTool.SetNormal(normal1);
                surfaceTool.SetColor(colorTL);
                surfaceTool.SetUV(new Vector2((float)x / Resolution, (float)(z + 1) / Resolution));
                surfaceTool.AddVertex(topLeft);

                surfaceTool.SetNormal(normal1);
                surfaceTool.SetColor(colorBR);
                surfaceTool.SetUV(new Vector2((float)(x + 1) / Resolution, (float)z / Resolution));
                surfaceTool.AddVertex(bottomRight);

                // Створюємо другий трикутник (верхній-лівий, верхній-правий, нижній-правий)
                surfaceTool.SetNormal(normal2);
                surfaceTool.SetColor(colorTL);
                surfaceTool.SetUV(new Vector2((float)x / Resolution, (float)(z + 1) / Resolution));
                surfaceTool.AddVertex(topLeft);

                surfaceTool.SetNormal(normal2);
                surfaceTool.SetColor(colorTR);
                surfaceTool.SetUV(new Vector2((float)(x + 1) / Resolution, (float)(z + 1) / Resolution));
                surfaceTool.AddVertex(topRight);

                surfaceTool.SetNormal(normal2);
                surfaceTool.SetColor(colorBR);
                surfaceTool.SetUV(new Vector2((float)(x + 1) / Resolution, (float)z / Resolution));
                surfaceTool.AddVertex(bottomRight);
            }
        }

        // Створюємо меш і призначаємо його
        surfaceTool.Index();
        ArrayMesh arrayMesh = surfaceTool.Commit();
        terrainMesh.Mesh = arrayMesh;

        // Створюємо форму для колізії
        ConcavePolygonShape3D shape = new ConcavePolygonShape3D();
        shape.Data = arrayMesh.GetFaces();
        terrainCollision.Shape = shape;

        // Опціонально: Створюємо стандартний матеріал
        StandardMaterial3D material = new StandardMaterial3D();
        material.VertexColorUseAsAlbedo = true;  // Використовуємо кольори вершин
        terrainMesh.MaterialOverride = material;
    }

    // Метод для модифікації певної області карти висот (корисно для редагування в реальному часі)
    public void ModifyHeightmapRegion(int startX, int startZ, int width, int height, Func<int, int, float, float> modifierFunction)
    {
        bool modified = false;

        // Обмежуємо регіон межами карти
        startX = Mathf.Clamp(startX, 0, Resolution - 1);
        startZ = Mathf.Clamp(startZ, 0, Resolution - 1);
        width = Mathf.Clamp(width, 0, Resolution - startX);
        height = Mathf.Clamp(height, 0, Resolution - startZ);

        // Модифікуємо висоти в заданому регіоні
        for (int z = startZ; z < startZ + height; z++)
        {
            for (int x = startX; x < startX + width; x++)
            {
                float oldHeight = heightMap[x, z];
                float newHeight = modifierFunction(x, z, oldHeight);

                if (oldHeight != newHeight)
                {
                    heightMap[x, z] = newHeight;
                    modified = true;
                }
            }
        }

        // Оновлюємо терен, якщо були зміни
        if (modified)
        {
            RedrawTerrain();
        }
    }

    // Приклад: Підняти точку та навколишню область
    public void RaiseTerrain(int centerX, int centerZ, float strength, float radius)
    {
        ModifyHeightmapRegion(
            centerX - (int)radius, centerZ - (int)radius,
            (int)(radius * 2) + 1, (int)(radius * 2) + 1,
            (x, z, height) => {
                float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (z - centerZ) * (z - centerZ));
                if (distance <= radius)
                {
                    float factor = 1.0f - (distance / radius);
                    return height + strength * factor;
                }
                return height;
            }
        );
    }

    private Vector3 CalculateNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        // Обчислюємо два ребра та їх векторний добуток для нормалі
        Vector3 edge1 = b - a;
        Vector3 edge2 = c - a;
        return edge1.Cross(edge2).Normalized();
    }

    // Функція для отримання кольору на основі висоти і позиції
    private Color getTerrainColor(float height, float x, float z)
    {
        // Це приклад реалізації, яка інтерполює між різними кольорами на основі висоти
        // Замініть цю функцію на вашу власну логіку вибору кольору

        if (height < 0)
            return new Color(0.0f, 0.2f, 0.5f); // Глибока вода
        else if (height < 2)
            return new Color(0.8f, 0.8f, 0.3f); // Пісок
        else if (height < 8)
            return new Color(0.3f, 0.6f, 0.0f); // Трава
        else if (height < 12)
            return new Color(0.4f, 0.4f, 0.4f); // Камінь
        else
            return new Color(1.0f, 1.0f, 1.0f); // Сніг
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