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

namespace TodoHD
{
    public class EditString
    {
        /*
         * multiple LinkedLists for insertion/deletion performance
         * "jump lists"? (multiple smaller linkedlists connected through one main linkedlist)
         */

    }

    public class TextBox
    {
        public string GetInput()
        {
            var pos = Console.CursorTop;
            Console.SetCursorPosition(0, pos);
            
            ConsoleKeyInfo read;
            while((read = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                
            }
            return "";
        }
    }
}
