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
using System.Linq;
using System.Collections.Generic;
using SaferVariants;

namespace TodoHD
{
    public class NormalMode : IMode
    {
        ListBox<TodoItem> _list;

        public void Init(Editor editor)
        {
            editor.PrintHelpLine(true);
            _list ??= 
                new(
                    editor
                        .GetItems()
                        .Skip(editor.ItemsPerPage * (editor.Page - 1))
                        .Take(editor.ItemsPerPage)
                        .ToList,
                    item =>     
                        item.Priority switch {
                            Priority.Whenever => " * ",
                            Priority.Urgent => " (!) ",
                            _ => "-"
                        } + item.Title)
            {
                OrderBy = Option.Some<Func<IEnumerable<TodoItem>, IEnumerable<TodoItem>>>(it => it.OrderByDescending(x => (int)x.Priority).ThenBy(x => x.Order))
            };

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
                    editor.PushMode(new ViewMode(_list.SelectedItem));
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
                case ConsoleKey.G:
                    if (key.Modifiers == ConsoleModifiers.Shift)
                    {
                        if(_list.SelectLast())
                        {
                            editor.LastItem();
                        }
                    }
                    else
                    {
                        if(_list.SelectFirst())
                        {
                            editor.FirstItem();
                        }
                    }
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.J:
                    if(key.Modifiers == ConsoleModifiers.Shift)
                    {
                        if(editor.MoveItemDown())
                        {
                            _list.Update();
                            _list.SelectNext();
                            PrintItems(editor);
                        }
                    }
                    else
                    {
                        if(_list.SelectNext())
                        {
                            editor.NextItem();
                        }
                    }
                    break;
                case ConsoleKey.UpArrow:
                case ConsoleKey.K:
                    if(key.Modifiers == ConsoleModifiers.Shift)
                    {
                        if(editor.MoveItemUp())
                        {
                            _list.Update();
                            _list.SelectPrevious();
                            PrintItems(editor);
                        }
                    }
                    else
                    {
                        if(_list.SelectPrevious())
                        {
                            editor.PrevItem();
                        }
                    }
                    break;
            }

        }

        void PrintItems(Editor editor)
        {
            Console.SetCursorPosition(0,1);
            _list.Print();
        }
    }
}
