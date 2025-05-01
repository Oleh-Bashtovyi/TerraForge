using TerrainGenerationApp.Domain.Generators.DomainWarping;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;


namespace TerrainGenerationApp.Scenes.FeatureOptions.DomainWarping;

public partial class DomainWarpingOptions : OptionsContainer
{
    private readonly DomainWarpingApplier _domainWarpingApplier = new();
    private float _warpingStrength = 1.0f;
	private float _noiseScale = 1.0f;

    [InputLine(Description = "Strength")]
    [InputLineSlider(0.1f, 100.0f, 0.1f)]
    public float WarpingStrength
    {
		get => _warpingStrength;
        set
        {
			_warpingStrength = value;
            _domainWarpingApplier.WarpingStrength = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Scale")]
    [InputLineSlider(0.1f, 50.0f, 0.1f)]
    public float NoiseScale
    {
        get => _noiseScale;
        set
        {
            _noiseScale = value;
            _domainWarpingApplier.XNoise.Frequency = value;
            _domainWarpingApplier.YNoise.Frequency = value;
            InvokeParametersChangedEvent();
        }
    }

	public DomainWarpingApplier DomainWarpingApplier => _domainWarpingApplier;

    public override void _Ready()
	{
        base._Ready();
        _domainWarpingApplier.XNoise.Frequency = 0.125f;
		_domainWarpingApplier.YNoise.Frequency = 0.125f;
        InputLineManager.CreateInputLinesForObject(obj: this, container: this);
    }
}
