using System;

namespace TerrainGenerationApp.Domain.Generators;

public class PerlinNoiseGenerator : NoiseMapGenerator
{
    private static readonly byte[] PermOriginal =
    [
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225,
            140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148,
            247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32,
            57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
            74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
            60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
            65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169,
            200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
            52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
            207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
            119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
            129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104,
            218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241,
            81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
            184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
            222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
            151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225,
            140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148,
            247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32,
            57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
            74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
            60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
            65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169,
            200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
            52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
            207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
            119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
            129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104,
            218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241,
            81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
            184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
            222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
    ];





    private byte[] _perm;

    // Algorithm:
    // 1) Determine the coordinates of the grid cell. To limit the input 
    //    coordinates to 256 elements, the bitwise AND operator (&) is used.
    // 2) Determine the local coordinates within the identified cell.
    //    These will be values from 0 to 1 for both x and y.
    // 3) Smooth the local coordinates.
    //    Without smoothing, the noise won't be smooth or, in other words, natural.
    //    A cubic interpolation function is used for smoothing.
    //    For values from 0 to 1, it returns some smoothed value also between 0 and 1.
    // 4) Calculate the final noise value by performing interpolation between 
    //    the corners of a square (in 2D) or a cube (in 3D).
    public PerlinNoiseGenerator()
    {
        _perm = new byte[PermOriginal.Length];
        PermOriginal.CopyTo(_perm, 0);
    }

    public override void SetSeed(int value)
    {
        if (value == 0)
        {
            _perm = new byte[PermOriginal.Length];
            PermOriginal.CopyTo(_perm, 0);
        }
        else
        {
            _perm = new byte[512];
            var random = new Random(value);
            random.NextBytes(_perm);
        }

        _seed = value;
    }

    public override float Noise1D(float x)
    {
        // Determine grid coordinates
        int x0 = (int)Math.Floor(x) & 255;
        int x1 = x0 + 1 & 255;

        // Local coordinates within the grid
        float dx = x - (float)Math.Floor(x);

        // Gradients for both points
        float g0 = Grad1D(_perm[x0], dx);
        float g1 = Grad1D(_perm[x1], dx - 1);

        // Interpolation
        float u = Fade(dx);
        return Lerp(g0, g1, u);
    }

    public override float Noise2D(float x, float y)
    {
        // Determine grid coordinates, coordinates of the square itself
        int y0 = (int)Math.Floor(y) & 255;
        int x0 = (int)Math.Floor(x) & 255;
        int x1 = x0 + 1 & 255;
        int y1 = y0 + 1 & 255;

        // Local coordinates within the grid, within the square
        float dx = x - (float)Math.Floor(x);
        float dy = y - (float)Math.Floor(y);

        // Gradients, or values at the corners within the square
        float g00 = Grad2D(_perm[x0 + _perm[y0]], dx, dy);
        float g01 = Grad2D(_perm[x0 + _perm[y1]], dx, dy - 1);
        float g10 = Grad2D(_perm[x1 + _perm[y0]], dx - 1, dy);
        float g11 = Grad2D(_perm[x1 + _perm[y1]], dx - 1, dy - 1);

        // Interpolation
        float u = Fade(dx);
        float v = Fade(dy);
        float nx0 = Lerp(g00, g10, u);
        float nx1 = Lerp(g01, g11, u);

        return Lerp(nx0, nx1, v);
    }

    public override float Noise3D(float x, float y, float z)
    {
        // Determine grid coordinates, coordinates of the square itself
        int y0 = (int)Math.Floor(y) & 255;
        int x0 = (int)Math.Floor(x) & 255;
        int z0 = (int)Math.Floor(z) & 255;
        int x1 = x0 + 1 & 255;
        int y1 = y0 + 1 & 255;
        int z1 = z0 + 1 & 255;

        // Local coordinates within the grid, within the square
        float dx = x - (float)Math.Floor(x);
        float dy = y - (float)Math.Floor(y);
        float dz = z - (float)Math.Floor(z);

        // Get all gradients for the corners of the cube
        float g000 = Grad3D(_perm[_perm[_perm[x0] + y0] + z0], dx, dy, dz);
        float g001 = Grad3D(_perm[_perm[_perm[x0] + y0] + z1], dx, dy, dz - 1.0f);
        float g010 = Grad3D(_perm[_perm[_perm[x0] + y1] + z0], dx, dy - 1.0f, dz);
        float g011 = Grad3D(_perm[_perm[_perm[x0] + y1] + z1], dx, dy - 1.0f, dz - 1.0f);
        float g100 = Grad3D(_perm[_perm[_perm[x1] + y0] + z0], dx - 1.0f, dy, dz);
        float g101 = Grad3D(_perm[_perm[_perm[x1] + y0] + z1], dx - 1.0f, dy, dz - 1.0f);
        float g110 = Grad3D(_perm[_perm[_perm[x1] + y1] + z0], dx - 1.0f, dy - 1.0f, dz);
        float g111 = Grad3D(_perm[_perm[_perm[x1] + y1] + z1], dx - 1.0f, dy - 1.0f, dz - 1.0f);

        // Smooth coordinates
        float u = Fade(dx);
        float v = Fade(dy);
        float w = Fade(dz);

        // Interpolation along X (4 interpolations)
        float nx00 = Lerp(g000, g100, u);  // lower front edge
        float nx01 = Lerp(g001, g101, u);  // lower back edge
        float nx10 = Lerp(g010, g110, u);  // upper front edge
        float nx11 = Lerp(g011, g111, u);  // upper back edge

        // Interpolation along Y (2 interpolations)
        float ny0 = Lerp(nx00, nx10, v);   // front face
        float ny1 = Lerp(nx01, nx11, v);   // back face

        // Final interpolation along Z
        return Lerp(ny0, ny1, w);
    }

    protected float Grad1D(int h, float x)
    {
        // Directions: left (-x) or right (+x)
        return (h & 1) == 0 ? x : -x;
    }

    protected float Grad2D(int h, float x, float y)
    {
        // Gradient function
        // Source: http://riven8192.blogspot.com/2010/08/calculate-perlinnoise-twice-as-fast.html
        // Here: x is dx, y is dy, z is dz, h is hash
        //      x, y, z in the range [0.0; 1.0] 
        //      hash in the range [0; 255]
        switch (h & 3)  // Limit to 4 directions (0-3)
        {
            case 0x0: return x + y;
            case 0x1: return -x + y;
            case 0x2: return x - y;
            case 0x3: return -x - y;
            default: return 0.0f;
        }
    }

    protected float Grad3D(int h, float x, float y, float z)
    {
        switch (h & 0xF)
        {
            case 0x0: return x + y;
            case 0x1: return -x + y;
            case 0x2: return x - y;
            case 0x3: return -x - y;
            case 0x4: return x + z;
            case 0x5: return -x + z;
            case 0x6: return x - z;
            case 0x7: return -x - z;
            case 0x8: return y + z;
            case 0x9: return -y + z;
            case 0xA: return y - z;
            case 0xB: return -y - z;
            case 0xC: return y + x;
            case 0xD: return -y + z;
            case 0xE: return y - x;
            case 0xF: return -y - z;
            default: return 0.0f;
        }
    }
}
