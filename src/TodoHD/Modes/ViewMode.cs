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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SaferVariants;
using TodoHD.Controls;

namespace TodoHD.Modes;

public class ViewMode : IMode
{
    /// <summary>
    /// Stores amount of times an action is to be performed, should be cleared after action
    /// </summary>
    private readonly Accumulator _accumulator = new Accumulator(100);

    private int _stepStart;
    private readonly TodoItem _item;
    private readonly PagedListBox<TodoStep> _list;
    private readonly HelpLine _help;

    public ViewMode(TodoItem item)
    {
        // active items as well as selected items
        _help = new HelpLine(
            text:
            "[+] Add step [-] Remove step [T] Edit step [E] Edit item [Space] Mark step [A] Active step [O] Reader [Q] Quit");
        _item = item;
        _item.Steps ??= new();
        _list = new PagedListBox<TodoStep>(itemsFactory: GetItems,
            formatter: step => $" [{(step.Completed ? 'x' : step.Active ? 'o' : ' ')}] {step.Text}",
            colorFunc: (step, formattedString, selected) =>
                (selected, step.Active, step.Completed) switch
                {
                    (true, _, true) => Settings.Instance.Theme.StepCompletedSelected.Apply(formattedString),
                    (true, true, _) => Settings.Instance.Theme.StepActiveSelected.Apply(formattedString),
                    (true, _, _) => Settings.Instance.Theme.StepSelected.Apply(formattedString),
                    (_, _, true) => Settings.Instance.Theme.StepCompleted.Apply(formattedString),
                    (_, true, _) => Settings.Instance.Theme.StepActive.Apply(formattedString),
                    _ => Settings.Instance.Theme.Step.Apply(formattedString)
                })
        {
            HidePageNumberIfSinglePage = true,
            OrderBy = Option.Some<Func<IEnumerable<TodoStep>, IEnumerable<TodoStep>>>(value: it =>
                it.OrderBy(keySelector: x => x.Order))
        };
    }

    public void Init(Editor editor)
    {
        Console.Clear();
        _help.Print();
    }

    IEnumerable<TodoStep> GetItems() => (_item.Steps ?? new()).OrderBy(i => i.Order);

    public void PrintUI(Editor editor)
    {
        Console.SetCursorPosition(0, _help.Height);
        Console.WriteLine(Settings.Instance.Theme.TodoItemHeader.Apply($"== {_item.Title} =="));
        switch (_item.Priority)
        {
            case Priority.Whenever:
                Console.WriteLine($"   {BackgroundColors.Green.Apply($"<{_item.Priority}>")}");
                break;
            case Priority.Urgent:
                Console.WriteLine($"   {BackgroundColors.Red.Apply($"*{_item.Priority}*")}");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_item.Priority),
                    $"Priority '{_item.Priority}' is not handled in {nameof(PrintUI)}");
        }

        var availableHeight = Console.WindowHeight - 1;
        if (_item.Steps.Count != 0)
        {
            availableHeight /= 3;
        }

        var textBlock = new CollapsibleTextBlock(_item.Description);
        textBlock.Print(Console.WindowWidth, availableHeight);

        Console.WriteLine();
#if DEBUG
        Console.WriteLine(Terminal.Color(Settings.Instance.Theme.TodoItemHeader,
            $"== Steps {Console.WindowHeight - (Console.CursorTop + 1) - 1} lines left =="));
#else
        Console.WriteLine(Terminal.Color(Settings.Instance.Theme.TodoItemHeader, "== Steps =="));
#endif

        _stepStart = Console.CursorTop;

        PrintSteps(editor);
    }

    void PrintSteps(Editor editor)
    {
        Console.SetCursorPosition(0, _stepStart);
        _list.Print(Console.WindowWidth, Console.WindowHeight - _stepStart - 1);
    }

    public void KeyEvent(ConsoleKeyInfo key, Editor editor)
    {
        if (_help.KeyEvent(key, editor))
        {
            Init(editor);
            PrintUI(editor);
            return;
        }

        switch (key.Key)
        {
            case ConsoleKey.Backspace:
                editor.PopMode();
                _accumulator.Reset();
                break;
            case ConsoleKey.E:
                editor.PushMode(new EditMode(_item));
                _accumulator.Reset();
                break;
            case ConsoleKey.G:
                if (key.Modifiers == ConsoleModifiers.Shift)
                {
                    if (_list.SelectLast())
                    {
                        PrintSteps(editor);
                    }
                }
                else
                {
                    if (_list.SelectFirst())
                    {
                        PrintSteps(editor);
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
                        if (_list.SelectedItem.IsSome(out var item))
                        {
                            if (Editor.MoveStepDown(GetItems(), item))
                            {
                                _list.Update();
                                _list.SelectNext();
                                PrintSteps(editor);
                            }
                        }
                    }
                    else
                    {
                        if (_list.SelectNext())
                        {
                            PrintSteps(editor);
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
                        if (_list.SelectedItem.IsSome(out var item))
                        {
                            if (Editor.MoveStepUp(GetItems(), item))
                            {
                                _list.Update();
                                _list.SelectPrevious();
                                PrintSteps(editor);
                            }
                        }
                    }
                    else
                    {
                        if (_list.SelectPrevious())
                        {
                            PrintSteps(editor);
                        }
                    }
                });
                break;
            case ConsoleKey.Add:
            case ConsoleKey.OemPlus:
                AddStep(editor);
                _accumulator.Reset();
                break;
            case ConsoleKey.Subtract:
            case ConsoleKey.OemMinus:
                DeleteStep(editor);
                _accumulator.Reset();
                break;
            case ConsoleKey.N:
                _accumulator.Execute(() =>
                {
                    if (_list.NextPage())
                    {
                        PrintSteps(editor);
                    }
                });
                break;
            case ConsoleKey.P:
                _accumulator.Execute(() =>
                {
                    if (_list.PreviousPage())
                    {
                        PrintSteps(editor);
                    }
                });
                break;
            case ConsoleKey.T:
                EditStep(editor);
                _accumulator.Reset();
                break;
            case ConsoleKey.V:
                VisualEditStep(editor);
                _accumulator.Reset();
                break;
            case ConsoleKey.Spacebar:
                MarkStep(editor);
                _accumulator.Reset();
                break;
            case ConsoleKey.A:
                ActiveStep(editor);
                _accumulator.Reset();
                break;
            case ConsoleKey.O:
                _accumulator.Reset();
                if (key.Modifiers == ConsoleModifiers.Shift)
                {
                    // reader mode for selected step
                    if (_list.SelectedItem.IsSome(out var item))
                    {
                        editor.PushMode(new ReaderMode(item.Text));
                    }
                }
                else
                {
                    // reader mode for description
                    editor.PushMode(new ReaderMode(_item.Description));
                }

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

    int NextOrder => GetItems().Select(s => s.Order).DefaultIfEmpty(0).Max() + 1;

    void AddStep(Editor editor)
    {
        if (Console.CursorTop > Console.WindowHeight - 3)
        {
            Terminal.ClearBetween(Console.WindowHeight - 3, Console.WindowHeight, Console.WindowWidth);
            Console.SetCursorPosition(0, Console.WindowHeight - 3);
        }

        Console.WriteLine("Step text:");
        Console.CursorVisible = true;
        var maybeText = Input.GetString();
        Console.CursorVisible = false;
        maybeText.Then(text =>
        {
            _item.Steps.Add(new TodoStep { Text = text, Order = NextOrder });
            _list.Update();
            editor.Save();
        });
        Init(editor);
        PrintUI(editor);
    }

    void DeleteStep(Editor editor)
    {
        if (!_list.SelectedItem.IsSome(out var item))
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Are you sure you want to delete this step? (y/n)");
        Console.WriteLine(item.Text);
        Console.CursorVisible = true;
        var maybeDelete = Input.GetString();
        Console.CursorVisible = false;
        maybeDelete.Then(it =>
        {
            if (string.Equals(it, "Y", StringComparison.OrdinalIgnoreCase))
            {
                _item.Steps.Remove(item);
                _list.Update();
                editor.Save();
            }
        });
        Init(editor);
        PrintUI(editor);
    }

    void MarkStep(Editor editor)
    {
        if (!_list.SelectedItem.IsSome(out var item))
        {
            return;
        }

        item.Completed = !item.Completed;
        if (item.Completed)
        {
            item.Active = false; // should never be both active and completed
        }

        //editor.Save();
        PrintSteps(editor);
    }

    void ActiveStep(Editor editor)
    {
        if (!_list.SelectedItem.IsSome(out var item))
        {
            return;
        }

        if (!item.Completed)
        {
            item.Active = !item.Active;
            //editor.Save();
        }

        PrintSteps(editor);
    }

    void EditStep(Editor editor)
    {
        if (!_list.SelectedItem.IsSome(out var item))
        {
            return;
        }

        // Clear steps to focus on the one being edited
        Console.SetCursorPosition(0, _stepStart);
        var sb = new StringBuilder();
        Enumerable
            .Range(0, _item.Steps.Count)
            .ToList()
            .ForEach(_ => sb.AppendLine(new string(' ', Console.WindowWidth - 1)));
        Console.Write(sb);

        Console.SetCursorPosition(0, _stepStart);
        Console.WriteLine("Previous text:");
        Console.WriteLine($" {item.Text}");
        Console.WriteLine("New text:");
        Console.CursorVisible = true;
        var maybeText = Input.GetString();
        Console.CursorVisible = false;
        maybeText.Then(text =>
        {
            item.Text = text;
            editor.Save();
        });
        Init(editor);
        PrintUI(editor);
    }

    void VisualEditStep(Editor editor)
    {
        if (!_list.SelectedItem.IsSome(out var item))
        {
            return;
        }

        var extEditor = new ExternalEditor(item.Text);
        var edit = extEditor.Edit();
        edit.Then(text =>
        {
            item.Text = text;
            editor.Save();
        });
        Init(editor);
        PrintUI(editor);
    }
}