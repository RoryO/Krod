﻿using BepInEx.Logging;
using System.Diagnostics;

namespace Krod
{
    internal static class Log
    {
        private static ManualLogSource _logSource;

        internal static void Init(ManualLogSource logSource)
        {
            _logSource = logSource;
        }

        [Conditional("DEBUG")]
        internal static void Debug(object data) => _logSource.LogDebug(data);
        [Conditional("DEBUG")]
        internal static void Error(object data) => _logSource.LogError(data);
        [Conditional("DEBUG")]
        internal static void Fatal(object data) => _logSource.LogFatal(data);
        [Conditional("DEBUG")]
        internal static void Info(object data) => _logSource.LogInfo(data);
        [Conditional("DEBUG")]
        internal static void Message(object data) => _logSource.LogMessage(data);
        [Conditional("DEBUG")]
        internal static void Warning(object data) => _logSource.LogWarning(data);
    }
}