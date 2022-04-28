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

using TodoHD.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SaferVariants;

namespace TodoHD;

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

    public ViewMode(TodoItem item) {
        // active items as well as selected items
        _help = new HelpLine(text: "[+] Add step [-] Remove step [T] Edit step [E] Edit item [Space] Mark step [A] Active step [Q] Quit");
        _item = item;
        _item.Steps ??= new();
        _list = new PagedListBox<TodoStep>(itemsFactory: GetItems,
            formatter: step => $" [{(step.Completed ? 'x' : step.Active ? 'o' : ' ')}] {step.Text}",
            colorFunc: (step, formattedString, selected) =>
            {
                if (selected)
                {
                    if (step.Active && !step.Completed)
                    {
                        return Terminal.Color(color: Settings.Instance.Theme.StepActiveSelected, text: formattedString);
                        // return Terminal.Background(BackgroundColors.Blue,
                        //     Terminal.Foreground(ForegroundColors.White, formattedString));
                        // return Terminal.Background(BackgroundColors.DarkGray,
                        //     Terminal.Foreground(ForegroundColors.Cyan, formattedString));
                    }
                    else if (step.Completed)
                    {
                        return Terminal.Color(color: Settings.Instance.Theme.StepCompletedSelected, text: formattedString);
                    }
                    else
                    {
                        return Terminal.Color(color: Settings.Instance.Theme.StepSelected, text: formattedString);
                        // return Terminal.Foreground(ForegroundColors.Cyan, formattedString);
                    }
                }
                if (step.Completed)
                {
                    return Terminal.Color(color: Settings.Instance.Theme.StepCompleted, text: formattedString);
                    // return Terminal.Foreground(ForegroundColors.DarkGray, formattedString);
                }
                if (step.Active)
                {
                    return Terminal.Color(color: Settings.Instance.Theme.StepActive, text: formattedString);
                    // return Terminal.Background(BackgroundColors.DarkGray,
                    //     Terminal.Foreground(ForegroundColors.Blue, formattedString));
                }
                return Terminal.Color(color: Settings.Instance.Theme.Step, text: formattedString);
            })
        {
            HidePageNumberIfSinglePage = true,
            OrderBy = Option.Some<Func<IEnumerable<TodoStep>, IEnumerable<TodoStep>>>(value: it => it.OrderBy(keySelector: x => x.Order))
        };
    }

    public void Init(Editor editor) {
        Console.Clear();
        _help.Print();
    }

    IEnumerable<TodoStep> GetItems() => (_item.Steps ?? new()).OrderBy(i => i.Order);

    public void PrintUI(Editor editor) {
        Console.SetCursorPosition(0, _help.Height);
        Console.WriteLine(Terminal.Color(Settings.Instance.Theme.TodoItemHeader, $"== {_item.Title} =="));
        switch(_item.Priority)
        {
            case Priority.Whenever:
                Console.Write("   ");
                Output.WithBackground(ConsoleColor.Green, () => {
                    Console.WriteLine($"<{_item.Priority}>");
                });
                break;
            case Priority.Urgent:
                Console.Write("   ");
                Output.WithBackground(ConsoleColor.Red, () => {
                    Console.WriteLine($"*{_item.Priority}*");
                });
                break;
        }
        
        _item.Description
            .ExceptEndingNewline()
            .ReplaceLineEndings("\n")
            .Split('\n')
            .ToList()
            .ForEach(part =>
                Output.WriteLineWrapping($"{new string(' ', 2)}{part}"));

        Console.WriteLine();
        Console.WriteLine(Terminal.Color(Settings.Instance.Theme.TodoItemHeader, "== Steps =="));

        _stepStart = Console.CursorTop;
        
        PrintSteps(editor);
    }

    void PrintSteps(Editor editor)
    {
        Console.SetCursorPosition(0, _stepStart);
        _list.Print(Console.BufferWidth, Console.BufferHeight - _stepStart - 1);
    }

    public void KeyEvent(ConsoleKeyInfo key, Editor editor)
    {
        if (_help.KeyEvent(key, editor))
        {
            Init(editor);
            PrintUI(editor);
            return;
        }
        switch(key.Key)
        {
            case ConsoleKey.Backspace:
                editor.PopMode();
                _accumulator.Reset();
                break;
            case ConsoleKey.E:
                editor.PushMode(new EditMode());
                _accumulator.Reset();
                break;
            case ConsoleKey.G:
                if (key.Modifiers == ConsoleModifiers.Shift)
                {
                    if(_list.SelectLast())
                    {
                        PrintSteps(editor);
                    }
                }
                else
                {
                    if(_list.SelectFirst())
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
                        if (_item.Steps.Count > 0)
                        {
                            if (Editor.MoveStepDown(GetItems(), _list.SelectedItem))
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
                        if (_item.Steps.Count > 0)
                        {
                            if (Editor.MoveStepUp(GetItems(), _list.SelectedItem))
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
            case ConsoleKey.OemPlus:
                AddStep(editor);
                _accumulator.Reset();
                break;
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
        Console.WriteLine("Step text:");
        Console.CursorVisible = true;
        var maybeText = Input.GetString();
        Console.CursorVisible = false;
        maybeText.Then(text =>
        {
            _item.Steps.Add(new TodoStep{ Text = text, Order = NextOrder });
            _list.Update();
            editor.Save();
        });
        Init(editor);
        PrintUI(editor);
    }

    void DeleteStep(Editor editor)
    {
        if(_item.Steps.Count == 0)
        {
            return;
        }
        Console.WriteLine();
        Console.WriteLine("Are you sure you want to delete this step? (y/n)");
        Console.WriteLine(_list.SelectedItem.Text);
        Console.CursorVisible = true;
        var maybeDelete = Input.GetString();
        Console.CursorVisible = false;
        maybeDelete.Then(it =>
        {
            if (string.Equals(it, "Y", StringComparison.OrdinalIgnoreCase))
            {
                _item.Steps.Remove(_list.SelectedItem);
                _list.Update();
                editor.Save();
            }
        });
        Init(editor);
        PrintUI(editor);
    }

    void MarkStep(Editor editor)
    {
        if(_item.Steps.Count == 0)
        {
            return;
        }
        var selected = _list.SelectedItem;
        selected.Completed = !selected.Completed;
        if (selected.Completed)
        {
            selected.Active = false; // should never be both active and completed
        }
        //editor.Save();
        PrintSteps(editor);
    }

    void ActiveStep(Editor editor)
    {
        if(_item.Steps.Count == 0)
        {
            return;
        }
        var selected = _list.SelectedItem;
        if(!selected.Completed)
        {
            selected.Active = !selected.Active;
            //editor.Save();
        }
        PrintSteps(editor);
    }

    void EditStep(Editor editor)
    {
        if (_list.Items.Count < 1)
        {
            return;
        }
        // Clear steps to focus on the one being edited
        Console.SetCursorPosition(0, _stepStart);
        var sb = new StringBuilder();
        Enumerable
            .Range(0, _item.Steps.Count)
            .ToList()
            .ForEach(_ => sb.AppendLine(new string(' ', Console.BufferWidth - 1)));
        Console.Write(sb);

        var item = _list.SelectedItem;
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
        if (_list.Items.Count < 1)
        {
            return;
        }
        var item = _list.SelectedItem;
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

