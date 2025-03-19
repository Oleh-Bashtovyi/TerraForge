using Godot;
using System;
using TerrainGenerationApp.Enums;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Scenes;

public partial class TerrainScene2D : Control
{
    private GameComponents.GenerationMenu.MapGenerationMenu _generationMenu;
    private MapDisplayOptions _displayOptions;

    private Image _texture;
    private ImageTexture _mapTexture;
    private Gradient _terrainGradient;
    private Gradient _waterGradient;

    // NODES REFERENCED WITH "%" IN SCENE
    private Label _cellInfoLabel;
    private TextureRect _mapTextureRect;


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



    public void RedrawMap()
    {
        var displayFormat = _displayOptions.CurDisplayFormat;
        var slopesThreshold = _displayOptions.CurSlopeThreshold;
        var waterLevel = _displayOptions.CurWaterLevel;
        var slopes = _generationMenu.CurSlopesMap;
		var trees = _generationMenu.CurTreesMap;
        var map = _generationMenu.CurTerrainMap;
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
                    // Display water
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



            foreach (var item in _generationMenu.TreesMaps.TreeMaps)
            {
                var key = item.Key;
                var treeMap = item.Value;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (treeMap[y, x])
                        {
                            var treeColor = Colors.Red;

                            if (key == "Palm")
                            {
                                treeColor = Colors.Purple;
                            }
                            else if (key == "Oak")
                            {
                                treeColor = Colors.Red;
                            }
                            else if (key == "Pine")
                            {
                                treeColor = Colors.DarkBlue;
                            }

                            _texture.SetPixel(x, y, treeColor);
                            if (y > 0) _texture.SetPixel(x, y - 1, treeColor);
                            if (x > 0) _texture.SetPixel(x - 1, y, treeColor);
                            if (x < w - 1) _texture.SetPixel(x + 1, y, treeColor);
                            if (y < h - 1) _texture.SetPixel(x, y + 1, treeColor);
                        }
                    }
                }
            }

        }
        _mapTexture.Update(_texture);
    }




    public void SetGenerationMenu(GameComponents.GenerationMenu.MapGenerationMenu menu)
    {
        if (_generationMenu != null)
        {
            _generationMenu.OnMapGenerated -= _handleOnMapGenerated;
        }
		_generationMenu = menu;
        if (_generationMenu != null)
        {
			_generationMenu.OnMapGenerated += _handleOnMapGenerated;
        }
    }


    public void SetDisplayOptions(MapDisplayOptions menu)
    {
        if (_displayOptions != null)
        {
            _displayOptions.OnDisplayOptionsChanged -= _handleDisplayOptionsChanged;
        }
        _displayOptions = menu;
        if (_displayOptions != null)
        {
            _displayOptions.OnDisplayOptionsChanged += _handleDisplayOptionsChanged;
        }
    }



    private void _handleOnMapGenerated(MapGenerationResult result)
    {
        var map = _generationMenu.CurTerrainMap;
        var h = map.GetLength(0);
        var w = map.GetLength(1);
        if (_texture.GetSize() != new Vector2I(h, w))
        {
            _texture.Resize(w, h);
            _mapTexture.SetImage(_texture);
        }
        RedrawMap();
    }


    private void _handleDisplayOptionsChanged()
    {
        RedrawMap();
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
