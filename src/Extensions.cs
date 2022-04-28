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

public static class Extensions
{
    public static int OccurrencesOf(this string s, char toFind)
    {
        var count = 0;
        for(var n = s.Length; --n >= 0;)
            if(s[n] == toFind)
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
}