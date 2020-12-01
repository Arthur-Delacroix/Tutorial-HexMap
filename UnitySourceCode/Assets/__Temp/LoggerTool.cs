using UnityEngine;

//https://docs.unity3d.com/2018.3/Documentation/Manual/StyledText.html

public class LoggerTool
{
    public static void LogMessage(string _msg)
    {
        Debug.Log("<color=lime>" + _msg + "</color>");
    }

    public static void LogError(string _msg)
    {
        Debug.Log("<color=red>" + _msg + "</color>");
    }

    public static void LogWarning(string _msg)
    {
        Debug.Log("<color=yellow>" + _msg + "</color>");
    }

    public static void LogColorMessage(string _color, string _msg)
    {
        Debug.Log("<color=red>" + _msg + "</color>");
    }
}