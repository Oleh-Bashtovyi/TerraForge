namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

#nullable enable
public interface IInputLine
{
    public string Id { get; }
    public object? GetValueAsObject();
    public bool TrySetValue(object? value, bool invokeEvent = true);
    public void SetValueTracker(IInputLineValueTracker tracker);
    public void EnableInput();
    public void DisableInput();
    public void SetFontSize(int fontSize);
    public void SetDescription(string  description);
}