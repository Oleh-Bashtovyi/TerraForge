using System;
using TerrainGenerationApp.Data;

namespace TerrainGenerationApp.Generators.WaterErosion;

public interface IWaterErosionApplier
{
    public event Action WaterErosionIterationPassed;
    public void BeginApplyingErosion(IWorldData worldData);
}