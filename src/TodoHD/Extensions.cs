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
using SaferVariants;

namespace TodoHD;

public static class Extensions
{
    public static int OccurrencesOf(this string s, char toFind)
    {
        var count = 0;
        for (var n = s.Length; --n >= 0;)
            if (s[n] == toFind)
                count++;
        return count;
    }

    public static int CountNewLines(this string s)
        => s.OccurrencesOf('\n');

    public static string ExceptEndingNewline(this string s)
    {
        if (s.Length < 1)
            return s;

        if (s[^1] == '\n')
        {
            return s.Length == 1
                ? ""
                : s[..^2];
        }

        return s;
    }

    public static StringBuilder ExceptEndingNewline(this StringBuilder sb)
    {
        if (sb.Length < 1)
            return sb;

        return sb[^1] != '\n'
            ? sb
            : sb.Remove(sb.Length - 1, 1);
    }

    public static IOption<TResult> Map<T, TResult>(this T x, Func<T, IOption<TResult>> transform) =>
        Option.NoneIfNull(x).Map(transform);


    public static T Apply<T, TOption>(this T value, IOption<TOption> opt, Func<TOption, Func<T, T>> transform)
    {
        // I don't think this is great for performance..
        // but it's fun, so maybe profile it.
        return opt.MapOr(value, t => transform(t)(value));
    }

    public static int CountNonTerminalEscapeCodeChars(this string s)
    {
        // I really don't think this is a good way to "solve" it
        // TODO: finish writing an actual parser of escape codes or delete this code
        const int FLAG_NONE = 0;
        const int FLAG_ESC_BEGIN = 1;
        const int FLAG_CODE_BEGIN = 2;
        var count = 0;
        var flag = FLAG_NONE;
        for (var i = 0; i < s.Length; ++i)
        {
            switch (flag)
            {
                case FLAG_NONE:
                    switch (s[i])
                    {
                        case '\x1b':
                            flag = FLAG_ESC_BEGIN;
                            break;
                        default:
                            ++count;
                            break;
                    }

                    break;
                case FLAG_ESC_BEGIN:
                    switch (s[i])
                    {
                        case '[':
                            flag = FLAG_CODE_BEGIN;
                            break;
                        default:
                            flag = FLAG_NONE;
                            break;
                    }

                    break;
                case FLAG_CODE_BEGIN:
                    if (s[i] is > 'A')
                    {
                        flag = FLAG_NONE;
                    }

                    // missing a lot of flags and escape codes that move the cursor etc
                    break;
            }
        }

        return count;
    }
}