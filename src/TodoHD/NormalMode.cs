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

#define USE_PAGED_LIST_BOX

using TodoHD.Controls;
using System;
using System.Linq;
using System.Collections.Generic;
using SaferVariants;

namespace TodoHD;

public class NormalMode : IMode
{
    #if USE_PAGED_LIST_BOX
    PagedListBox<TodoItem> _list;
    #else
    ListBox<TodoItem> _list;
    #endif
    
    private readonly Accumulator _accumulator = new Accumulator(100);

    public void Init(Editor editor)
    {
        Editor.PrintHelpLine(true);
        #if USE_PAGED_LIST_BOX
        _list ??=
            new PagedListBox<TodoItem>(
                itemsFactory: () => editor
                    .GetItems()
                    .Skip(editor.ItemsPerPage * editor.Page)
                    .Take(editor.ItemsPerPage),
                formatter: item =>
                    item.Priority switch
                    {
                        Priority.Whenever => " * ",
                        Priority.Urgent => " (!) ",
                        _ => "-"
                    } + item.Title,
                colorFunc: (item, formattedString, selected) =>
                {
                    if (selected)
                    {
                        if (item.Priority == Priority.Urgent)
                        {
                            return Terminal.Color(Settings.Instance.Theme.TodoItemUrgentSelected, formattedString);
                        }
                        return Terminal.Color(Settings.Instance.Theme.TodoItemSelected, formattedString);
                    }

                    if (item.Priority == Priority.Urgent)
                    {
                        return Terminal.Color(Settings.Instance.Theme.TodoItemUrgent, formattedString);
                    }
                    return Terminal.Color(color: Settings.Instance.Theme.TodoItem, text: formattedString);
                })
            {
                OrderBy = Option.Some<Func<IEnumerable<TodoItem>, IEnumerable<TodoItem>>>(
                    it =>
                        it.OrderByDescending(x => (int) x.Priority)
                            .ThenBy(x => x.Order))
            };
        #else
        _list ??=
            new ListBox<TodoItem>(
                itemsFactory: () => editor
                    .GetItems()
                    .Skip(editor.ItemsPerPage * editor.Page)
                    .Take(editor.ItemsPerPage),
                formatter: item =>
                    item.Priority switch
                    {
                        Priority.Whenever => " * ",
                        Priority.Urgent => " (!) ",
                        _ => "-"
                    } + item.Title)
            {
                OrderBy = Option.Some<Func<IEnumerable<TodoItem>, IEnumerable<TodoItem>>>(
                    it =>
                        it.OrderByDescending(x => (int) x.Priority)
                            .ThenBy(x => x.Order))
            };
        #endif
        _list.Update();
    }

    public void PrintUI(Editor editor)
    {
        PrintItems(editor);
    }

    public void KeyEvent(ConsoleKeyInfo key, Editor editor)
    {
        switch (key.Key)
        {
            case ConsoleKey.Enter:
                editor.PushMode(new ViewMode(_list.SelectedItem));
                _accumulator.Reset();
                break;
            case ConsoleKey.I:
                editor.PushMode(new InsertMode(), true);
                _accumulator.Reset();
                break;
            case ConsoleKey.E:
                editor.PushMode(new EditMode());
                _accumulator.Reset();
                break;
            case ConsoleKey.H:
                editor.PushMode(new HelpMode());
                _accumulator.Reset();
                break;
            case ConsoleKey.D:
                editor.PushMode(new DeleteMode());
                _accumulator.Reset();
                break;
            case ConsoleKey.N:
                _accumulator.Execute(editor.NextPage);
                break;
            case ConsoleKey.P:
                _accumulator.Execute(editor.PrevPage);
                break;
            case ConsoleKey.G:
                if (key.Modifiers == ConsoleModifiers.Shift)
                {
                    if (_list.SelectLast())
                    {
                        editor.LastItem();
                    }
                }
                else
                {
                    if (_list.SelectFirst())
                    {
                        editor.FirstItem();
                    }
                }
                
                _accumulator.Reset();
                break;
            case ConsoleKey.DownArrow:
            case ConsoleKey.J:
                _accumulator.Execute(() =>
                {
                    if (key.Modifiers == ConsoleModifiers.Shift)
                    {
                        if (editor.MoveItemDown())
                        {
                            _list.Update();
                            _list.SelectNext();
                            PrintItems(editor);
                        }
                    }
                    else
                    {
                        if (_list.SelectNext())
                        {
                            editor.NextItem();
                        }
                    }
                });

                break;
            case ConsoleKey.UpArrow:
            case ConsoleKey.K:
                _accumulator.Execute(() =>
                {
                    if (key.Modifiers == ConsoleModifiers.Shift)
                    {
                        if (editor.MoveItemUp())
                        {
                            _list.Update();
                            _list.SelectPrevious();
                            PrintItems(editor);
                        }
                    }
                    else
                    {
                        if (_list.SelectPrevious())
                        {
                            editor.PrevItem();
                        }
                    }
                });
                break;
            case ConsoleKey.D0:
            case ConsoleKey.D1:
            case ConsoleKey.D2:
            case ConsoleKey.D3:
            case ConsoleKey.D4:
            case ConsoleKey.D5:
            case ConsoleKey.D6:
            case ConsoleKey.D7:
            case ConsoleKey.D8:
            case ConsoleKey.D9:
                _accumulator.AccumulateDigit((uint)key.Key - 48);
                break;
            default:
                _accumulator.Reset();
                break;
        }
    }

    private void PrintItems(Editor editor)
    {
        Console.SetCursorPosition(0, 1);
        #if USE_PAGED_LIST_BOX
        _list.Print(Console.BufferWidth, Console.BufferHeight - 2);
        #else
        _list.Print();
        #endif
    }
}

