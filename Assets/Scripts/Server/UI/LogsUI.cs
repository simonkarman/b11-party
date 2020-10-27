using Logging;
using UnityEngine;

public class LogsUI : MonoBehaviour, ILogAppender {
    [SerializeField]
    private GameObject logUIPrefab = default;
    [SerializeField]
    private Transform body = default;
    [SerializeField]
    private LogLevel logLevel = LogLevel.INFO;
    [SerializeField]
    private int maxLogCount = 15;

    protected void Awake() {
        Logging.Logger.AddAppender(this);
    }

    public void Append(LogLevel logLevel, LogMetaData logMetaData, string message, params object[] args) {
        Transform logUIObject;
        if (body.childCount < maxLogCount) {
            logUIObject = Instantiate(logUIPrefab, body).transform;
        } else {
            logUIObject = body.GetChild(0);
        }
        string logMoment = logMetaData.GetTimestamp().ToString("HH:mm:ss");
        logUIObject.SetAsLastSibling();
        logUIObject.name = "LogUI-" + logMoment;
        LogUI logUI = logUIObject.GetComponent<LogUI>();
        logUI.SetLogLevel(logLevel);
        logUI.SetLogText(string.Format(string.Format("{0} <b>{1}</b>: {2}", logMoment, logLevel, message), args));
    }

    public LogLevel GetLogLevel() {
        return logLevel;
    }
}