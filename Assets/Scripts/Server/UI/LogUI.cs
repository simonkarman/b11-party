using Logging;
using System;
using UnityEngine;
using UnityEngine.UI;

public class LogUI : MonoBehaviour {
    [SerializeField]
    private Image background = default;
    [SerializeField]
    private Text text = default;
    [SerializeField]
    private Color traceColor = Color.magenta;
    [SerializeField]
    private Color infoColor = Color.black;
    [SerializeField]
    private Color warnColor = Color.Lerp(Color.yellow, Color.red, 0.3f);
    [SerializeField]
    private Color errorCollor = Color.red;

    private Color GetColor(LogLevel logLevel) {
        switch (logLevel) {
        case LogLevel.TRACE:
            return traceColor;
        default:
        case LogLevel.INFO:
            return infoColor;
        case LogLevel.WARNING:
            return warnColor;
        case LogLevel.ERROR:
            return errorCollor;
        }
    }

    public void SetLogLevel(LogLevel logLevel) {
        Color color = GetColor(logLevel);
        background.color = Color.Lerp(Color.white, color, 0.2f);
        text.color = color;
    }

    public void SetLogText(string logText) {
        text.text = logText;
    }
}