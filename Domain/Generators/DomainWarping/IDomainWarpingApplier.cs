namespace TerrainGenerationApp.Domain.Generators.DomainWarping;

public interface IDomainWarpingApplier
{
    public float[,] ApplyWarping(float[,] map);
}