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

using System;
using System.Collections.Generic;
using System.Linq;
using SaferVariants;
using TodoHD.Controls;

namespace TodoHD.Modes;

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
                        it.OrderByDescending(x => (int)x.Priority)
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

    private void Find(Editor editor)
    {
        if (Console.CursorTop > Console.WindowHeight - 3)
        {
            Console.SetCursorPosition(0, Console.WindowHeight - 3);
        }

        Console.WriteLine("Find:");
        Console.CursorVisible = true;
        var maybeText = Input.GetString();
        Console.CursorVisible = false;
        maybeText.HandleNone(() =>
        {
            Init(editor);
            PrintUI(editor);
        }).Then(text =>
        {
            var results = editor.Find(text).ToList();
            var termSize = Terminal.GetWindowSize();
            if (results.Count > 0)
            {
                Console.SetCursorPosition(0, 1);
                var height = Console.WindowHeight - Console.CursorTop - 3;
                Console.WriteLine("Search results:" + new string(' ', Console.WindowWidth - "Search results:".Length));
                var selected = Input.Select(results, height);
                if (selected.IsSome(out var selectedItem))
                {
                    editor.PushMode(new ViewMode(selectedItem.Item));
                }
                else
                {
                    Init(editor);
                    PrintUI(editor);
                }
            }
            else
            {
                editor.PushMode(new FloatingBoxMode($"No results for '{text}'", termSize.Width, termSize.Height));
            }
        });
    }

    private void ConfirmSave(Editor editor)
    {
        Console.SetCursorPosition(0, 1);
        var height = Console.WindowHeight - Console.CursorTop - 3;
        Console.WriteLine("Save?" + new string(' ', Console.WindowWidth - "Save?".Length));
        var confirm = Input.Confirm(height);
        if (confirm == Confirm.Yes)
        {
            editor.Save();
        }

        Init(editor);
        PrintUI(editor);
    }

    public void KeyEvent(ConsoleKeyInfo key, Editor editor)
    {
        switch (key.Key)
        {
            case ConsoleKey.Enter:
            {
                if (_list.SelectedItem.IsSome(out var item))
                {
                    editor.PushMode(new ViewMode(item));
                }

                _accumulator.Reset();
            }
                break;
            case ConsoleKey.I:
                editor.PushMode(new InsertMode(), true);
                _accumulator.Reset();
                break;
            case ConsoleKey.E:
            {
                if (_list.SelectedItem.IsSome(out var item))
                {
                    editor.PushMode(new EditMode(item));
                }

                _accumulator.Reset();
            }
                break;
            case ConsoleKey.H:
                editor.PushMode(new HelpMode());
                _accumulator.Reset();
                break;
            case ConsoleKey.D:
            {
                if (_list.SelectedItem.IsSome(out var item))
                {
                    editor.PushMode(new DeleteMode(item));
                }

                _accumulator.Reset();
            }
                break;
            case ConsoleKey.S:
                ConfirmSave(editor);
                _accumulator.Reset();
                break;
            case ConsoleKey.F:
                Find(editor);
                _accumulator.Reset();
                break;
            case ConsoleKey.N:
                _accumulator.Execute(() =>
                {
                    if (_list.NextPage())
                    {
                        PrintItems(editor);
                    }
                });
                break;
            case ConsoleKey.P:
                _accumulator.Execute(() =>
                {
                    if (_list.PreviousPage())
                    {
                        PrintItems(editor);
                    }
                });
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
        _list.Print(Console.WindowWidth, Console.WindowHeight - 4);
#else
        _list.Print();
#endif
    }
}