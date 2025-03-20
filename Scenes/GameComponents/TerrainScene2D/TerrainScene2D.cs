using Godot;
using System;
using System.Collections.Generic;
using TerrainGenerationApp.Enums;
using TerrainGenerationApp.Scenes.GameComponents.DisplayOptions;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Scenes.GameComponents.TerrainScene2D;

public partial class TerrainScene2D : Control
{
	private MapDisplayOptions _displayOptions;
	private ImageTexture _mapTexture;
	private Image _texture;
	private Gradient _terrainGradient;
	private Gradient _waterGradient;

	private Label _cellInfoLabel;
	private TextureRect _mapTextureRect;

    private Dictionary<string, Color> _treeColors = new();


	public override void _Ready()
	{
		_mapTextureRect = GetNode<TextureRect>("%MapTextureRect");
		_cellInfoLabel = GetNode<Label>("%CellInfoLabel");

		// GENERATE GRADIENTS FOR 2D MAPS
		_terrainGradient = new Gradient();
		_waterGradient = new Gradient();
		_terrainGradient.RemovePoint(1);
		_waterGradient.RemovePoint(1);
		foreach (var heightColor in ColorPallets.DefaultTerrainColors)
		{
			var gradOffset = heightColor.Key;
			_terrainGradient.AddPoint(gradOffset, heightColor.Value);
		}

		foreach (var heightColor in ColorPallets.DefaultWaterColors)
		{
			var gradOffset = heightColor.Key;
			_waterGradient.AddPoint(gradOffset, heightColor.Value);
		}

		// INIT 2D MAP TEXTURE
		_texture = Image.CreateEmpty(1, 1, false, Image.Format.Rgb8);
		_mapTexture = ImageTexture.CreateFromImage(_texture);
		_mapTextureRect.Texture = _mapTexture;
	}



    public void SetTreeColors(Dictionary<string, Color> dict)
    {
		_treeColors = dict;
    }

	public void ResizeMap(IWorldData worldData)
	{
		var h = worldData.MapHeight;
		var w = worldData.MapWidth;
		if (_texture.GetSize() != new Vector2I(h, w))
		{
			_texture.Resize(w, h);
			_mapTexture.SetImage(_texture);
		}
	}


	public void RedrawTerrain(IWorldData worldData)
	{
		var displayFormat = _displayOptions.CurDisplayFormat;
		var slopesThreshold = _displayOptions.CurSlopeThreshold;
		var waterLevel = worldData.SeaLevel;
		var slopes = worldData.SlopesMap;
		var map = worldData.TerrainMap;
		var h = map.GetLength(0);
		var w = map.GetLength(1);

		if (displayFormat == MapDisplayFormat.Grey)
		{
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					var v = map[y, x];
					_texture.SetPixel(x, y, new Color(v, v, v, 1.0f));
				}
			}
		}
		else
		{
			if (displayFormat == MapDisplayFormat.Colors)
			{
				_terrainGradient.InterpolationMode = Gradient.InterpolationModeEnum.Constant;
				_waterGradient.InterpolationMode = Gradient.InterpolationModeEnum.Constant;
			}
			else
			{
				_terrainGradient.InterpolationMode = Gradient.InterpolationModeEnum.Linear;
				_waterGradient.InterpolationMode = Gradient.InterpolationModeEnum.Linear;
			}

			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					if (map[y, x] < waterLevel)
					{
						_texture.SetPixel(x, y, _waterGradient.Sample(waterLevel - map[y, x]));
					}
					else
					{
						var baseColor = _terrainGradient.Sample(map[y, x] - waterLevel);
						var slopeBaseColor =
							GetSlopeColor(baseColor, slopes[y, x], map[y, x], slopesThreshold);

						_texture.SetPixel(x, y, slopeBaseColor);
					}
				}
			}
		}
		_mapTexture.Update(_texture);
	}

	public void RedrawTreeLayers(IWorldData worldData)
    {
		GD.Print($"<{nameof(TerrainScene2D)}><{nameof(RedrawTreeLayers)}>--->STARTED....");

        var h = worldData.MapHeight;
        var w = worldData.MapWidth;

        foreach (var item in worldData.TreeMaps)
        {
            var id = item.Key;
			var map = item.Value;
            var treesCount = 0;
            var treeColor = _treeColors[id];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (map[y, x])
                    {
                        
                        treesCount++;

                        _texture.SetPixel(x, y, treeColor);
                        if (y > 0) _texture.SetPixel(x, y - 1, treeColor);
                        if (x > 0) _texture.SetPixel(x - 1, y, treeColor);
                        if (x < w - 1) _texture.SetPixel(x + 1, y, treeColor);
                        if (y < h - 1) _texture.SetPixel(x, y + 1, treeColor);
                    }
                }
            }

            if (treesCount == 0)
            {
				GD.Print("SOME TREE MAP HAS ZERO TREES - " + id);
            }
            else
            {
                GD.Print("Map id - " + id + " trees count - " + treesCount + " Color - " + treeColor);
            }
        }
        _mapTexture.Update(_texture);
        GD.Print($"<{nameof(TerrainScene2D)}><{nameof(RedrawTreeLayers)}>--->ENDED....");
}


	public void RedrawTreeLayer(IWorldData worldData, string layerName)
	{

	}




/*
	public void RedrawMap()
	{

		else
		{



			GD.Print("Maps count: " + _generationMenu.WorldData.TreeMaps.Count);
			foreach (var item in _generationMenu.WorldData.TreeMaps)
			{
				var key = item.Key;
				var treeMap = item.Value;

				GD.Print("Current color: " + treesColors[key]);
				var treesCount = 0;

				for (int y = 0; y < h; y++)
				{
					for (int x = 0; x < w; x++)
					{
						if (treeMap[y, x])
						{
							treesCount++;
							var treeColor = treesColors[key];

							_texture.SetPixel(x, y, treeColor);
							if (y > 0) _texture.SetPixel(x, y - 1, treeColor);
							if (x > 0) _texture.SetPixel(x - 1, y, treeColor);
							if (x < w - 1) _texture.SetPixel(x + 1, y, treeColor);
							if (y < h - 1) _texture.SetPixel(x, y + 1, treeColor);
						}
					}
				}

				GD.Print("Has trees: " + treesCount);
			}

		}
		_mapTexture.Update(_texture);
	}*/



	public void SetDisplayOptions(DisplayOptions.MapDisplayOptions menu)
	{
		_displayOptions = menu;
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

	private void _on_map_texture_rect_mouse_exited()
	{
		//_cellInfoLabel.Text = "";
	}
}
