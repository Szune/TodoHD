//
// TodoHD is a CLI tool/TUI to organize stuff you need to do.
// Copyright (C) 2022  Carl Erik Patrik Iwarson
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace TodoHD;

public static class Logger
{
    private static string _path;
    private static readonly List<string> DebugLines = new List<string>();

    public static void Initialize(string path)
    {
        _path = path;
    }

#if DEBUG
    public static List<string> GetLines()
    {
        return DebugLines;
    }
#endif

    public static void LogException(Exception ex)
    {
        LogInternal($"[Exception] {ex}");
    }

    [Conditional("DEBUG")]
    public static void LogAssertion(
        bool condition,
        string message = "",
        [CallerArgumentExpression("condition")]
        string callingArgumentExpression = "",
        [CallerMemberName] string callingMethod = "",
        [CallerFilePath] string callingFilePath = "",
        [CallerLineNumber] int callingFileLine = 0
    )
    {
        if (condition) return;
        LogInternal(
            !string.IsNullOrWhiteSpace(message)
                ? $"[Assert] {callingFilePath}:{callingFileLine} at {callingMethod} Assertion failed '{callingArgumentExpression}': {message}"
                : $"[Assert] {callingFilePath}:{callingFileLine} at {callingMethod} Assertion failed '{callingArgumentExpression}'");
    }

    [Conditional("DEBUG")]
    public static void LogDebug(string msg)
    {
        DebugLines.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [Debug] {msg}");
    }

    [Conditional("DEBUG")]
    public static void LogTrace(string msg)
    {
        if (Settings.Instance.Trace)
        {
            DebugLines.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [Trace] {msg}");
        }
    }

    [Conditional("DEBUG")]
    public static void DebugSave()
    {
        try
        {
            File.AppendAllLines(GetLogPath(), DebugLines);
        }
        catch
        {
            // should have some kind of notification if this happens
        }
    }

    private static void LogInternal(string msg)
    {
        try
        {
            File.AppendAllText(GetLogPath(), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {msg}");
        }
        catch
        {
            // should have some kind of notification if this happens
        }
    }

    private static string GetLogPath()
    {
        var backupLogPath = $"{_path}.{DateTime.Now:yyyyMMdd}.log";
        return backupLogPath;
    }
}