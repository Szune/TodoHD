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
using System.Linq;

namespace TodoHD
{
    public class DeleteMode : IMode
    {
        public void Init(Editor editor) { }
        public void PrintUI(Editor editor)
        {
            var item = editor.GetSelectedItem();
            Console.Clear();
            Console.WriteLine($"== {item.Title} ==");
            Console.WriteLine($"   <{item.Priority}>");
            item
                .Description
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .ToList()
                .ForEach(part =>
                    Console.WriteLine($"{new string(' ', 2)}{part}"));

            Console.WriteLine();
            Console.WriteLine("Are you sure you want to delete? (y/n)");
            Console.CursorVisible = true;
            var delete = Helpers.GetNonEmptyString();
            Console.CursorVisible = false;
            if(delete.ToUpperInvariant() == "Y")
            {
                editor.DeleteItemById(item.Id);
                editor.PopMode();
            }
            else
            {
                editor.PopMode();
            }
        }

        public void KeyEvent(ConsoleKeyInfo key, Editor editor)
        {
        }

    }
}
