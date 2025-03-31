using Godot;
using System;
using System.Runtime.CompilerServices;
using TerrainGenerationApp.Enums;

namespace TerrainGenerationApp.Utilities;

public class Logger<T> where T : class
{
    public string ClassName => typeof(T).Name;

    public void LogMethodStart([CallerMemberName] string callerName = "")
    {
        LogMethod(LogMark.Start, callerName);
    }
    public void LogMethodEnd([CallerMemberName] string callerName = "")
    {
        LogMethod(LogMark.End, callerName);
    }
    public void LogMethod(LogMark mark = LogMark.Default, [CallerMemberName] string callerName = "")
    {
        switch (mark)
        {
            case LogMark.Default:
                GD.Print($"<{ClassName}><{callerName}>;");
                break;
            case LogMark.Start:
                GD.Print($"<{ClassName}><{callerName}><START>;");
                break;
            case LogMark.End:
                GD.Print($"<{ClassName}><{callerName}><END>");
                break;
            case LogMark.Error:
                GD.PrintErr($"<{ClassName}><{callerName}><ERROR>");
                break;
            default:
                throw new NotSupportedException($"Such log type is not supported: {mark}");
        }
    }

    public void LogError(string text, [CallerMemberName] string callerName = "")
    {
        Log(text, LogMark.Error, callerName);
    }
    public void Log(string text, LogMark mark = LogMark.Default, [CallerMemberName] string callerName = "")
    {
        switch (mark)
        {
            case LogMark.Default:
                GD.Print($"<{ClassName}><{callerName}> - {text}");
                break;
            case LogMark.Start:
                GD.Print($"<{ClassName}><{callerName}><START> - {text}");
                break;
            case LogMark.End:
                GD.Print($"<{ClassName}><{callerName}><END> - {text}");
                break;
            case LogMark.Error:
                GD.PrintErr($"<{ClassName}><{callerName}><ERROR> - {text}");
                break;
            default:
                throw new NotSupportedException($"Such log type is not supported: {mark}");
        }
    }
}
