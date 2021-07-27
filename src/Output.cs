//
// TodoHD is a CLI tool/TUI to organize stuff you need to do.
// Copyright (C) 2021  Carl Erik Patrik Iwarson
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

namespace TodoHD
{
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

        public static void WriteLineWrapping(string fullLine)
        {
            var sb = new StringBuilder();
            _ = WriteLineWrapping(sb, fullLine, Console.BufferWidth);
            Console.Write(sb);
        }
        
        public static void WriteLineWrapping(string fullLine, out int lines)
        {
            var sb = new StringBuilder();
            lines = WriteLineWrapping(sb, fullLine, Console.BufferWidth);
            Console.Write(sb);
        }

        public static int WriteLineWrapping(StringBuilder sb, string fullLine, int bufferWidth)
            // returns lines written
        {
            var writtenOnThisItem = fullLine.Length;

            switch (writtenOnThisItem.CompareTo(bufferWidth))
            {
                case 0:
                    sb.AppendLine(fullLine);
                    return 1;
                case < 0:
                    sb.Append(fullLine);
                    sb.AppendLine(new string(' ', bufferWidth - writtenOnThisItem));
                    return 1;
                case > 0:
                    return WriteMultiline(sb, fullLine, bufferWidth);
            }
        }

        public static int WriteMultiline(StringBuilder sb, string fullLine, int bufferWidth)
            // returns lines written
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

    }
}
