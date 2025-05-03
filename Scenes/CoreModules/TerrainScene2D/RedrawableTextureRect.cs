using System;
using Godot;

namespace TerrainGenerationApp.Scenes.CoreModules.TerrainScene2D;

public partial class RedrawableTextureRect : TextureRect
{
    private Image _image;
    private ImageTexture _imageTexture;
    private Color _clearColor = Colors.Black;
    private bool _imageTextureResizeRequired;

    public Image GetImage() => _image;


    public override void _Ready()
    {
        _image = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
        _imageTexture = ImageTexture.CreateFromImage(_image);
    }

    public void SetClearColor(Color color)
    {
        _clearColor = color;
    }

    public void SetSize(int height, int width)
    {
        var actualSize = _image.GetSize();

        if (actualSize.X != width || actualSize.Y != height)
        {
            _image.Resize(width, height);
            _imageTextureResizeRequired = true;
        }
    }

    public void Clear()
    {
        _image.Fill(_clearColor);
    }

    public void Redraw(Func<int, int, Color> redrawFunc)
    {
        var size = _image.GetSize();

        for (int row = 0; row < size.Y; row++)
        {
            for (int col = 0; col < size.X; col++)
            {
                var color = redrawFunc(row, col);
                _image.SetPixel(col, row, color);
            }
        }
    }

    public void UpdateTexture()
    {
        if (_imageTextureResizeRequired)
        {
            _imageTexture.SetImage(_image);
            _imageTextureResizeRequired = false;
        }
        else
        {
            _imageTexture.Update(_image);
        }
    }
}
