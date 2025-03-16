using System;

namespace TerrainGenerationApp.Utilities;

public static class Distances
{
    public static float EuclideanSquared(float x1, float y1, float x2, float y2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        return dx * dx + dy * dy;
    }

    public static float Diagonal(float x1, float y1, float x2, float y2)
    {
        return Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
    }

    public static float Manhattan(float x1, float y1, float x2, float y2)
    {
        return Math.Abs(x2 - x1) + Math.Abs(y2 - y1);
    }

    public static float Euclidean(float x1, float y1, float x2, float y2)
    {
        return (float)Math.Sqrt(EuclideanSquared(x1, y1, x2, y2));
    }

    public static float Hyperboloid(float x1, float y1, float x2, float y2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        return (float)(Math.Sqrt(dx * dx + dy * dy + 1) - 1);
    }

    public static float Blob(float x1, float y1, float x2, float y2)
    {
        float d = Euclidean(x1, y1, x2, y2);
        return (float)Math.Exp(-d * d);
    }

    public static float SquareBump(float x1, float y1, float x2, float y2)
    {
        float d = Euclidean(x1, y1, x2, y2);
        return d < 1 ? 1 - d * d : 0;
    }
}