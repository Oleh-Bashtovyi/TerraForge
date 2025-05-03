namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

public interface IInputLineValueTracker
{
    public void TrackValue(IInputLine inputLine);
    public void TrackValue(string id, object value);
}