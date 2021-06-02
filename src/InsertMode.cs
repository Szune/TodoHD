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
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace TodoHD
{
    public class InsertMode : IMode
    {
        public void Init(Editor editor)
        {
            editor.PrintHelpLine(true);
        }

        public void PrintUI(Editor editor)
        {
        }

        public void KeyEvent(ConsoleKeyInfo key, Editor editor)
        {
            Console.CursorVisible = true;
            Console.WriteLine("== Inserting new item ==");
            Console.WriteLine("Title:");
            var title = Helpers.GetNonEmptyString();

            Console.WriteLine("Description (<br> for newlines):");
            var description = Helpers.GetNonEmptyString().Replace("<br>", Environment.NewLine);

            //Console.WriteLine("Category:");
            //var category = Helpers.GetNonEmptyString();
            var category = "";

            Console.WriteLine("Priority:");
            var priority = Helpers.GetPriority();

            editor.InsertItem(new(-1, -1, title, description, category, priority));
            Console.CursorVisible = false;
            editor.Save();
            editor.PopMode();
        }
    }
}
