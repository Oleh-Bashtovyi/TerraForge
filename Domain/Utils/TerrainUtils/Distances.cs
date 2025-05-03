using Godot;
using System;

namespace TerrainGenerationApp.Domain.Utils.TerrainUtils;

/// <summary>
/// Provides various distance calculation methods.
/// </summary>
public static class Distances
{
    /// <summary>
    /// Calculates the squared Euclidean distance between two points.
    /// </summary>
    public static float EuclideanSquared(float x1, float y1, float x2, float y2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        return dx * dx + dy * dy;
    }

    /// <summary>
    /// Calculates the diagonal distance (Chebyshev distance) between two points.
    /// </summary>
    public static float Diagonal(float x1, float y1, float x2, float y2)
    {
        return Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
    }

    /// <summary>
    /// Calculates the Manhattan distance between two points.
    /// </summary>
    public static float Manhattan(float x1, float y1, float x2, float y2)
    {
        return Math.Abs(x2 - x1) + Math.Abs(y2 - y1);
    }

    /// <summary>
    /// Calculates the Euclidean distance between two points.
    /// </summary>
    public static float Euclidean(Vector2 p1, Vector2 p2)
    {
        return Euclidean(p1.X, p1.Y, p2.X, p2.Y);
    }

    /// <summary>
    /// Calculates the Euclidean distance between two points.
    /// </summary>
    public static float Euclidean(float x1, float y1, float x2, float y2)
    {
        return (float)Math.Sqrt(EuclideanSquared(x1, y1, x2, y2));
    }

    /// <summary>
    /// Calculates the hyperboloid distance between two points.
    /// </summary>
    public static float Hyperboloid(float x1, float y1, float x2, float y2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        return (float)(Math.Sqrt(dx * dx + dy * dy + 1) - 1);
    }

    /// <summary>
    /// Calculates the blob distance between two points.
    /// </summary>
    public static float Blob(float x1, float y1, float x2, float y2)
    {
        float d = Euclidean(x1, y1, x2, y2);
        return (float)Math.Exp(-d * d);
    }

    /// <summary>
    /// Calculates the square bump distance between two points.
    /// </summary>
    public static float SquareBump(float x1, float y1, float x2, float y2)
    {
        float d = Euclidean(x1, y1, x2, y2);
        return d < 1 ? 1 - d * d : 0;
    }
}
