using UnityEngine;

public class Logger {
    public static void Log(string format, params object[] args) {
        Debug.Log(string.Format(format, args));
    }
}