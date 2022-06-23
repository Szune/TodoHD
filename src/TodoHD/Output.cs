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
using System.Linq;
using System.Text;

namespace TodoHD;

public static class Output
{
    public static void WithBackground(ConsoleColor color, Action print)
    {
        var old = Console.BackgroundColor;
        Console.BackgroundColor = color;
        print();
        Console.BackgroundColor = old;
    }

    public static void WithForeground(ConsoleColor color, Action print)
    {
        var old = Console.ForegroundColor;
        Console.ForegroundColor = color;
        print();
        Console.ForegroundColor = old;
    }

    /// <summary>
    /// Writes the result to the console.
    /// </summary>
    public static void WriteLineWrapping(string fullLine)
    {
        var sb = new StringBuilder();
        _ = WriteLineWrapping(sb, fullLine, Console.WindowWidth);
        Console.Write(sb);
    }

    /// <summary>
    /// Writes the result to the console.
    /// </summary>
    public static void WriteLineWrapping(string fullLine, out int lines)
    {
        var sb = new StringBuilder();
        lines = WriteLineWrapping(sb, fullLine, Console.WindowWidth);
        Console.Write(sb);
    }

    /// <summary>
    /// Writes the result to the given StringBuilder
    /// </summary>
    public static void WriteLineWrapping(StringBuilder sb, string fullLine, out int lines)
    {
        lines = WriteLineWrapping(sb, fullLine, Console.WindowWidth);
    }

    /// <summary>
    /// Writes to a StringBuilder and returns the lines written
    /// </summary>
    public static int WriteLineWrapping(StringBuilder sb, string fullLine, int windowWidth)
    {
        var writtenOnThisItem = fullLine.Length;
        var newlines = fullLine.CountNewLines();

        switch (writtenOnThisItem.CompareTo(windowWidth))
        {
            case 0:
                if (newlines < 1)
                {
                    sb.AppendLine(fullLine);
                    return 1;
                }

                WriteWithNewLines(sb, fullLine, windowWidth);
                return newlines + 1;
            case < 0:
                if (newlines < 1)
                {
                    sb.Append(fullLine);
                    sb.AppendLine(new string(' ', windowWidth - writtenOnThisItem));
                    return 1;
                }

                WriteWithNewLines(sb, fullLine, windowWidth);
                return newlines + 1;
            case > 0:
                return newlines + WriteMultiline(sb, fullLine, windowWidth);
        }
    }

    private static void WriteWithNewLines(StringBuilder sb, string fullLine, int windowWidth)
    {
        fullLine = fullLine.ReplaceLineEndings("\n");
        int x = 0, i = 0, max = fullLine.Length;
        for (; i < max; ++i, ++x)
        {
            if (x == windowWidth)
            {
                sb.AppendLine();
                sb.Append(fullLine[i]);
                x = 0;
                continue;
            }

            switch (fullLine[i])
            {
                case '\r':
                    continue;
                case '\n':
                    sb.Append(new string(' ', windowWidth - x));
                    sb.Append(fullLine[i]);
                    x = 0;
                    continue;
                default:
                    sb.Append(fullLine[i]);
                    break;
            }
        }

        if (fullLine[^1] != '\n')
        {
            sb.AppendLine();
        }
    }

    /// <summary>
    /// Writes to a StringBuilder and returns the lines written
    /// </summary>
    public static int WriteMultiline(StringBuilder sb, string fullLine, int windowWidth)
    {
        var fullLength = fullLine.Length;
        // split up in multiple lines
        var ii = 0;
        for (var writtenToSb = 0;
             writtenToSb < fullLength;
             ii++, writtenToSb += windowWidth)
        {
            var leftToWrite = fullLength - writtenToSb;
            if (leftToWrite >= windowWidth)
            {
                sb.AppendLine(fullLine[(ii * windowWidth)..(ii * windowWidth + windowWidth)]);
            }
            else
            {
                sb.Append(fullLine[(ii * windowWidth)..(ii * windowWidth + leftToWrite)]);
                sb.AppendLine(new string(' ', windowWidth - leftToWrite));
            }
        }

        return ii + 1;
    }

    /// <summary>
    /// Writes a list and returns the lines written
    /// </summary>
    public static (int LineCount, List<string> Lines) WriteLineWrappingList(string fullLine, int windowWidth)
    {
        var fullLength = fullLine.Length;
        var lines = 0;
        var list = new List<string>();
        var sb = new StringBuilder();

        for (var i = 0; i < fullLength; i++)
        {
            var curChar = fullLine[i];
            if (curChar == '\n')
            {
                lines++;
                if (sb.Length == 0)
                {
                    list.Add(new string(' ', windowWidth));
                }
                else
                {
                    list.Add(sb.Append(new string(' ', windowWidth - sb.Length)).ToString());
                    sb.Clear();
                }
            }
            else if (curChar == '\r')
            {
                // skip
            }
            else
            {
                if (curChar == '\t')
                {
                    if (windowWidth - sb.Length > 0)
                    {
                        sb.Append(new string(' ', Math.Min(windowWidth - sb.Length, Terminal.TabWidth)));
                    }
                    else
                    {
                        Logger.LogAssertion(windowWidth - sb.Length > 0,
                            $"WindowWidth - sb.Length = {windowWidth - sb.Length}");
                    }
                    // otherwise, skip, don't think it would make sense to write it on the next line
                }
                else
                {
                    sb.Append(curChar);
                }

                // TODO: needs to handle chars that take up more space than 1 column
                // TODO: in all the calculations
                // TODO: see if there are functions you can remove

                if (sb.Length < windowWidth) continue;
                lines++;
                list.Add(sb.ToString());
                sb.Clear();
            }
        }

        if (sb.Length > 0)
        {
            lines++;
            list.Add(sb.Append(new string(' ', windowWidth - sb.Length)).ToString());
        }

        if (list.Count > 1 && string.IsNullOrWhiteSpace(list[^1]))
        {
            list.RemoveAt(list.Count - 1);
        }

        if (list.Count == 0)
        {
            list.Add(new string(' ', windowWidth));
            lines = 1;
        }

        return (lines, list);
    }

    /// <summary>
    /// Returns the max width and total height that was measured.
    /// </summary>
    public static (int MaxWidth, int MaxHeight) Measure(string fullLine, int windowWidth)
    {
        return MeasureLinesWrappedNew(fullLine, windowWidth);
        var writtenOnThisItem = fullLine.Length;
        var newlines = fullLine.CountNewLines();

        return writtenOnThisItem.CompareTo(windowWidth) switch
        {
            0 => (windowWidth, newlines + 1),
            < 0 => (newlines > 0 ? windowWidth : writtenOnThisItem, newlines + 1),
            > 0 => (windowWidth, newlines + MeasureLinesWrapped(fullLine, windowWidth))
        };
    }

    /// <summary>
    /// Writes a list and returns the lines written
    /// </summary>
    public static (int MaxWidth, int MaxHeight) MeasureLinesWrappedNew(string fullLine, int windowWidth)
    {
        var fullLength = fullLine.Length;
        var MaxHeight = 0;
        var MaxWidth = 0;
        var charsOnLine = 0;

        for (var i = 0; i < fullLength; i++)
        {
            var curChar = fullLine[i];
            if (curChar == '\n')
            {
                if (i != fullLength - 1)
                {
                    MaxHeight++;
                    MaxWidth = windowWidth;
                    charsOnLine = 0;
                }
            }
            else if (curChar == '\r')
            {
                // skip
            }
            else
            {
                if (curChar == '\t')
                {
                    if (windowWidth - charsOnLine > 0)
                    {
                        charsOnLine += Math.Min(windowWidth - charsOnLine, Terminal.TabWidth);
                    }
                    else
                    {
                        Logger.LogAssertion(windowWidth - charsOnLine > 0,
                            $"WindowWidth - charsOnLine = {windowWidth - charsOnLine}");
                    }
                    // otherwise, skip, don't think it would make sense to write it on the next line
                }
                else
                {
                    charsOnLine += 1;
                }
                // TODO: needs to handle chars that take up more space than 1 column
                // TODO: in all the calculations

                if (charsOnLine < windowWidth) continue;
                MaxHeight++;
                MaxWidth = Math.Max(MaxWidth, charsOnLine);
                charsOnLine = 0;
            }
        }

        if (charsOnLine > 0)
        {
            MaxHeight++;
            MaxWidth = windowWidth;
        }

        // assumes at least 1 line
        return (MaxWidth, Math.Max(1, MaxHeight));
    }

    public static int MeasureLinesWrapped(string fullLine, int windowWidth)
    {
        var fullLength = fullLine.Length;
        // split up in multiple lines
        var ii = 0;
        for (var writtenToSb = 0;
             writtenToSb < fullLength;
             ii++, writtenToSb += windowWidth)
        {
        }

        return ii + 1;
        //Logger.LogDebug("lines with original algor = " + (ii + 1));

//        var line = fullLength / WindowWidth;
//        if (line * WindowWidth < fullLength)
//        {
//            line++;
//        }
//
//        Logger.LogDebug("lines with new algor = " + line);
//        return line;
    }
}