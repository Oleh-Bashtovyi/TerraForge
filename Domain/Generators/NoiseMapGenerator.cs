using Godot;
using System;
using System.Runtime.CompilerServices;
using TerrainGenerationApp.Domain.Enums;

namespace TerrainGenerationApp.Domain.Generators;

public abstract class NoiseMapGenerator
{
	private const short INLINE = 256; // MethodImplOptions.AggressiveInlining;
	private const float InitialAmplitude = 1 / 1.75f;


    protected int _seed; 
    private float _frequency = 0.01f;
	private int _octaves = 2;
	private Vector2 _offset = Vector2.Zero;
	private float _persistence = 0.5f;
	private float _lacunarity = 2.0f;
	private float _warpingStrength = 1.0f;
	private float _warpingSize = 1.0f;
	private bool _enableWarping = true;


	public FractalType Fractal { get; set; } = FractalType.Fbm;

	public int Seed
    {
        get => _seed;
        set => SetSeed(value);
    }

    public float Frequency
	{
		get => _frequency;
		set => _frequency = value;
	}

	public Vector2 Offset
	{
		get => _offset;
		set => _offset = value;
	}

	public int Octaves
	{
		get => _octaves;
		set => _octaves = Mathf.Clamp(value, 1, 10);
	}

	public float Persistence
	{
		get => _persistence;
		set => _persistence = value;
	}

	public float Lacunarity
	{
		get => _lacunarity;
		set => _lacunarity = value;
	}

	public float WarpingStrength
	{
		get => _warpingStrength;
		set => _warpingStrength = value;
	}

	public float WarpingSize
	{
		get => _warpingSize;
		set => _warpingSize = value;
	}

	public bool EnableWarping
	{
		get => _enableWarping;
		set => _enableWarping = value;
	}


	// To generate map we need:
	// Map height       #[1; 1024]     # The vertical size of the generated map.
	// Map width        #[1; 1024]     # The horizontal size of the generated map.
	// Seed             #[0; inf]      # The starting point for random number generation to ensure reproducibility.
	// Scale            #[0.0001; 100] # Controls the zoom level of the noise; smaller values zoom in, larger values zoom out.
	// Octaves          #[1; 10]       # Number of layers of noise to combine for added detail.
	// Persistence      #[0; 1]        # Controls the amplitude reduction for each octave; lower values make octaves fade out faster.
	// Frequency        #[0; inf]      # Controls the base frequency of the noise; higher values increase detail but may distort.
	// Lacunarity       #[1; inf]      # Controls the increase in frequency for each octave; higher values make noise more detailed.
	// Offset           #[-inf; inf]   # Shifts the noise pattern by a specific amount in x and y directions.
	// Warping strength #[0; inf]      # Controls the intensity of the warping effect.
	// Warping size     #[0; inf]      # Controls the scale of the warping effect.
	public float[,] GenerateMap(int mapHeight, int mapWidth)
	{
		var map = new float[mapHeight, mapWidth];

		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				var xSample = (x + _offset.X) * _frequency;
				var ySample = (y + _offset.Y) * _frequency;
				var height = 0.0f;

				if (_enableWarping)
				{
					height = GetNoise(xSample, ySample, _warpingStrength * Noise2D(xSample * _warpingSize, ySample * _warpingSize));
				}
				else
				{
					height = GetNoise(xSample, ySample);
				}

				map[y, x] = (height + 1.0f) / 2.0f;
			}
		}
		return map;
	}

	protected float GetNoise(float x, float y)
	{
		if (Fractal == FractalType.Fbm)
		{
			return Octaves2D(x, y);
		}

		if (Fractal == FractalType.Ridged)
		{
			return RidgeOctaves2D(x, y);
		}

		throw new ArgumentException("Invalid fractal type");
	}

	protected float GetNoise(float x, float y, float z)
	{
		if (Fractal == FractalType.Fbm)
		{
			return Octaves3D(x, y, z);
		}

		if (Fractal == FractalType.Ridged)
		{
			return RidgeOctaves3D(x, y, z);
		}

		throw new ArgumentException("Invalid fractal type");
	}

	public float Octaves2D(float x, float y)
	{
		var sum = 0.0f;
		var amplitude = InitialAmplitude;
		var frequency = 1.0f;
		var maxValue = 0.0f;

		for (int i = 0; i < _octaves; i++)
		{
			sum += Noise2D(x * frequency, y * frequency) * amplitude;
			maxValue += amplitude;
			amplitude *= _persistence;
			frequency *= _lacunarity;
		}

		return sum / maxValue;
	}


	public float Octaves3D(float x, float y, float z)
	{
		var sum = 0.0f;
		var amplitude = InitialAmplitude;
		var frequency = 1.0f;
		var maxValue = 0.0f;

		for (int i = 0; i < _octaves; i++)
		{
			sum += Noise3D(x * frequency, y * frequency, z * frequency) * amplitude;
			maxValue += amplitude;
			amplitude *= _persistence;
			frequency *= _lacunarity;
		}

		return sum / maxValue;
	}

	public float RidgeOctaves2D(float x, float y)
	{
		var sum = 0.0f;
		var amplitude = InitialAmplitude;
		var maxValue = 0.0f;
		for (int i = 0; i < _octaves; i++)
		{
			float noise = FastAbs(Noise2D(x, y));
			// noise *= noise;
			sum += noise * amplitude;
			x *= _lacunarity;
			y *= _lacunarity;
			maxValue += amplitude;
			amplitude *= _persistence;
		}

		return sum / maxValue;
	}

	public float RidgeOctaves3D(float x, float y, float z)
	{
		var sum = 0.0f;
		var amplitude = InitialAmplitude;
		var maxValue = 0.0f;
		for (int i = 0; i < _octaves; i++)
		{
			sum += (1.0f - FastAbs(Noise3D(x, y, z))) * amplitude;
			x *= _lacunarity;
			y *= _lacunarity;
			z *= _lacunarity;
			maxValue += amplitude;
			amplitude *= _persistence;
		}
		return sum / maxValue;
	}

	public abstract void SetSeed(int seed);
    public abstract float Noise1D(float x);
	public abstract float Noise2D(float x, float y);
	public abstract float Noise3D(float x, float y, float z);

	[MethodImpl(INLINE)]
	protected static float FastMin(float a, float b) { return a < b ? a : b; }

	[MethodImpl(INLINE)]
	protected static float FastMax(float a, float b) { return a > b ? a : b; }

	[MethodImpl(INLINE)]
	protected static float FastAbs(float f) { return f < 0 ? -f : f; }

	[MethodImpl(INLINE)]
	protected static int FastFloor(float f) { return f >= 0 ? (int)f : (int)f - 1; }

	[MethodImpl(INLINE)]
	protected static int FastRound(float f) { return f >= 0 ? (int)(f + 0.5f) : (int)(f - 0.5f); }

	[MethodImpl(INLINE)]
	protected static float Lerp(float a, float b, float t) { return a + t * (b - a); }

	[MethodImpl(INLINE)]
	protected static float Fade(float t) { return t * t * t * (t * (t * 6 - 15) + 10); }
}
