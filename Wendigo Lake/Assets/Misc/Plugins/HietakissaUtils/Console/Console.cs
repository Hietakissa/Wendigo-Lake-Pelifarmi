using UnityEngine;

public static class Console
{
    /*public static void Log(string message, params string[] tags)
    {
        ConsoleController.Instance.Log(message, tags);
    }

    public static void Log(string message, Color color, params string[] tags)
    {
        ConsoleController.Instance.Log(message, color, tags);
    }*/

    public static void Log(object message, params string[] tags)
    {
        ConsoleController.Instance.Log(message.ToString(), tags);
    }

    public static void Log(object message, Color color, params string[] tags)
    {
        ConsoleController.Instance.Log(message.ToString(), color, tags);
    }
}