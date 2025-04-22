using Godot;
using System;
using System.Runtime.CompilerServices;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Domain.Visualization;

namespace TerrainGenerationApp.Scenes.CoreModules.TerrainScene2D;

public partial class TerrainScene2D : Control
{
    private Label _cellInfoLabel;
    private Label _generationStatusLabel;
    private TextureRect _terrainTextureRect;
    private TextureRect _waterTextureRect;
    private TextureRect _treesTextureRect;
    private ColorRect _mouseCapturer;
    private CheckBox _treesVisibilityCheckBox;

    private readonly Logger<TerrainScene2D> _logger = new();
    private bool _terrainImageTextureResizeRequired;
    private bool _treesImageTextureResizeRequired;
    private bool _waterTextureResizeRequired;
    private Image _terrainImage;
    private Image _waterImage;
    private Image _treesImage;
    private ImageTexture _terrainImageTexture;
    private ImageTexture _waterImageTexture;
    private ImageTexture _treesImageTexture;
    private IWorldVisualSettings _visualSettings;
    private IWorldData _worldData;

    public override void _Ready()
    {
        _terrainTextureRect = GetNode<TextureRect>("%TerrainTextureRect");
        _waterTextureRect = GetNode<TextureRect>("%WaterTextureRect");
        _treesTextureRect = GetNode<TextureRect>("%TreesTextureRect");
        _generationStatusLabel = GetNode<Label>("%GenerationStatusLabel");
        _mouseCapturer = GetNode<ColorRect>("%MouseCapturer");
        _cellInfoLabel = GetNode<Label>("%CellInfoLabel");
        _treesVisibilityCheckBox = GetNode<CheckBox>("%TreesVisibilityCheckBox");
        _treesVisibilityCheckBox.Toggled += TreesVisibilityCheckBoxOnToggled;
        _mouseCapturer.MouseExited += OnMapTextureRectMouseExited;
        _mouseCapturer.GuiInput += OnMapTextureRectGuiInput;

        _terrainImage = Image.CreateEmpty(1, 1, false, Image.Format.Rgb8);
        _treesImage = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8); // RGBA8 for transparency
        _terrainImageTexture = ImageTexture.CreateFromImage(_terrainImage);
        _treesImageTexture = ImageTexture.CreateFromImage(_treesImage);
        _terrainTextureRect.Texture = _terrainImageTexture;
        _treesTextureRect.Texture = _treesImageTexture;
    }

    private void TreesVisibilityCheckBoxOnToggled(bool toggledOn)
    {
        _treesTextureRect.Visible = toggledOn;
    }

    /// <summary>
    /// Binds the world data to the scene. Note that direct changes to the data object will not be automatically reflected in the UI.
    /// To update the images with new data, use <see cref="RedrawTerrainImage"/> or <see cref="RedrawTreesImage"/>.
    /// </summary>
    /// <param name="data">The world data to bind to the scene.</param>
    public void BindWorldData(IWorldData data)
    {
        _worldData = data;
    }

    /// <summary>
    /// Binds the visual settings to the scene. Note that direct changes to the settings object will not be automatically reflected in the UI.
    /// To update the images with new settings, use <see cref="RedrawTerrainImage"/> or <see cref="RedrawTreesImage"/>.
    /// </summary>
    /// <param name="settings">The visual settings to bind to the scene.</param>
    public void BindVisualSettings(IWorldVisualSettings settings)
    {
        _visualSettings = settings;
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
        ThrowIfNoWorldDataOrDisplayOptions();

        ResizeTerrainImageIfRequired();

        _visualSettings.TerrainSettings.RedrawTerrainImage(_terrainImage, _worldData);
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
        var terrainMapSize = _worldData.TerrainData.GetMapSize();
        _terrainImageTextureResizeRequired = ResizeImageIfRequired(_terrainImage, terrainMapSize);
    }


    public void ClearTreesImage()
    {
        _treesImage.Fill(Colors.Transparent);
    }

    public void RedrawTreesImage()
    {
        ThrowIfNoWorldDataOrDisplayOptions();

        if (!_worldData.TreesData.HasLayers())
        {
            return;
        }

        ResizeTreesImageIfRequired();

        ClearTreesImage();

        _visualSettings.TreeSettings.RedrawTreesImage(_treesImage, _worldData);
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
        var treesMapSize = _worldData.TreesData.GetLayersSize();
        _treesImageTextureResizeRequired = ResizeImageIfRequired(_treesImage, treesMapSize);
    }


    private void OnMapTextureRectGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
        {
            //_logger.Log("Mouse moved over texture rect");
            Vector2 localPosition = _terrainTextureRect.GetLocalMousePosition();
            var h = _worldData.TerrainData.TerrainMapHeight;
            var w = _worldData.TerrainData.TerrainMapWidth;

            // Convert coordinates to map grid coordinates
            var cellX = (int)(localPosition.X / _terrainTextureRect.Size.X * w);
            var cellY = (int)(localPosition.Y / _terrainTextureRect.Size.Y * h);

            // Check boundaries
            if (cellX >= 0 && cellX < w && cellY >= 0 && cellY < h)
            {
                var height = _worldData.TerrainData.HeightAt(cellY, cellX);
                var slope = _worldData.TerrainData.SlopeAt(cellY, cellX);
                _cellInfoLabel.Text = $"Cell: [ {cellX} ; {cellY} ] - Value: {height:F5}; Slope: {slope:F5}";
            }
        }
    }

    private void OnMapTextureRectMouseExited()
    {
        //_logger.Log("Exiting texture rect");
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

    private void ThrowIfNoWorldDataOrDisplayOptions([CallerMemberName] string callerName = "")
    {
        if (_worldData == null)
        {
            _logger.LogError($"{nameof(_worldData)} is not set", callerName);
            throw new NullReferenceException($"{nameof(_worldData)} is not set");
        }

        if (_visualSettings == null)
        {
            _logger.LogError($"{nameof(_visualSettings)} is not set", callerName);
            throw new NullReferenceException($"{nameof(_visualSettings)} is not set");
        }
    }
}
