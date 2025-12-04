using UnityEngine;

public static class GameLogger
{
    public static bool IsEnabled { get; set; } = true;

    public static void Log(object message)
    {
        if (!IsEnabled) return;
        Debug.Log(message);
    }

    public static void Log(object message, Object context)
    {
        if (!IsEnabled) return;
        Debug.Log(message, context);
    }

    public static void LogWarning(object message)
    {
        if (!IsEnabled) return;
        Debug.LogWarning(message);
    }

    public static void LogWarning(object message, Object context)
    {
        if (!IsEnabled) return;
        Debug.LogWarning(message, context);
    }

    public static void LogError(object message)
    {
        if (!IsEnabled) return;
        Debug.LogError(message);
    }

    public static void LogError(object message, Object context)
    {
        if (!IsEnabled) return;
        Debug.LogError(message, context);
    }

    public static void LogException(System.Exception exception)
    {
        if (!IsEnabled) return;
        Debug.LogException(exception);
    }

    public static void LogException(System.Exception exception, Object context)
    {
        if (!IsEnabled) return;
        Debug.LogException(exception, context);
    }
}