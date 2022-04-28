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
using System.IO;

namespace TodoHD;

public static class Logger
{
    private static string _path;

    public static void Initialize(string path)
    {
        _path = path;
    }

    public static void LogException(Exception ex)
    {
        LogInternal($"[Exception] {ex}");
    }
    
    public static void LogDebug(string msg)
    {
        #if DEBUG
        LogInternal($"[Debug] {msg}");
        #endif
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