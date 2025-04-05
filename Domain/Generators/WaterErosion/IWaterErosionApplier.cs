using System;
using TerrainGenerationApp.Domain.Core;

namespace TerrainGenerationApp.Domain.Generators.WaterErosion;

public interface IWaterErosionApplier
{
    public event Action WaterErosionIterationPassed;
    public void BeginApplyingErosion(IWorldData worldData);
}