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

namespace TodoHD.Modes;

public class ConfirmMode : IMode
{
    private readonly string _question;
    private readonly Action _onConfirm;

    public ConfirmMode(string question, Action onConfirm)
    {
        _question = question;
        _onConfirm = onConfirm;
    }


    public void Init(Editor editor)
    {
    }

    public void PrintUI(Editor editor)
    {
        Console.SetCursorPosition(0, 1);
        var height = Console.WindowHeight - Console.CursorTop - 3;
        Console.WriteLine(_question + new string(' ', Console.WindowWidth - _question.Length));
        var confirm = Input.Confirm(height);
        if (confirm == Confirm.Yes)
        {
            _onConfirm();
        }

        editor.PopMode();
    }

    public void KeyEvent(ConsoleKeyInfo key, Editor editor)
    {
    }
}