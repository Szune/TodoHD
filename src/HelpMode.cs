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
using System.Collections.Generic;

namespace TodoHD
{
    public class HelpMode : IMode
    {
        public void Init(Editor editor) { }
        public void PrintUI(Editor editor)
        {
            Console.Clear();
            Output.WithBackground(ConsoleColor.DarkCyan, () => {
                Console.WriteLine("== Keybindings ==");
            });
            Output.WithForeground(ConsoleColor.White, () => {
                    Output.WriteLineWrapping($" [Enter] Show item");
                    Output.WriteLineWrapping($" [Backspace] Back");
                    Output.WriteLineWrapping($" [I] New item  [H] Help [Q] Quit");
                    Output.WriteLineWrapping($" [E] Edit item [D] Delete item");
                    Output.WriteLineWrapping($" [G] First item [Shift+G] Last item");
                    Output.WriteLineWrapping($" [J] Next item [K] Prev item");
                    Output.WriteLineWrapping($" [Shift+J] Move item [Shift+K] Move item");
                    Output.WriteLineWrapping($" [N] Next Page [P] Prev Page");
                    });
        }

        public void KeyEvent(ConsoleKeyInfo key, Editor editor)
        {
            switch(key.Key)
            {
                case ConsoleKey.Backspace:
                    editor.PopMode();
                    break;
            }
        }

    }
}
