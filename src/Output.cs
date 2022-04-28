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
        _ = WriteLineWrapping(sb, fullLine, Console.BufferWidth);
        Console.Write(sb);
    }
    
    /// <summary>
    /// Writes the result to the console.
    /// </summary>
    public static void WriteLineWrapping(string fullLine, out int lines)
    {
        var sb = new StringBuilder();
        lines = WriteLineWrapping(sb, fullLine, Console.BufferWidth);
        Console.Write(sb);
    }
    
    /// <summary>
    /// Writes the result to the given StringBuilder
    /// </summary>
    public static void WriteLineWrapping(StringBuilder sb, string fullLine, out int lines)
    {
        lines = WriteLineWrapping(sb, fullLine, Console.BufferWidth);
    }

    /// <summary>
    /// Writes to a StringBuilder and returns the lines written
    /// </summary>
    public static int WriteLineWrapping(StringBuilder sb, string fullLine, int bufferWidth)
    {
        var writtenOnThisItem = fullLine.Length;
        var newlines = fullLine.CountNewLines();

        switch (writtenOnThisItem.CompareTo(bufferWidth))
        {
            case 0:
                if (newlines < 1)
                {
                    sb.AppendLine(fullLine);
                    return 1;
                }

                WriteWithNewLines(sb, fullLine, bufferWidth);
                return newlines + 1;
            case < 0:
                if (newlines < 1)
                {
                    sb.Append(fullLine);
                    sb.AppendLine(new string(' ', bufferWidth - writtenOnThisItem));
                    return 1;
                }
                
                WriteWithNewLines(sb, fullLine, bufferWidth);
                return newlines + 1;
            case > 0:
                return newlines + WriteMultiline(sb, fullLine, bufferWidth);
        }
    }

    private static void WriteWithNewLines(StringBuilder sb, string fullLine, int bufferWidth)
    {
        fullLine = fullLine.ReplaceLineEndings("\n");
        int x = 0, i = 0, max = fullLine.Length;
        for (; i < max; ++i, ++x)
        {
            if (x == bufferWidth)
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
                    sb.Append(new string(' ', bufferWidth - x));
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
    public static int WriteMultiline(StringBuilder sb, string fullLine, int bufferWidth)
    {
        var fullLength = fullLine.Length;
        // split up in multiple lines
        var ii = 0;
        for (var writtenToSb = 0;
            writtenToSb < fullLength;
            ii++, writtenToSb += bufferWidth)
        {
            var leftToWrite = fullLength - writtenToSb;
            if (leftToWrite >= bufferWidth)
            {
                sb.AppendLine(fullLine[(ii * bufferWidth)..(ii * bufferWidth + bufferWidth)]);
            }
            else
            {
                sb.Append(fullLine[(ii * bufferWidth)..(ii * bufferWidth + leftToWrite)]);
                sb.AppendLine(new string(' ', bufferWidth - leftToWrite));
            }
        }

        return ii + 1;
    }

    /// <summary>
    /// Writes to a StringBuilder and returns the max width and max height that was written
    /// </summary>
    public static (int MaxWidth, int MaxHeight) Measure(string fullLine, int bufferWidth)
    {
        var writtenOnThisItem = fullLine.Length;
        var newlines = fullLine.CountNewLines();
        
        return writtenOnThisItem.CompareTo(bufferWidth) switch
        {
            0 => (bufferWidth, newlines + 1),
            < 0 => (newlines > 0 ? bufferWidth : writtenOnThisItem, newlines + 1),
            > 0 => (bufferWidth, newlines + MeasureLinesWrapped(fullLine, bufferWidth))
        };
    }

    public static int MeasureLinesWrapped(string fullLine, int bufferWidth)
    {
        var fullLength = fullLine.Length;
        // split up in multiple lines
        var ii = 0;
        for (var writtenToSb = 0;
            writtenToSb < fullLength;
            ii++, writtenToSb += bufferWidth) { }
        // TODO: rewrite the above without a loop
        // TODO: looks like it's just this:
        /*
         * ii = fullLength / bufferWidth
         * if (ii * bufferWidth < fullLength)
         * {
         *     ii++;
         * }
         */

        return ii + 1;
    }

}
