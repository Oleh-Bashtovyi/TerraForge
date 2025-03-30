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

        // GENERATE GRADIENTS FOR 2D MAPS
        _terrainGradient = CreateGradient(ColorPallets.DefaultTerrainColors);
        _waterGradient = CreateGradient(ColorPallets.DefaultWaterColors);

        // INIT 2D MAP TEXTURE
        _terrainImage = Image.CreateEmpty(1, 1, false, Image.Format.Rgb8);
        _treesImage = Image.CreateEmpty(1, 1, false, Image.Format.Rgb8);
		_terrainImageTexture = ImageTexture.CreateFromImage(_terrainImage);
        _treesImageTexture = ImageTexture.CreateFromImage(_treesImage);
        _terrainTextureRect.Texture = _terrainImageTexture;
        _treesTextureRect.Texture = _treesImageTexture;
    }

    private Gradient CreateGradient(IReadOnlyDictionary<float, Color> colorPallet)
    {
        var gradient = new Gradient();
        //gradient.RemovePoint(1);
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
                    var slopeBaseColor =
                        GetSlopeColor(baseColor, slopes[y, x], map[y, x], slopesThreshold);

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

    public void ClearTerrainImage()
    {
        var size = _terrainImage.GetSize();

        for (int x = 0; x < size.X; x++)
        {
            for (int y = 0; y < size.Y; y++)
            {
                _terrainImage.SetPixel(x, y, Colors.Black);
            }
        }
    }
    public void SetTerrainImageToTexture()
    {
		_terrainImageTexture.SetImage(_terrainImage);
    }
    public void UpdateTerrainTextureWithImage()
    {
        _terrainImageTexture.Update(_terrainImage);
    }


    public void ClearTreesImage()
    {
        var size = _treesImage.GetSize();

        for (int x = 0; x < size.X; x++)
        {
            for (int y = 0; y < size.Y; y++)
            {
                _treesImage.SetPixel(x, y, Colors.Transparent);
            }
        }
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
                GD.Print($"<{nameof(TerrainScene2D)}><{nameof(RedrawTreesImage)}> - " +
                         $"Tree layer: {id} has ZERO trees");
            }
            else
            {
                GD.Print($"<{nameof(TerrainScene2D)}><{nameof(RedrawTreesImage)}> - " +
                         $"Tree layer: {id}, trees count: {treesCount}, color: {treeColor}");
            }
        }
    }



	private Color GetSlopeColor(Color baseColor, float slope, float elevation, float slopeThreshold)
	{
		// Нормалізація значення ухилу (щоб вписатись у межі 0-1)
		float slopeFactor = MathF.Min(slope * slopeThreshold, 1.0f);

		// Затемнення залежно від крутизни
		float r = baseColor.R * (1.0f - slopeFactor);
		float g = baseColor.G * (1.0f - slopeFactor);
		float b = baseColor.B * (1.0f - slopeFactor);

		// Корекція яскравості (на великих висотах трохи висвітлюємо)
		float brightnessFactor = 1.0f + (elevation * 0.2f);
		r = Math.Clamp(r * brightnessFactor, 0, 1.0f);
		g = Math.Clamp(g * brightnessFactor, 0, 1.0f);
		b = Math.Clamp(b * brightnessFactor, 0, 1.0f);

		return new Color(r, g, b);
	}


	private void _on_map_texture_rect_gui_input(InputEvent @event)
	{
		/*        if (@event is InputEventMouseMotion)
				{
					Vector2 localPosition = _mapTextureRect.GetLocalMousePosition();
					int h = _curTerrainMap.GetLength(0);
					int w = _curTerrainMap.GetLength(1);

					// Convert coordinates to map grid coordinates
					int cellX = (int)(localPosition.X / _mapTextureRect.Size.X * w);
					int cellY = (int)(localPosition.Y / _mapTextureRect.Size.Y * h);

					// Check boundaries
					if (cellX >= 0 && cellX < w && cellY >= 0 && cellY < h)
					{
						float height = _curTerrainMap[cellY, cellX];  // Get height from the heat map
						float slope = _curSlopesMap[cellY, cellX];
						_cellInfoLabel.Text = string.Format("Cell: [ {0} ; {1} ] <---> Value: {2} <---> Slope: {3}", cellX, cellY, height, slope);
					}
				}*/
	}

	private void OnMapTextureRectMouseExited()
	{
		_cellInfoLabel.Text = "";
	}
}
