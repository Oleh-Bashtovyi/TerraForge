using Godot;
using System;

namespace TerrainGenerationApp.Domain.Generators;

public class PerlinNoise
{
    public enum SUBTYPE
    {
        PERLIN,
        SIMPLEX
    }

    private const int MAX_MAP_WIDTH = 1024;
    private const int MAX_MAP_HEIGHT = 1024;
    private const float MAX_SCALE = 100.0f;
    private const float MIN_SCALE = 0.0001f;
    private const int MAX_OCTAVES = 10;
    private const float MIN_PERSISTANCE = 0.0f;
    private const float MAX_PERSISTANCE = 1.0f;
    private const float MAX_FREQUENCY = 100.0f;

    // To generate map we need:
    // Map height     #[1; 1024]     # The vertical size of the generated map.
    // Map width      #[1; 1024]     # The horizontal size of the generated map.
    // Seed           #[0; inf]      # The starting point for random number generation to ensure reproducibility.
    // Scale          #[0.0001; 100] # Controls the zoom level of the noise; smaller values zoom in, larger values zoom out.
    // Octaves        #[1; 10]       # Number of layers of noise to combine for added detail.
    // Persistance    #[0; 1]        # Controls the amplitude reduction for each octave; lower values make octaves fade out faster.
    // Frequency      #[0; inf]      # Controls the base frequency of the noise; higher values increase detail but may distort.
    // Lacunarity     #[1; inf]      # Controls the increase in frequency for each octave; higher values make noise more detailed.
    // Offset         #[-inf; inf]   # Shifts the noise pattern by a specific amount in x and y directions.

    // Алгоритм:
    // 1) Дізнатись координати клітини на сітці. Для обмеження вхідних 
    //    координат до 256 елементів було використано оператор &
    // 2) Дізнатись локальні координати всередині знайденої клітки.
    //    Це будуть значення від 0 до 1 для х та у.
    // 3) Провести згладжування локальних координат.
    //    Якщо це не зробити, то шум не буде плавним, або можна сказати натуральним.
    //    Для згладжування використано кубічну функцію кубічної інтерполяції.
    //    Для значень від 0 до 1 вона поверне деякі значення від 0 до 1
    // 4) Знайти значення після виконання інтерполяціями між вершинами квадрата (при 2д)
    //    Або куба (для 3д)

    private int _seed = 0;
    private int _mapWidth = 100;
    private int _mapHeight = 100;
    private float _scale = 25.0f;
    private Vector2 _offset = Vector2.Zero;
    private int _octaves = 4;
    private float _persistance = 0.5f;
    private float _lacunarity = 2.0f;
    private SUBTYPE _curSubtype = SUBTYPE.PERLIN;
    private float _warpingStrength = 1.0f;
    private float _warpingSize = 1.0f;
    private bool _enableWarping = true;

    private RandomNumberGenerator _rng;

    private int[] p = new int[]
    {
            151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225,
            140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148,
            247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32,
            57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
            74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
            60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
            65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169,
            200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
            52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
            207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
            119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
            129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104,
            218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241,
            81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
            184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
            222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
            151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225,
            140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148,
            247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32,
            57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
            74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
            60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
            65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169,
            200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
            52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
            207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
            119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
            129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104,
            218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241,
            81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
            184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
            222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
    };



    // Properties
    public int Seed
    {
        get => _seed;
        set => _seed = value;
    }

    public int MapWidth
    {
        get => _mapWidth;
        set => _mapWidth = value;
    }

    public int MapHeight
    {
        get => _mapHeight;
        set => _mapHeight = value;
    }

    public float Scale
    {
        get => _scale;
        set => _scale = value;
    }

    public Vector2 Offset
    {
        get => _offset;
        set => _offset = value;
    }

    public int Octaves
    {
        get => _octaves;
        set => _octaves = value;
    }

    public float Persistance
    {
        get => _persistance;
        set => _persistance = value;
    }

    public float Lacunarity
    {
        get => _lacunarity;
        set => _lacunarity = value;
    }

    public SUBTYPE CurSubtype
    {
        get => _curSubtype;
        set => _curSubtype = value;
    }

    public float WarpingStrength
    {
        get => _warpingStrength;
        set => _warpingStrength = value;
    }

    public float WarpingSize
    {
        get => _warpingSize;
        set => _warpingSize = value;
    }

    public bool EnableWarping
    {
        get => _enableWarping;
        set => _enableWarping = value;
    }



    public float[,] GenerateMap()
    {
        var map = new float[MapHeight, MapWidth];
        var wstr = _warpingStrength;
        var wsize = _warpingSize;

        for (int y = 0; y < _mapHeight; y++)
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                float xSample = x / _scale + _offset.X;
                float ySample = y / _scale + _offset.Y;
                float height = 0.0f;

                if (_enableWarping)
                {
                    height = Perlin3DOctaves(xSample, ySample, wstr * Perlin2D(xSample * wsize, ySample * wsize));
                }
                else
                {
                    height = Perlin2DOctaves(xSample, ySample);
                }

                map[y, x] = (height + 1.0f) / 2.0f;
            }
        }
        return map;
    }

    public float Perlin1DOctaves(float x)
    {
        float value = 0.0f;
        float amplitude = 1.0f;
        float frequency = 1.0f;
        float maxValue = 0.0f;

        for (int i = 0; i < _octaves; i++)
        {
            value += Perlin1D(x * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= _persistance;
            frequency *= _lacunarity;
        }

        // Нормалізуємо до діапазону [-1; 1]
        return value / maxValue;
    }

    public float Perlin2DOctaves(float x, float y)
    {
        // v - value
        // a - amplitude
        // f - frequency
        // m - max value
        float v = 0.0f;
        float a = 1.0f;
        float f = 1.0f;
        float m = 0.0f;

        for (int i = 0; i < _octaves; i++)
        {
            v += Perlin2D(x * f, y * f) * a;
            m += a;
            a *= _persistance;
            f *= _lacunarity;
        }

        // Нормалізуємо до діапазону [-1; 1]
        return v / m;
    }

    public float Perlin3DOctaves(float x, float y, float z)
    {
        float value = 0.0f;
        float amplitude = 1.0f;
        float frequency = 1.0f;
        float maxValue = 0.0f;

        for (int i = 0; i < _octaves; i++)
        {
            value += Perlin3D(x * frequency, y * frequency, z * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= _persistance;
            frequency *= _lacunarity;
        }

        // Нормалізуємо до діапазону [-1; 1]
        return value / maxValue;
    }

    public float Perlin1D(float x)
    {
        // Визначення координат сітки
        int x0 = (int)Math.Floor(x) & 255;
        int x1 = x0 + 1 & 255;

        // Локальні координати всередині сітки
        float dx = x - (float)Math.Floor(x);

        // Градієнти для обох точок
        float g0 = Grad1D(p[x0], dx);
        float g1 = Grad1D(p[x1], dx - 1);

        // Інтерполяція
        float u = Fade(dx);
        return Lerp(g0, g1, u);
    }



    public float Perlin2D(float x, float y)
    {
        // Визначення координат сітки, координати самого квадрату
        int y0 = (int)Math.Floor(y) & 255;
        int x0 = (int)Math.Floor(x) & 255;
        int x1 = x0 + 1 & 255;
        int y1 = y0 + 1 & 255;

        // Локальні координати всередині сітки, всередині квадрату
        float dx = x - (float)Math.Floor(x);
        float dy = y - (float)Math.Floor(y);

        // Градієнти, або ж значення по кутам всередині квадрату
        float g00 = Grad2D(p[x0 + p[y0]], dx, dy);
        float g01 = Grad2D(p[x0 + p[y1]], dx, dy - 1);
        float g10 = Grad2D(p[x1 + p[y0]], dx - 1, dy);
        float g11 = Grad2D(p[x1 + p[y1]], dx - 1, dy - 1);

        // Інтерполяція
        float u = Fade(dx);
        float v = Fade(dy);
        float nx0 = Lerp(g00, g10, u);
        float nx1 = Lerp(g01, g11, u);

        return Lerp(nx0, nx1, v);
    }

    public float Perlin3D(float x, float y, float z)
    {
        // Визначення координат сітки, координати самого квадрату
        int y0 = (int)Math.Floor(y) & 255;
        int x0 = (int)Math.Floor(x) & 255;
        int z0 = (int)Math.Floor(z) & 255;
        int x1 = x0 + 1 & 255;
        int y1 = y0 + 1 & 255;
        int z1 = z0 + 1 & 255;

        // Локальні координати всередині сітки, всередині квадрату
        float dx = x - (float)Math.Floor(x);
        float dy = y - (float)Math.Floor(y);
        float dz = z - (float)Math.Floor(z);

        // Оотримуємо всі градієнти для кутів куба
        float g000 = Grad3D(p[p[p[x0] + y0] + z0], dx, dy, dz);
        float g001 = Grad3D(p[p[p[x0] + y0] + z1], dx, dy, dz - 1.0f);
        float g010 = Grad3D(p[p[p[x0] + y1] + z0], dx, dy - 1.0f, dz);
        float g011 = Grad3D(p[p[p[x0] + y1] + z1], dx, dy - 1.0f, dz - 1.0f);
        float g100 = Grad3D(p[p[p[x1] + y0] + z0], dx - 1.0f, dy, dz);
        float g101 = Grad3D(p[p[p[x1] + y0] + z1], dx - 1.0f, dy, dz - 1.0f);
        float g110 = Grad3D(p[p[p[x1] + y1] + z0], dx - 1.0f, dy - 1.0f, dz);
        float g111 = Grad3D(p[p[p[x1] + y1] + z1], dx - 1.0f, dy - 1.0f, dz - 1.0f);

        // Згладжування координат
        float u = Fade(dx);
        float v = Fade(dy);
        float w = Fade(dz);

        // Інтерполяція по X (4 інтерполяції)
        float nx00 = Lerp(g000, g100, u);  // нижній передній край
        float nx01 = Lerp(g001, g101, u);  // нижній задній край
        float nx10 = Lerp(g010, g110, u);  // верхній передній край
        float nx11 = Lerp(g011, g111, u);  // верхній задній край

        // Інтерполяція по Y (2 інтерполяції)
        float ny0 = Lerp(nx00, nx10, v);   // передня грань
        float ny1 = Lerp(nx01, nx11, v);   // задня грань

        // Фінальна інтерполяція по Z
        return Lerp(ny0, ny1, w);
    }

    public float Grad1D(int h, float x)
    {
        // Напрямки: вліво (-x) або вправо (+x)
        return (h & 1) == 0 ? x : -x;
    }

    public float Grad2D(int h, float x, float y)
    {
        // Градієнтна функція
        // Source: http://riven8192.blogspot.com/2010/08/calculate-perlinnoise-twice-as-fast.html
        // Тут: x це dx, y це dy, z це dz, h це hash
        //      x, y, z в межах [0.0; 1.0] 
        //      hash в межах [0; 255]
        switch (h & 3)  // Обмежуємо до 4 напрямків (0-3)
        {
            case 0x0: return x + y;
            case 0x1: return -x + y;
            case 0x2: return x - y;
            case 0x3: return -x - y;
            default: return 0.0f;
        }
    }

    public float Grad3D(int h, float x, float y, float z)
    {
        switch (h & 0xF)
        {
            case 0x0: return x + y;
            case 0x1: return -x + y;
            case 0x2: return x - y;
            case 0x3: return -x - y;
            case 0x4: return x + z;
            case 0x5: return -x + z;
            case 0x6: return x - z;
            case 0x7: return -x - z;
            case 0x8: return y + z;
            case 0x9: return -y + z;
            case 0xA: return y - z;
            case 0xB: return -y - z;
            case 0xC: return y + x;
            case 0xD: return -y + z;
            case 0xE: return y - x;
            case 0xF: return -y - z;
            default: return 0.0f;
        }
    }

    public float Fade(float t)
    {
        // Функція згладжування. В даному разі кубічна,
        // щоб перехід між значеннями виглядав більш натурально
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    public float Lerp(float a, float b, float t)
    {
        // Лінійна інтерполяція t -> (0; 1.0)
        return a + t * (b - a);
    }



}
