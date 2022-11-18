using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggingUtility
{
    public static void LogWithColor(string logText, Color color) => Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{logText}</color>");

    public static void LogInfo(string infoText) => Debug.Log($"<color=grey>INFO: {infoText}</color>");
}
