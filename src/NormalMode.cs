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
    public class NormalMode : IMode
    {
        public void Init(Editor editor)
        {
            editor.PrintHelpLine(true);
        }
        public void PrintUI(Editor editor)
        {
            PrintItems(editor);
        }

        public void KeyEvent(ConsoleKeyInfo key, Editor editor)
        {
            switch(key.Key)
            {
                case ConsoleKey.Enter:
                    editor.PushMode(new ViewMode());
                    break;
                case ConsoleKey.I:
                    editor.PushMode(new InsertMode(), true);
                    break;
                case ConsoleKey.E:
                    editor.PushMode(new EditMode());
                    break;
                case ConsoleKey.H:
                    editor.PushMode(new HelpMode());
                    break;
                case ConsoleKey.D:
                    editor.PushMode(new DeleteMode());
                    break;
                case ConsoleKey.N:
                    editor.NextPage();
                    break;
                case ConsoleKey.P:
                    editor.PrevPage();
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.J:
                    if(key.Modifiers == ConsoleModifiers.Shift)
                    {
                        editor.MoveItemDown();
                    }
                    else
                    {
                        editor.NextItem();
                    }
                    break;
                case ConsoleKey.UpArrow:
                case ConsoleKey.K:
                    if(key.Modifiers == ConsoleModifiers.Shift)
                    {
                        editor.MoveItemUp();
                    }
                    else
                    {
                        editor.PrevItem();
                    }
                    break;
            }

        }

        void PrintItems(Editor editor)
        {
            Console.SetCursorPosition(0,1);
            var sb = new StringBuilder();
            editor
                .GetItems()
                .Skip(editor.ItemsPerPage * (editor.Page - 1))
                .Take(editor.ItemsPerPage)
                .Select((item,index) => new{item,index})
                .ToList()
                .ForEach(it => {
                    var c = sb.Length;
                    if(editor.Item == it.index + 1)
                    {
                        sb.Append(" >");
                    }
                    sb.Append(it.item.Priority switch {
                        Priority.Whenever => " * ",
                        Priority.Urgent => " (!) ",
                        _ => "-"
                    });
                    sb.Append(it.item.Title);
                    c = sb.Length - c;
                    sb.AppendLine(new string(' ', Console.BufferWidth - 1 - c));
                });
            Console.Write(sb);
        }
    }
}
