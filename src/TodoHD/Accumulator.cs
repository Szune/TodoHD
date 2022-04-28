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

namespace TodoHD;

public class Accumulator
{
    private readonly int _max;
    private uint _accumulated;

    public Accumulator(int max = -1)
    {
        _max = max;
    }

    /// <summary>
    /// Only valid for values between 0-9, that should be more obvious tbh
    /// </summary>
    public void AccumulateDigit(uint digit)
    {
        _accumulated = Math.Clamp((_accumulated * 10) + digit, 0, _max == -1 ? uint.MaxValue : (uint)_max);
    }

    public void Execute(Action action, bool reset = true)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (_accumulated < 1)
        {
            action();
            goto end;
        }
        for (var i = 0; i < _accumulated; ++i)
        {
            action();
        }

        end:
        if (reset)
        {
            Reset();
        }
    }

    public void Reset()
    {
        _accumulated = 0;
    }
}