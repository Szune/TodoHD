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

namespace TodoHD;

public static class Terminal
{
    public static string Foreground(ForegroundColor color, string text)
    {
        return $"\x1b[1;{color.Value}m{text}\x1b[0m";
    }

    public static string Background(BackgroundColor color, string text)
    {
        return $"\x1b[1;{color.Value}m{text}\x1b[0m";
    }

    public static string Color(Color color, string text)
    {
        if (color.Background != BackgroundColors.None)
        {
            text = Background(color.Background, text);
        }
        if (color.Foreground != ForegroundColors.None)
        {
            text = Foreground(color.Foreground, text);
        }
        return text;
    }
}

public record ForegroundColor(string Value)
{
    public static implicit operator ForegroundColor(string value)
    {
        return new ForegroundColor(value);
    }
}
public static class ForegroundColors
{
    public static readonly ForegroundColor None = new("");
    public static readonly ForegroundColor Reset = new("0");
    public static readonly ForegroundColor Black = new("30");
    public static readonly ForegroundColor DarkRed = new("31");
    public static readonly ForegroundColor DarkGreen = new("32");
    public static readonly ForegroundColor DarkYellow = new("33");
    public static readonly ForegroundColor DarkBlue = new("34");
    public static readonly ForegroundColor DarkMagenta = new("35");
    public static readonly ForegroundColor DarkCyan = new("36");
    public static readonly ForegroundColor Gray = new("37");
    public static readonly ForegroundColor DarkGray = new("90");
    public static readonly ForegroundColor Red = new("91");
    public static readonly ForegroundColor Green = new("92");
    public static readonly ForegroundColor Yellow = new("93");
    public static readonly ForegroundColor Blue = new("94");
    public static readonly ForegroundColor Magenta = new("95");
    public static readonly ForegroundColor Cyan = new("96");
    public static readonly ForegroundColor White = new("97");
}

public record BackgroundColor(string Value)
{
    public static implicit operator BackgroundColor(string value)
    {
        return new BackgroundColor(value);
    }
}
public static class BackgroundColors
{
    public static readonly BackgroundColor None = new("");
    public static readonly BackgroundColor Reset = new("0");
    public static readonly BackgroundColor Black = new("40");
    public static readonly BackgroundColor DarkRed = new("41");
    public static readonly BackgroundColor DarkGreen = new("42");
    public static readonly BackgroundColor DarkYellow = new("43");
    public static readonly BackgroundColor DarkBlue = new("44");
    public static readonly BackgroundColor DarkMagenta = new("45");
    public static readonly BackgroundColor DarkCyan = new("46");
    public static readonly BackgroundColor Gray = new("47");
    public static readonly BackgroundColor DarkGray = new("100");
    public static readonly BackgroundColor Red = new("101");
    public static readonly BackgroundColor Green = new("102");
    public static readonly BackgroundColor Yellow = new("103");
    public static readonly BackgroundColor Blue = new("104");
    public static readonly BackgroundColor Magenta = new("105");
    public static readonly BackgroundColor Cyan = new("106");
    public static readonly BackgroundColor White = new("107");
}
