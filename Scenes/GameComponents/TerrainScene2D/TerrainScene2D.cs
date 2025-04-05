using Godot;
using System;
using System.Runtime.CompilerServices;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Domain.Visualization;

namespace TerrainGenerationApp.Scenes.GameComponents.TerrainScene2D;

public partial class TerrainScene2D : Control
{
    private Label _cellInfoLabel;
    private Label _generationStatusLabel;
    private TextureRect _terrainTextureRect;
    private TextureRect _waterTextureRect;
    private TextureRect _treesTextureRect;

    private readonly Logger<TerrainScene2D> _logger = new();
    private bool _terrainImageTextureResizeRequired = false;
    private bool _treesImageTextureResizeRequired = false;
    private bool _waterTextureResizeRequired = false;
    private Image _terrainImage;
    private Image _waterImage;
    private Image _treesImage;
    private ImageTexture _terrainImageTexture;
    private ImageTexture _waterImageTexture;
    private ImageTexture _treesImageTexture;
    private IWorldDataProvider _worldDataProvider;
    private IWorldVisualizationSettingsProvider _visualizationSettingsProvider;

    public override void _Ready()
    {
        _terrainTextureRect = GetNode<TextureRect>("%TerrainTextureRect");
        _waterTextureRect = GetNode<TextureRect>("%WaterTextureRect");
        _treesTextureRect = GetNode<TextureRect>("%TreesTextureRect");
        _generationStatusLabel = GetNode<Label>("%GenerationStatusLabel");
        _cellInfoLabel = GetNode<Label>("%CellInfoLabel");
        _terrainTextureRect.MouseExited += OnMapTextureRectMouseExited;
        _terrainTextureRect.GuiInput += OnMapTextureRectGuiInput;

        _terrainImage = Image.CreateEmpty(1, 1, false, Image.Format.Rgb8);
        _treesImage = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8); // RGBA8 for transparency
        _terrainImageTexture = ImageTexture.CreateFromImage(_terrainImage);
        _treesImageTexture = ImageTexture.CreateFromImage(_treesImage);
        _terrainTextureRect.Texture = _terrainImageTexture;
        _treesTextureRect.Texture = _treesImageTexture;
    }

    public void SetWorldDataProvider(IWorldDataProvider provider)
    {
        _worldDataProvider = provider;
    }

    public void SetDisplayOptionsProvider(IWorldVisualizationSettingsProvider provider)
    {
        _visualizationSettingsProvider = provider;
    }

    public void SetTitleTip(string tip)
    {
        _generationStatusLabel.Text = tip;
    }

    public void ClearAllImages()
    {
        ClearTerrainImage();
        ClearTreesImage();
    }

    public void RedrawAllImages()
    {
        RedrawTerrainImage();
        RedrawTreesImage();
    }

    public void UpdateAllTextures()
    {
        UpdateTerrainTexture();
        UpdateTreesTexture();
    }


    public void ClearTerrainImage()
    {
        _terrainImage.Fill(Colors.Black);
    }

    public void RedrawTerrainImage()
    {
        ThrowIfNoWorldDataProviderOrDisplayOptionsProvider();

        ResizeTerrainImageIfRequired();

        var worldData = _worldDataProvider.WorldData;
        _visualizationSettingsProvider.Settings.TerrainSettings.RedrawTerrainImage(_terrainImage, worldData);
    }

    public void UpdateTerrainTexture()
    {
        if (_terrainImageTextureResizeRequired)
        {
            _terrainImageTexture.SetImage(_terrainImage);
            _terrainImageTextureResizeRequired = false;
        }
        else
        {
            _terrainImageTexture.Update(_terrainImage);
        }
    }

    private void ResizeTerrainImageIfRequired()
    {
        var terrainMapSize = _worldDataProvider.WorldData.TerrainData.GetMapSize();
        _terrainImageTextureResizeRequired = ResizeImageIfRequired(_terrainImage, terrainMapSize);
    }


    public void ClearTreesImage()
    {
        _treesImage.Fill(Colors.Transparent);
    }

    public void RedrawTreesImage()
    {
        ThrowIfNoWorldDataProviderOrDisplayOptionsProvider();

        ResizeTreesImageIfRequired();

        ClearTreesImage();

        var worldData = _worldDataProvider.WorldData;
        _visualizationSettingsProvider.Settings.TreeSettings.RedrawTreesImage(_treesImage, worldData);
    }

    public void UpdateTreesTexture()
    {
        if (_treesImageTextureResizeRequired)
        {
            _treesImageTexture.SetImage(_treesImage);
            _treesImageTextureResizeRequired = false;
        }
        else
        {
            _treesImageTexture.Update(_treesImage);
        }
    }

    private void ResizeTreesImageIfRequired()
    {
        var treesMapSize = _worldDataProvider.WorldData.TreesData.GetLayersSize();
        _treesImageTextureResizeRequired = ResizeImageIfRequired(_treesImage, treesMapSize);
    }


    private void OnMapTextureRectGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
        {
            Vector2 localPosition = _terrainTextureRect.GetLocalMousePosition();
            var h = _worldDataProvider.WorldData.TerrainData.TerrainMapHeight;
            var w = _worldDataProvider.WorldData.TerrainData.TerrainMapWidth;

            // Convert coordinates to map grid coordinates
            var cellX = (int)(localPosition.X / _terrainTextureRect.Size.X * w);
            var cellY = (int)(localPosition.Y / _terrainTextureRect.Size.Y * h);

            // Check boundaries
            if (cellX >= 0 && cellX < w && cellY >= 0 && cellY < h)
            {
                var height = _worldDataProvider.WorldData.TerrainData.HeightAt(cellY, cellX);
                var slope = _worldDataProvider.WorldData.TerrainData.SlopeAt(cellY, cellX);
                _cellInfoLabel.Text = string.Format("Cell: [ {0} ; {1} ] <---> Value: {2} <---> Slope: {3}", cellX, cellY, height, slope);
            }
        }
    }

    private void OnMapTextureRectMouseExited()
    {
        _cellInfoLabel.Text = "";
    }


    /// <summary>
    /// Resizes the image if its size does not match the specified size.
    /// </summary>
    /// <returns>True if the image was resized</returns>
    private bool ResizeImageIfRequired(Image image, Vector2I size)
    {
        if (image.GetSize() != size)
        {
            image.Resize(size.X, size.Y);
            return true;
        }
        return false;
    }

    private void ThrowIfNoWorldDataProviderOrDisplayOptionsProvider([CallerMemberName] string callerName = "")
    {
        if (_worldDataProvider == null)
        {
            _logger.LogError($"{nameof(_worldDataProvider)} is not set");
            throw new NullReferenceException($"{nameof(_worldDataProvider)} is not set");
        }

        if (_visualizationSettingsProvider == null)
        {
            _logger.LogError($"{nameof(_visualizationSettingsProvider)} is not set");
            throw new NullReferenceException($"{nameof(_visualizationSettingsProvider)} is not set");
        }
    }
}
