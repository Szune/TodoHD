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
using System.Linq;
using TodoHD.Modes;
using TodoHD.Rendering;

namespace TodoHD.Controls;

public class FloatingBoxMode : IMode
{
    private readonly string _message;
    private readonly int _availableWidth;
    private readonly int _availableHeight;

    public FloatingBoxMode(string message, int availableWidth, int availableHeight)
    {
        _message = message;
        _availableWidth = availableWidth;
        _availableHeight = availableHeight;
    }

    public void Init(Editor editor)
    {
    }

    public void PrintUI(Editor editor)
    {
        // make a message box with ascii box codes
        const int boxCharactersTotalWidth = 2;
        const int xMarginPerSide = 1;
        const int boxWidthSpace = boxCharactersTotalWidth + (xMarginPerSide * 2);

        //var width = ((availableWidth / 4) * 3); // 75%
        var width = Math.Max(Math.Min((_availableWidth / 4) * 3, _message.Length + boxWidthSpace), 30); // 75%
        var innerWidth = Math.Max(width - boxWidthSpace, 0);
        var wrapped = Output
            .WriteLineWrappingList(_message, innerWidth)
            .Lines
            .Take(Math.Max(_availableHeight - 4, 0))
            .ToList();

        var style = new Style(ForegroundColors.White, BackgroundColors.Black);
        var boxSpan = new Span();
        boxSpan.Add($"┌{new string('─', Math.Max(innerWidth + (xMarginPerSide * 2), 0))}┐", style);
        boxSpan.AddLine();
        foreach (var line in wrapped)
        {
            boxSpan.Add($"│ {line} │", style);
            boxSpan.AddLine();
        }

        boxSpan.Add($"├────┐{new string(' ', innerWidth - 4)} │", style);
        boxSpan.AddLine();
        boxSpan.Add($"│ OK │{new string(' ', innerWidth - 4)} │", style);
        boxSpan.AddLine();
        boxSpan.Add($"└────┴{new string('─', Math.Max(innerWidth - 5 + (xMarginPerSide * 2), 0))}┘", style);

        var posX = (_availableWidth - width) / 2;
        var posY = (_availableHeight - 2 - wrapped.Count) / 2;
        Console.SetCursorPosition(posX, posY);
        foreach (var segment in boxSpan)
        {
            if (segment is LineSpan)
            {
                posY += 1;
                Console.SetCursorPosition(posX, posY);
            }
            else
            {
                Console.Write(segment.ToString());
            }
        }

        //var box = new StringBuilder();
        //var indent = new string(' ', (_availableWidth - width) / 2);

        //box.Append(indent);
        //box.Append('┌');
        //box.Append('─', Math.Max(innerWidth + (xMarginPerSide * 2), 0));
        //box.AppendLine("┐");
        //foreach (var line in wrapped)
        //{
        //    box.AppendLine($"{indent}│ {line} │");
        //}

        //box.AppendLine($"{indent}├────┐{new string(' ', innerWidth - 4)} │");
        //box.AppendLine($"{indent}│ OK │{new string(' ', innerWidth - 4)} │");
        //box.Append($"{indent}└────┴");
        //box.Append('─', Math.Max(innerWidth - 5 + (xMarginPerSide * 2), 0));
        //box.AppendLine("┘");

        //var startY = (_availableHeight - wrapped.Count) / 2;
        //Console.SetCursorPosition(0, startY);
        //Console.Write(box.ToString());
    }

    public void KeyEvent(ConsoleKeyInfo key, Editor editor)
    {
        switch (key.Key)
        {
            case ConsoleKey.Enter:
                editor.PopMode();
                break;
        }
    }

    /*
┌─┐
│ │
└─┘
┌┬┐
├┼┤
└┴┘
─ 
├
┤
┴
┬
│
┘
┐
┌
└
┼
     */
}