using Godot;
using System;
using TerrainGenerationApp.Domain.Core;

namespace TerrainGenerationApp.Domain.Generators.WaterErosion;


/// <summary>
/// Defines the rain generation strategy during landscape erosion iterations
/// </summary>
public enum RainRateType
{
    /// <summary>
    /// Rain occurs only at the beginning of iterations (once)
    /// </summary>
    Once,

    /// <summary>
    /// Rain occurs on every iteration without skipping
    /// </summary>
    EveryIteration,

    /// <summary>
    /// Random probability of rain depends on the RainChance parameter
    /// </summary>
    Randomly
}

/// <summary>
/// Defines the type of rain intensity generation
/// </summary>
public enum RainType
{
    /// <summary>
    /// Uses static fixed rain intensity values
    /// </summary>
    StaticValues,

    /// <summary>
    /// Intensity values are randomly generated up to RainPower
    /// </summary>
    RandomValues
}


public partial class WaterErosionApplier : IWaterErosionApplier
{
    public const int MAX_ITERATIONS = 3000;
    public const float MAX_RAIN_POWER = 1.0f;



    public event Action<float[,]> IterationPassed;

    // Terrain generation parameters
    private int mapHeight;
    private int mapWidth;
    private int iterations = 10;









    public int Iterations
    {
        get => iterations;
        set => iterations = Mathf.Clamp(value, 1, MAX_ITERATIONS);
    }


    private RainRateType _rainRateType;
    private RainType _rainType;
    private float _rainPower;
    private float _rainChance;


    public RainRateType RainRateType
    {
        get => _rainRateType;
        set => _rainRateType = value;
    }


    public RainType RainType
    {
        get => _rainType;
        set => _rainType = value;
    }

    public float RainPower
    {
        get => _rainPower;
        set => _rainPower = Mathf.Clamp(value, 0.01f, MAX_RAIN_POWER);
    }

    public float RainChance
    {
        get => _rainChance;
        set => _rainChance = value;

    }




    // Terrain-related arrays for simulation
    private float[,] heightMap;
    private float[,] sediment;
    private float[,] water;
    private float[,] velocity;
    private float[,,] outflow;
    private float[,] inflow;
    private float[,] velocityX;
    private float[,] velocityY;


    /// <summary>
    /// Begin applying erosion to the terrain map
    /// </summary>
    /// <param name="map">2D height map to apply erosion to</param>
    /// <param name="seaLevel">Base sea level for water distribution</param>
    public void BeginApplyingErosion(float[,] map, float seaLevel)
    {
        // Initialize terrain dimensions and arrays
        mapHeight = map.GetLength(0);
        mapWidth = map.GetLength(1);
        heightMap = map;
        sediment = new float[mapHeight, mapWidth];
        water = new float[mapHeight, mapWidth];

        // Initialize velocity vectors (x and y components)
        velocityX = new float[mapHeight, mapWidth];
        velocityY = new float[mapHeight, mapWidth];

        // Reuse these arrays instead of creating new ones each iteration
        outflow = new float[mapHeight, mapWidth, 8]; // 8 directions
        inflow = new float[mapHeight, mapWidth];

        // Direction vectors for 8-directional flow (including diagonals)
        int[] dx = { -1, 1, 0, 0, -1, -1, 1, 1 };
        int[] dy = { 0, 0, -1, 1, -1, 1, -1, 1 };
        float[] weights = { 1, 1, 1, 1, 0.7071f, 0.7071f, 0.7071f, 0.7071f }; // Diagonal distance adjustment

        // Initial water distribution based on sea level
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                water[y, x] = Math.Max(0.0f, seaLevel - heightMap[y, x]);
            }
        }

        // Track total volume for debugging and validation
        float initialWaterVolume = CalculateTotalWaterVolume();
        float initialSedimentVolume = 0.0f;

        // Perform erosion iterations
        for (int i = 0; i < iterations; i++)
        {
            // Handle rain based on configuration
            switch (RainRateType)
            {
                case RainRateType.EveryIteration:
                    ApplyRain();
                    break;
                case RainRateType.Once:
                    if (i == 0)
                    {
                        ApplyRain();
                    }
                    break;
                case RainRateType.Randomly:
                    if (GD.Randf() <= RainChance)
                    {
                        ApplyRain();
                    }
                    break;
            }

            // Reset inflow and outflow arrays
            ResetFlowArrays();

            // Calculate water flow and velocity
            CalculateWaterFlow(dx, dy, weights, seaLevel);

            // Update water volumes and handle erosion
            UpdateWaterAndErosion(dx, dy, seaLevel);

            // Fire event for external visualization or processing
            IterationPassed?.Invoke(heightMap);

            // Optional: Debug volume conservation
            if (i % 10 == 0 && DEBUG_VOLUME_CONSERVATION)
            {
                float currentWaterVolume = CalculateTotalWaterVolume();
                float currentSedimentVolume = CalculateTotalSedimentVolume();
                float totalVolume = currentWaterVolume + currentSedimentVolume;
                float expectedVolume = initialWaterVolume + initialSedimentVolume;
                GD.Print($"Iteration {i}: Water volume: {currentWaterVolume}, Sediment volume: {currentSedimentVolume}");
                GD.Print($"Volume conservation: {totalVolume / expectedVolume * 100}%");
            }
        }
    }

    /// <summary>
    /// Reset flow arrays for the next iteration
    /// </summary>
    private void ResetFlowArrays()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                inflow[y, x] = 0.0f;
                velocityX[y, x] *= 1.0f - VELOCITY_DECAY;
                velocityY[y, x] *= 1.0f - VELOCITY_DECAY;

                for (int d = 0; d < 8; d++)
                {
                    outflow[y, x, d] = 0.0f;
                }
            }
        }
    }

    /// <summary>
    /// Calculate water flow between cells
    /// </summary>
    private void CalculateWaterFlow(int[] dx, int[] dy, float[] weights, float seaLevel)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // Skip cells with no water or underwater cells
                if (water[y, x] <= FLOW_THRESHOLD || heightMap[y, x] < seaLevel)
                    continue;

                float totalOutflow = 0.0f;
                float[] outflowToNeighbors = new float[8];

                // Calculate height differences and potential water flow
                for (int d = 0; d < 8; d++)
                {
                    int nx = x + dx[d];
                    int ny = y + dy[d];

                    // Check boundaries
                    if (nx < 0 || nx >= mapWidth || ny < 0 || ny >= mapHeight)
                        continue;

                    float heightDiff = heightMap[y, x] + water[y, x] - (heightMap[ny, nx] + water[ny, nx]);

                    if (heightDiff > 0)
                    {
                        // Apply weight for diagonal directions
                        outflowToNeighbors[d] = heightDiff * weights[d];
                        totalOutflow += outflowToNeighbors[d];
                    }
                }

                // Distribute water flow with damping
                if (totalOutflow > 0)
                {
                    // Limit outflow to available water
                    float scale = Math.Min(1.0f, water[y, x] / totalOutflow) * (1.0f - DAMPING);

                    for (int d = 0; d < 8; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];

                        // Check boundaries again
                        if (nx < 0 || nx >= mapWidth || ny < 0 || ny >= mapHeight)
                            continue;

                        float flow = outflowToNeighbors[d] * scale;

                        // Store outflow to be processed later
                        outflow[y, x, d] = flow;
                        inflow[ny, nx] += flow;

                        // Update velocity vectors
                        velocityX[y, x] += flow * dx[d];
                        velocityY[y, x] += flow * dy[d];
                    }
                }
            }
        }
    }

    /// <summary>
    /// Update water volumes and handle erosion and deposition
    /// </summary>
    private void UpdateWaterAndErosion(int[] dx, int[] dy, float seaLevel)
    {
        float[,] transportedSediment = new float[mapHeight, mapWidth];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // Calculate total outflow from this cell
                float totalOutflow = 0.0f;
                for (int d = 0; d < 8; d++)
                {
                    totalOutflow += outflow[y, x, d];
                }

                // Update water volume
                float oldWater = water[y, x];
                water[y, x] = Math.Max(0.0f, water[y, x] + inflow[y, x] - totalOutflow);

                // Calculate velocity magnitude
                float velocityMagnitude = (float)Math.Sqrt(velocityX[y, x] * velocityX[y, x] + velocityY[y, x] * velocityY[y, x]);

                // Only process erosion on land cells
                if (heightMap[y, x] >= seaLevel)
                {
                    // Calculate sediment capacity based on slope and velocity
                    float slopeFactor = ComputeSlopeFactor(x, y);
                    float sedimentCapacity = SOLUBILITY * velocityMagnitude * slopeFactor;

                    // Apply erosion based on velocity and slope
                    float erosionRate = ABRASION * velocityMagnitude * slopeFactor;

                    // Reduce erosion in deep water
                    if (water[y, x] > DEEP_WATER_CUTOFF)
                    {
                        erosionRate *= DEEP_WATER_EROSION_FACTOR;
                    }

                    // Apply erosion
                    float erosion = Math.Min(erosionRate, heightMap[y, x] - MIN_HEIGHT);
                    heightMap[y, x] -= erosion;
                    sediment[y, x] += erosion;

                    // Handle sediment capacity
                    if (sediment[y, x] > sedimentCapacity)
                    {
                        // Deposit excess sediment
                        float excessSediment = sediment[y, x] - sedimentCapacity;
                        heightMap[y, x] += excessSediment;
                        sediment[y, x] = sedimentCapacity;
                    }

                    // Calculate sediment transport
                    if (totalOutflow > 0 && oldWater > 0)
                    {
                        float sedimentToTransport = sediment[y, x] * (totalOutflow / oldWater);
                        sediment[y, x] -= sedimentToTransport;

                        // Distribute sediment to neighbors based on outflow
                        for (int d = 0; d < 8; d++)
                        {
                            if (outflow[y, x, d] > 0)
                            {
                                int nx = x + dx[d];
                                int ny = y + dy[d];

                                // Check boundaries
                                if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight)
                                {
                                    float sedimentShare = sedimentToTransport * (outflow[y, x, d] / totalOutflow);
                                    transportedSediment[ny, nx] += sedimentShare;
                                }
                            }
                        }
                    }
                }

                // Apply evaporation
                water[y, x] *= 1.0f - EVAPORATION;

                // Maintain sea level
                if (heightMap[y, x] < seaLevel)
                {
                    water[y, x] = Math.Max(seaLevel - heightMap[y, x], water[y, x]);
                }

                // Clamp height to valid range
                heightMap[y, x] = Mathf.Clamp(heightMap[y, x], MIN_HEIGHT, MAX_HEIGHT);
            }
        }

        // Apply transported sediment
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                sediment[y, x] += transportedSediment[y, x];
            }
        }
    }

    /// <summary>
    /// Compute terrain slope factor for erosion intensity
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Slope factor affecting erosion</returns>
    private float ComputeSlopeFactor(int x, int y)
    {
        int[] dx = { -1, 1, 0, 0, -1, -1, 1, 1 };
        int[] dy = { 0, 0, -1, 1, -1, 1, -1, 1 };
        float[] weights = { 1, 1, 1, 1, 0.7071f, 0.7071f, 0.7071f, 0.7071f };

        float maxSlope = 0.0f;

        // Find maximum slope in neighboring cells
        for (int d = 0; d < 8; d++)
        {
            int nx = x + dx[d];
            int ny = y + dy[d];

            if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight)
            {
                float slope = Math.Abs(heightMap[y, x] - heightMap[ny, nx]) / weights[d];
                maxSlope = Math.Max(maxSlope, slope);
            }
        }

        // Compute and clamp slope factor
        return (float)Math.Pow(Mathf.Clamp(maxSlope / MAX_SLOPE_FACTOR, MIN_SLOPE_FACTOR, 1.0f), SLOPE_POWER);
    }

    /// <summary>
    /// Apply rain to the terrain
    /// </summary>
    private void ApplyRain()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // Apply rain based on configuration and terrain height
                float rainMultiplier = 1.0f;

                // Option: More rain at higher elevations
                if (HEIGHT_BASED_RAIN)
                {
                    rainMultiplier = Mathf.Lerp(1.0f, HEIGHT_RAIN_FACTOR, heightMap[y, x]);
                }

                switch (RainType)
                {
                    case RainType.StaticValues:
                        water[y, x] += RainPower * rainMultiplier;
                        break;
                    case RainType.RandomValues:
                        water[y, x] += (float)GD.RandRange(0.0f, RainPower) * rainMultiplier;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Calculate total water volume for debugging
    /// </summary>
    private float CalculateTotalWaterVolume()
    {
        float totalVolume = 0.0f;
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                totalVolume += water[y, x];
            }
        }
        return totalVolume;
    }

    /// <summary>
    /// Calculate total sediment volume for debugging
    /// </summary>
    private float CalculateTotalSedimentVolume()
    {
        float totalVolume = 0.0f;
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                totalVolume += sediment[y, x];
            }
        }
        return totalVolume;
    }

    // Constants and parameters
    private const float DAMPING = 0.04f;
    private const float SOLUBILITY = 0.01f;
    private const float ABRASION = 0.02f;
    private const float DEEP_WATER_CUTOFF = 0.5f;
    private const float DEEP_WATER_EROSION_FACTOR = 0.1f;
    private const float EVAPORATION = 0.02f;
    private const float VELOCITY_DECAY = 0.05f;
    private const float FLOW_THRESHOLD = 0.001f;
    private const float MIN_HEIGHT = 0.0f;
    private const float MAX_HEIGHT = 1.0f;
    private const float MIN_SLOPE_FACTOR = 0.1f;
    private const float MAX_SLOPE_FACTOR = 0.5f;
    private const float SLOPE_POWER = 0.5f;
    private const bool HEIGHT_BASED_RAIN = true;
    private const float HEIGHT_RAIN_FACTOR = 2.0f;
    private const bool DEBUG_VOLUME_CONSERVATION = false;


    public event Action WaterErosionIterationPassed;
    public void BeginApplyingErosion(IWorldData worldData)
    {
        throw new NotImplementedException();
    }
}




















































/*using Godot;
using System;
using ProceduralGeneration.Utility.Sharp;

namespace ProceduralGeneration.Generators.Sharp;



/// <summary>
/// Defines the rain generation strategy during landscape erosion iterations
/// </summary>
public enum RainRateType
{
	/// <summary>
	/// Rain occurs only at the beginning of iterations (once)
	/// </summary>
	Once,

	/// <summary>
	/// Rain occurs on every iteration without skipping
	/// </summary>
	EveryIteration,

	/// <summary>
	/// Random probability of rain depends on the RainChance parameter
	/// </summary>
	Randomly
}

/// <summary>
/// Defines the type of rain intensity generation
/// </summary>
public enum RainType
{
	/// <summary>
	/// Uses static fixed rain intensity values
	/// </summary>
	StaticValues,

	/// <summary>
	/// Intensity values are randomly generated up to RainPower
	/// </summary>
	RandomValues
}


public partial class WaterErosionApplier : Resource
{
	// IterationsCount [1 - 200]
	// RainPower [0.01 - 1.0]
	// RainType: Complete(value), Random(0 - value)
	// RainRate: Once, EveryIteration, Random - SpecificRate [0 - 1.0] 
	
	
	
	//[Signal]
	//public delegate void IterationPassedEventHandler(float[,] map);

	public const int MAX_ITERATIONS = 3000;
	public const float MAX_RAIN_POWER = 1.0f;



	public event Action<float[,]> IterationPassed;

	// Terrain generation parameters
	private int mapHeight;
	private int mapWidth;
	private int iterations = 10;

	// Terrain-related arrays for simulation
	private float[,] heightMap;
	private float[,] sediment;
	private float[,] water;
	private float[,] velocity;

	// Erosion constants
	private const float EVAPORATION = 0.5f;
	private const float SOLUBILITY = 1.0f;
	private const float ABRASION = 0.1f;
	private const float DEEP_WATER_CUTOFF = 0.1f;
	private const float DAMPING = 0.25f;
	private const float MIN_EROSION_THRESHOLD = 0.01f;
	private const float SEDIMENT_SMOOTHING = 0.2f;














	public int Iterations
	{
		get => iterations;
		set => iterations = Mathf.Clamp(value, 1, MAX_ITERATIONS);
	}


	private RainRateType _rainRateType;
	private RainType _rainType;
	private float _rainPower;
	private float _rainChance;


	public RainRateType RainRateType
	{
		get => _rainRateType;
		set => _rainRateType = value;
	}


	public RainType RainType
	{
		get => _rainType;
		set => _rainType = value;
	}

	public float RainPower
	{
		get => _rainPower;
		set => _rainPower = Mathf.Clamp(value, 0.01f, MAX_RAIN_POWER);
	}

	public float RainChance
	{
		get => _rainChance;
        set => _rainChance = value;

    }





	/// <summary>
	/// Begin applying erosion to the terrain map
	/// </summary>
	/// <param name="map">2D height map to apply erosion to</param>
	/// <param name="seaLevel">Base sea level for water distribution</param>
	public void BeginApplyingErosion(float[,] map, float seaLevel)
	{
		// Initialize terrain dimensions and arrays
		mapHeight = map.GetLength(0);
		mapWidth = map.GetLength(1);
		heightMap = map;
		sediment = new float[mapHeight, mapWidth];
		water = new float[mapHeight, mapWidth];
		velocity = new float[mapHeight, mapWidth];

		// Initial water distribution based on sea level
		for (int x = 0; x < mapWidth; x++)
		{
			for (int y = 0; y < mapHeight; y++)
			{
				water[y, x] = Math.Max(0.0f, seaLevel - heightMap[y, x]);
			}
		}

		// Perform erosion iterations
		for (int i = 0; i < iterations; i++)
		{
*//*			GD.Print($"Iteration: {i + 1} / {iterations}");
			ApplyRain("Complete", 0.02f);*//*

			switch (RainRateType)
			{
				case RainRateType.EveryIteration:
					ApplyRain();
					break;
				case RainRateType.Once:
					if (i == 0)
					{
						ApplyRain();
					}
					break;
				case RainRateType.Randomly:
					if (GD.Randf() <= RainChance)
					{
						ApplyRain();
					}
					break;
			}

			var outflow = new float[mapHeight, mapWidth];
			var inflow = new float[mapHeight, mapWidth];

			// Calculate water flow and velocity
			for (int x = 1; x < mapWidth - 1; x++)
			{
				for (int y = 1; y < mapHeight - 1; y++)
				{
					if (heightMap[y, x] < seaLevel)
						continue;

					float totalOutflow = 0.0f;
					var outflowToNeighbors = new float[4];
					int[] dx = { -1, 1, 0, 0 };
					int[] dy = { 0, 0, -1, 1 };

					// Calculate height differences and potential water flow
					for (int d = 0; d < 4; d++)
					{
						int nx = x + dx[d];
						int ny = y + dy[d];
						float heightDiff = (heightMap[y, x] + water[y, x]) - (heightMap[ny, nx] + water[ny, nx]);

						if (heightDiff > 0)
						{
							outflowToNeighbors[d] = heightDiff;
							totalOutflow += heightDiff;
						}
					}

					// Distribute water flow with damping
					if (totalOutflow > 0)
					{
						float scale = Math.Min(1.0f, water[y, x] / totalOutflow) * (1.0f - DAMPING);
						for (int d = 0; d < 4; d++)
						{
							int nx = x + dx[d];
							int ny = y + dy[d];
							float flow = outflowToNeighbors[d] * scale;
							outflow[y, x] += flow;
							inflow[ny, nx] += flow;
							velocity[y, x] += flow;
						}
					}
				}
			}

			// Erosion and sediment simulation
			for (int x = 1; x < mapWidth - 1; x++)
			{
				for (int y = 1; y < mapHeight - 1; y++)
				{
					float waterVolume = water[y, x] + inflow[y, x] - outflow[y, x];
					float sedimentCapacity = SOLUBILITY * waterVolume;
					float erosion = ABRASION * velocity[y, x];

					// Reduce erosion in deep water
					if (waterVolume > DEEP_WATER_CUTOFF)
						erosion = 0;

					erosion *= ComputeSlopeFactor(x, y);
					heightMap[y, x] -= erosion;
					sediment[y, x] += erosion;

					// Handle excess sediment
					float excessSediment = sediment[y, x] - sedimentCapacity;
					if (excessSediment > 0)
					{
						heightMap[y, x] += excessSediment;
						sediment[y, x] -= excessSediment;
					}

					// Clamp height and update water
					heightMap[y, x] = Mathf.Clamp(heightMap[y, x], 0.0f, 1.0f);
					water[y, x] = Math.Max(0.0f, waterVolume * (1.0f - EVAPORATION));

					// Adjust water level near sea level
					if (heightMap[y, x] < seaLevel)
					{
						water[y, x] = Math.Max(seaLevel - heightMap[y, x], water[y, x]);
					}
				}
			}

			IterationPassed?.Invoke(heightMap);
		}
	}

	/// <summary>
	/// Smooth sediment distribution across the terrain
	/// </summary>
	private void SmoothSediment()
	{
		var newSediment = new float[mapHeight, mapWidth];
		int[] dx = { -1, 1, 0, 0 };
		int[] dy = { 0, 0, -1, 1 };

		for (int x = 1; x < mapWidth - 1; x++)
		{
			for (int y = 1; y < mapHeight - 1; y++)
			{
				float totalSediment = sediment[y, x];
				float count = 1.0f;

				// Average sediment with neighboring cells
				for (int d = 0; d < 4; d++)
				{
					int nx = x + dx[d];
					int ny = y + dy[d];
					totalSediment += sediment[ny, nx];
					count += 1.0f;
				}

				// Interpolate sediment with smoothing factor
				newSediment[y, x] = Mathf.Lerp(sediment[y, x], totalSediment / count, SEDIMENT_SMOOTHING);
			}
		}

		sediment = newSediment;
	}



	/// <summary>
	/// Compute terrain slope factor for erosion intensity
	/// </summary>
	/// <param name="x">X coordinate</param>
	/// <param name="y">Y coordinate</param>
	/// <returns>Slope factor affecting erosion</returns>
	private float ComputeSlopeFactor(int x, int y)
	{
		int[] dx = { -1, 1, 0, 0 };
		int[] dy = { 0, 0, -1, 1 };
		float maxSlope = 0.0f;

		// Find maximum slope in neighboring cells
		for (int d = 0; d < 4; d++)
		{
			int nx = x + dx[d];
			int ny = y + dy[d];

			if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight)
			{
				float slope = Math.Abs(heightMap[y, x] - heightMap[ny, nx]);
				maxSlope = Math.Max(maxSlope, slope);
			}
		}

		// Compute and clamp slope factor
		return (float)Math.Pow(Mathf.Clamp(maxSlope / 2.0f, 0.1f, 1.0f), 0.5);
	}


	/// <summary>
	/// Apply rain to the terrain
	/// </summary>
	private void ApplyRain()
	{
		for (int x = 0; x < mapWidth; x++)
		{
			for (int y = 0; y < mapHeight; y++)
			{
				switch (RainType)
				{
					case RainType.StaticValues:
						water[y, x] += RainPower;
						break;
					case RainType.RandomValues:
						water[y, x] += (float)GD.RandRange(0.0, RainPower);
						break;
				}
			}
		}
	}
	*//*	private void ApplyRain(string rainType, float intensity)
	{
		// For "Complete" rain type, add uniform water across the terrain
		if (rainType == "Complete")
		{
			for (int x = 0; x < mapWidth; x++)
			{
				for (int y = 0; y < mapHeight; y++)
				{
					water[y, x] += intensity;
				}
			}
		}
	}*//*
}
*/