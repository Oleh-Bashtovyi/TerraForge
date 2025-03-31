using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TerrainGenerationApp.Enums;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Scenes.GameComponents.TerrainScene2D;

public partial class TerrainScene2D : Control
{
    private static readonly Color DefaultTreesColor = Colors.Red;

    private Label _cellInfoLabel;
    private Label _generationStatusLabel;
    private TextureRect _terrainTextureRect;
    private TextureRect _waterTextureRect;
    private TextureRect _treesTextureRect;

    private readonly Logger<TerrainScene2D> _logger = new();
    private Image _terrainImage;
    private Image _waterImage;
    private Image _treesImage;
    private ImageTexture _terrainImageTexture;
    private ImageTexture _waterImageTexture;
    private ImageTexture _treesImageTexture;
    private IWorldDataProvider _worldDataProvider;
    private IDisplayOptionsProvider _displayOptionsProvider;
    private Gradient _terrainGradient;
    private Gradient _waterGradient;

    public override void _Ready()
    {
        _terrainTextureRect = GetNode<TextureRect>("%TerrainTextureRect");
        _waterTextureRect = GetNode<TextureRect>("%WaterTextureRect");
        _treesTextureRect = GetNode<TextureRect>("%TreesTextureRect");
        _generationStatusLabel = GetNode<Label>("%GenerationStatusLabel");
        _cellInfoLabel = GetNode<Label>("%CellInfoLabel");
        _terrainTextureRect.MouseExited += OnMapTextureRectMouseExited;
        _terrainTextureRect.GuiInput += OnMapTextureRectGuiInput;

        _terrainGradient = CreateGradient(ColorPallets.DefaultTerrainColors);
        _waterGradient = CreateGradient(ColorPallets.DefaultWaterColors);

        _terrainImage = Image.CreateEmpty(1, 1, false, Image.Format.Rgb8);
        _treesImage = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8); // RGBA8 for transparency
        _terrainImageTexture = ImageTexture.CreateFromImage(_terrainImage);
        _treesImageTexture = ImageTexture.CreateFromImage(_treesImage);
        _terrainTextureRect.Texture = _terrainImageTexture;
        _treesTextureRect.Texture = _treesImageTexture;
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

    public void SetWorldDataProvider(IWorldDataProvider provider)
    {
        _worldDataProvider = provider;
    }

    public void SetDisplayOptionsProvider(IDisplayOptionsProvider provider)
    {
        _displayOptionsProvider = provider;
    }

    public void SetTip(string tip)
    {
        _generationStatusLabel.Text = tip;
    }

    public void HandleAllImagesClear()
    {
        ClearTerrainImage();
        ClearTreesImage();
    }

    public void HandleAllImagesRedraw()
    {
        HandleTerrainImageRedraw();
        HandleTreesImageRedraw();
    }

    public void HandleAllTexturesUpdate()
    {
        UpdateTerrainTextureWithImage();
        UpdateTreesTextureWithImage();
    }

    public void HandleTerrainImageRedraw()
    {
        ThrowIfNoWorldDataProviderOrDisplayOptionsProvider();

        ResizeTerrainImageIfRequired();

        switch (_displayOptionsProvider.CurDisplayFormat)
        {
            case MapDisplayFormat.Grey:
                RedrawTerrainImageInGrey();
                break;
            case MapDisplayFormat.Colors:
                RedrawTerrainImageInColors();
                break;
            case MapDisplayFormat.GradientColors:
                RedrawTerrainImageWithGradients();
                break;
            default:
                throw new NotSupportedException($"<{nameof(TerrainScene2D)}><{nameof(HandleTerrainImageRedraw)}> - " +
                                                $"Display format: {_displayOptionsProvider.CurDisplayFormat} is NOT supported");
        }
    }

    private void ResizeTerrainImageIfRequired()
    {
        ResizeImageIfRequired(_terrainImage, _worldDataProvider.WorldData.GetMapSize());
    }

    private void ResizeTreesImageIfRequired()
    {
        ResizeImageIfRequired(_treesImage, _worldDataProvider.WorldData.GetMapSize());
    }

    private void ResizeImageIfRequired(Image image, Vector2I size)
    {
        if (image.GetSize() != size)
        {
            image.Resize(size.X, size.Y);
        }
    }

    private void RedrawTerrainImageInGrey()
    {
        var map = _worldDataProvider.WorldData.TerrainHeightMap;
        var h = _worldDataProvider.WorldData.MapHeight;
        var w = _worldDataProvider.WorldData.MapWidth;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var height = map[y, x];
                _terrainImage.SetPixel(x, y, new Color(height, height, height));
            }
        }
    }

    private void RedrawTerrainImageInColors()
    {
        _terrainGradient.InterpolationMode = Gradient.InterpolationModeEnum.Constant;
        _waterGradient.InterpolationMode = Gradient.InterpolationModeEnum.Constant;
        InternalRedrawTerrainImageWithGradients();
    }

    private void RedrawTerrainImageWithGradients()
    {
        _terrainGradient.InterpolationMode = Gradient.InterpolationModeEnum.Linear;
        _waterGradient.InterpolationMode = Gradient.InterpolationModeEnum.Linear;
        InternalRedrawTerrainImageWithGradients();
    }

    private void InternalRedrawTerrainImageWithGradients()
    {
        var slopesThreshold = _displayOptionsProvider.CurSlopeThreshold;
        var slopes = _worldDataProvider.WorldData.TerrainSlopesMap;
        var seaLevel = _worldDataProvider.WorldData.SeaLevel;
        var map = _worldDataProvider.WorldData.TerrainHeightMap;
        var h = _worldDataProvider.WorldData.MapHeight;
        var w = _worldDataProvider.WorldData.MapWidth;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (map[y, x] < seaLevel)
                {
                    _terrainImage.SetPixel(x, y, _waterGradient.Sample(seaLevel - map[y, x]));
                }
                else
                {
                    var baseColor = _terrainGradient.Sample(map[y, x] - seaLevel);
                    var slopeBaseColor = GetSlopeColor(baseColor, slopes[y, x], map[y, x], slopesThreshold);
                    _terrainImage.SetPixel(x, y, slopeBaseColor);
                }
            }
        }
    }

    private void ThrowIfNoWorldDataProviderOrDisplayOptionsProvider([CallerMemberName] string callerName = "")
    {
        if (_worldDataProvider == null)
            throw new NullReferenceException($"<{nameof(TerrainScene2D)}><{callerName}> - " +
                                             $"WorldDataProvider is not set");

        if (_displayOptionsProvider == null)
            throw new NullReferenceException($"<{nameof(TerrainScene2D)}><{callerName}> - " +
                                             $"DisplayOptionsProvider is not set");
    }

    private void ClearImage(Image image, Color clearColor)
    {
        var size = image.GetSize();
        for (int x = 0; x < size.X; x++)
        {
            for (int y = 0; y < size.Y; y++)
            {
                image.SetPixel(x, y, clearColor);
            }
        }
    }

    public void ClearTerrainImage()
    {
        ClearImage(_terrainImage, Colors.Black);
    }

    public void ClearTreesImage()
    {
        ClearImage(_treesImage, Colors.Transparent);
    }

    public void SetTerrainImageToTexture()
    {
        _terrainImageTexture.SetImage(_terrainImage);
    }

    public void UpdateTerrainTextureWithImage()
    {
        _terrainImageTexture.Update(_terrainImage);
    }

    public void SetTreesImageToTexture()
    {
        _treesImageTexture.SetImage(_treesImage);
    }

    public void UpdateTreesTextureWithImage()
    {
        _treesImageTexture.Update(_treesImage);
    }

    public void HandleTreesImageRedraw()
    {
        ThrowIfNoWorldDataProviderOrDisplayOptionsProvider();

        ResizeTreesImageIfRequired();

        switch (_displayOptionsProvider.CurDisplayFormat)
        {
            case MapDisplayFormat.Grey:
                break;
            case MapDisplayFormat.Colors:
            case MapDisplayFormat.GradientColors:
                RedrawTreesImage();
                break;
            default:
                throw new NotSupportedException($"<{nameof(TerrainScene2D)}><{nameof(HandleTreesImageRedraw)}> - " +
                                                $"Display format: {_displayOptionsProvider.CurDisplayFormat} is NOT supported");
        }
    }

    private void RedrawTreesImage()
    {
        var treeMaps = _worldDataProvider.WorldData.TreeMaps;
        var treeColors = _displayOptionsProvider.TreeColors;
        var h = _worldDataProvider.WorldData.MapHeight;
        var w = _worldDataProvider.WorldData.MapWidth;

        // Map should be transparent by default
        ClearTreesImage();

        foreach (var item in treeMaps)
        {
            var id = item.Key;
            var map = item.Value;
            var treesCount = 0;
            var treeColor = treeColors.GetValueOrDefault(id, DefaultTreesColor);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (map[y, x])
                    {
                        treesCount++;
                        _treesImage.SetPixel(x, y, treeColor);
                        if (y > 0) _treesImage.SetPixel(x, y - 1, treeColor);
                        if (x > 0) _treesImage.SetPixel(x - 1, y, treeColor);
                        if (x < w - 1) _treesImage.SetPixel(x + 1, y, treeColor);
                        if (y < h - 1) _treesImage.SetPixel(x, y + 1, treeColor);
                    }
                }
            }

            if (treesCount == 0)
            {
                _logger.Log($"Tree layer: {id} has ZERO trees");
            }
            else
            {
                _logger.Log($"Tree layer: {id}, trees count: {treesCount}, color: {treeColor}");
            }
        }
    }

    private Color GetSlopeColor(Color baseColor, float slope, float elevation, float slopeThreshold)
    {
        // Normalize slope value (to fit within 0-1 range)
        float slopeFactor = MathF.Min(slope * slopeThreshold, 1.0f);

        // Darken based on steepness
        float r = baseColor.R * (1.0f - slopeFactor);
        float g = baseColor.G * (1.0f - slopeFactor);
        float b = baseColor.B * (1.0f - slopeFactor);

        // Brightness correction (slightly brighten at higher elevations)
        float brightnessFactor = 1.0f + (elevation * 0.2f);
        r = Math.Clamp(r * brightnessFactor, 0, 1.0f);
        g = Math.Clamp(g * brightnessFactor, 0, 1.0f);
        b = Math.Clamp(b * brightnessFactor, 0, 1.0f);

        return new Color(r, g, b);
    }

    private void OnMapTextureRectGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
        {
            Vector2 localPosition = _terrainTextureRect.GetLocalMousePosition();
            var h = _worldDataProvider.WorldData.MapHeight;
            var w = _worldDataProvider.WorldData.MapWidth;

            // Convert coordinates to map grid coordinates
            var cellX = (int)(localPosition.X / _terrainTextureRect.Size.X * w);
            var cellY = (int)(localPosition.Y / _terrainTextureRect.Size.Y * h);

            // Check boundaries
            if (cellX >= 0 && cellX < w && cellY >= 0 && cellY < h)
            {
                var height = _worldDataProvider.WorldData.GetHeightAt(cellY, cellX);
                var slope = _worldDataProvider.WorldData.GetSlopeAt(cellY, cellX);
                _cellInfoLabel.Text = string.Format("Cell: [ {0} ; {1} ] <---> Value: {2} <---> Slope: {3}", cellX, cellY, height, slope);
            }
        }
    }

    private void OnMapTextureRectMouseExited()
    {
        _cellInfoLabel.Text = "";
    }
}
