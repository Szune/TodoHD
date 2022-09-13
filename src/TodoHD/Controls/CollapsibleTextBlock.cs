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
using System.Linq;
using SaferVariants;

namespace TodoHD.Controls;

public class CollapsibleTextBlock
{
    private readonly string _text;
    private readonly IOption<Func<string, string>> _lineFormatter;
    private IOption<(int Width, int Height)> _collapsedSize = Option.None<(int, int)>();

    public CollapsibleTextBlock(string text, Func<string, string> lineFormatter = null)
    {
        _text = text;
        _lineFormatter = Option.NoneIfNull(lineFormatter);
    }

    public void Collapse(int width, int height)
    {
        _collapsedSize = Option.Some((width, height));
    }

    public void Expand()
    {
        _collapsedSize = Option.None<(int, int)>();
    }

    public void Print(int availableWidth, int availableHeight)
    {
        var size = _collapsedSize.ValueOr((availableWidth, availableHeight));
        var lineFormatter = _lineFormatter.ValueOr(null);
        var text =
            CollapseLines(
                _text
                    .ExceptEndingNewlines()
                    .ReplaceLineEndings("\n")
                    .Split('\n')
                    .SelectMany(line =>
                        Output.WriteLineWrappingList(lineFormatter?.Invoke(line) ?? line, size.Width).Lines)
                    .ToList(),
                size.Height);

        Console.WriteLine(text);
    }

    private static string CollapseLines(List<string> lines, int collapseHeight)
    {
        var formatted =
            string.Join(
                Environment.NewLine,
                lines.Take(Math.Max(collapseHeight, 1)));

        if (lines.Count <= collapseHeight)
        {
            return formatted;
        }

        if (formatted.Length >= 6)
            formatted = formatted[..^6];

        // clear out full line
        var lengthBeforeTrim = formatted.Length;
        formatted = formatted.TrimEnd();
        var trimmedLength = formatted.Length;

        // add collapse hint before clearing out the rest of the line
        formatted += " [...]";

        if (lengthBeforeTrim > trimmedLength)
        {
            formatted += new string(' ', lengthBeforeTrim - trimmedLength);
        }

        return formatted;
    }
}