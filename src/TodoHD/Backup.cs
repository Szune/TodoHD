﻿//
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

public static class Backup
{
    private static DateTimeOffset _lastSave = DateTimeOffset.UtcNow;

    public static void Checkpoint(Editor editor)
    {
        if ((DateTimeOffset.UtcNow - _lastSave).TotalMinutes < 5)
            return;
        _lastSave = DateTimeOffset.UtcNow;
        editor.Save();
    }
}