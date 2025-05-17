using Godot;
using System;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Domain.Utils;

namespace TerrainGenerationApp.Domain.Visualization;

public class TerrainVisualSettings
{
    private readonly Logger<TerrainVisualSettings> _logger = new();
    private readonly Gradient _dryTerrainGradient;
    private readonly Gradient _terrainGradient;
    private readonly Gradient _waterGradient;
    private MapDisplayFormat _mapDisplayFormat;
    private float _slopeThreshold = 0.1f;
    private float _moistureInfluence = 0.8f;
    private bool _includeMoisture = false;

    public float SlopeThreshold => _slopeThreshold;
    public float MoistureInfluence => _moistureInfluence;
    public bool IncludeMoisture => _includeMoisture;
    public MapDisplayFormat MapDisplayFormat => _mapDisplayFormat;

    public TerrainVisualSettings()
    {
        _dryTerrainGradient = new Gradient();
        _terrainGradient = new Gradient();
        _waterGradient = new Gradient();
        ClearDryTerrainGradient();
        ClearTerrainGradient();
        ClearWaterGradient();
        SetMapDisplayFormat(MapDisplayFormat.Colors);
    }

    public void SetMapDisplayFormat(MapDisplayFormat mapDisplayFormat)
    {
        _mapDisplayFormat = mapDisplayFormat;

        if (_mapDisplayFormat == MapDisplayFormat.Colors)
        {
            _terrainGradient.InterpolationMode = Gradient.InterpolationModeEnum.Constant;
            _waterGradient.InterpolationMode = Gradient.InterpolationModeEnum.Constant;
        }
        else if (_mapDisplayFormat == MapDisplayFormat.GradientColors)
        {
            _terrainGradient.InterpolationMode = Gradient.InterpolationModeEnum.Linear;
            _waterGradient.InterpolationMode = Gradient.InterpolationModeEnum.Linear;
        }
    }

    public void SetIncludeMoisture(bool includeMoisture)
    {
        _includeMoisture = includeMoisture;
    }

    public void SetSlopeThreshold(float slopeThreshold)
    {
        _slopeThreshold = slopeThreshold;
    }

    public void SetMoistureInfluence(float moistureInfluence)
    {
        _moistureInfluence = moistureInfluence;
    }

    public Color GetColor(Vector2I pos, IWorldData worldData, bool includeSlope = true)
    {
        return GetColor(pos.Y, pos.X, worldData, includeSlope);
    }

    public Color GetColor(Vector2 pos, IWorldData worldData, bool includeSlope = true)
    {
        return GetColor(pos.Y, pos.X, worldData, includeSlope);
    }

    public Color GetColor(float row, float col, IWorldData worldData, bool includeSlope = true)
    {
        var h = worldData.TerrainData.HeightAt(row, col);

        switch (_mapDisplayFormat)
        {
            case MapDisplayFormat.Grey:
                return new Color(h, h, h);
            case MapDisplayFormat.Colors:
            case MapDisplayFormat.GradientColors:
                if (h < worldData.SeaLevel)
                {
                    return _waterGradient.Sample(worldData.SeaLevel - h);
                }

                var baseColor = _terrainGradient.Sample(h - worldData.SeaLevel);

                if (_includeMoisture)
                {
                    var moisture = worldData.TerrainData.MoistureAt(row, col);
                    return ApplyMoisture(baseColor, 1.0f - moisture, h - worldData.SeaLevel);
                }
                return baseColor;

            default:
                throw new NotImplementedException("Can not handle color get operation idk");
        }
    }

    private Color ApplyMoisture(Color baseColor, float moisture, float elevation)
    {
        if (moisture < 0.1f)
            return baseColor;

        /*        float closestKey = 0f;
                float minDiff = float.MaxValue;

                foreach (var key in ColorPallets.MoistTerrainColors.Keys)
                {
                    float diff = Math.Abs(key - elevation);
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        closestKey = key;
                    }
                }

                var moistColor = ColorPallets.MoistTerrainColors[closestKey];*/
        var moistColor = _dryTerrainGradient.Sample(elevation);
        float blendFactor = moisture * _moistureInfluence;

        float r = Mathf.Lerp(baseColor.R, moistColor.R, blendFactor);
        float g = Mathf.Lerp(baseColor.G, moistColor.G, blendFactor);
        float b = Mathf.Lerp(baseColor.B, moistColor.B, blendFactor);

        return new Color(r, g, b);
    }





    public void ClearTerrainGradient()
    {
        var pointCount = _terrainGradient.GetPointCount();
        for (int i = 0; i < pointCount - 1; i++)
        {
            _terrainGradient.RemovePoint(0);
        }
    }

    public void ClearWaterGradient()
    {
        var pointCount = _waterGradient.GetPointCount();
        for (int i = 0; i < pointCount - 1; i++)
        {
            _waterGradient.RemovePoint(0);
        }
    }

    public void ClearDryTerrainGradient()
    {
        var pointCount = _dryTerrainGradient.GetPointCount();
        for (int i = 0; i < pointCount - 1; i++)
        {
            _dryTerrainGradient.RemovePoint(0);
        }
    }

    public void AddTerrainGradientPoint(float offset, Color color)
    {
        _terrainGradient.AddPoint(offset, color);
    }

    public void AddWaterGradientPoint(float offset, Color color)
    {
        _waterGradient.AddPoint(offset, color);
    }

    public void AddDryTerrainGradientPoint(float offset, Color color)
    {
        _dryTerrainGradient.AddPoint(offset, color);
    }


    public void RedrawTerrainImage(Image image, IWorldData worldData)
    {
        var imageSize = image.GetSize();
        var mapSize = worldData.TerrainData.GetMapSize();

        if (mapSize != imageSize)
        {
            _logger.LogError($"Image size {imageSize} does not match map size {mapSize}");
            throw new ArgumentException($"Terrain image size {imageSize} does not match actual map size: {mapSize}");
        }

        var h = worldData.TerrainData.TerrainMapHeight;
        var w = worldData.TerrainData.TerrainMapWidth;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                image.SetPixel(x, y, GetColor(y, x, worldData));
            }
        }
    }
}