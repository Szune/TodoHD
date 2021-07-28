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
using System.Text;
using SaferVariants;

namespace TodoHD
{
    public class ViewMode : IMode
    {
        int _stepStart = 0;
        TodoItem _item;
        ListBox<TodoStep> _list;
        HelpLine _help;

        public ViewMode(TodoItem item)
        {
            _help = new HelpLine("[+] Add step [-] Remove step [T] Edit step [E] Edit item [Space] Mark step [Q] Quit");
            _item = item;
            _item.Steps ??= new();
            _list = new(GetItems, step => $" [{(step.Completed ? 'x' : ' ')}] {step.Text}")
            {
                OrderBy = Option.Some<Func<IEnumerable<TodoStep>, IEnumerable<TodoStep>>>(it => it.OrderBy(x => x.Order))
            };
        }

        public void Init(Editor editor) {
            Console.Clear();
            _help.Print();
        }

        IEnumerable<TodoStep> GetItems() => (_item.Steps ?? new()).OrderBy(i => i.Order);

        public void PrintUI(Editor editor)
        {
            Console.SetCursorPosition(0, _help.Height);
            Output.WithForeground(ConsoleColor.Magenta, () => {
            Console.WriteLine($"== {_item.Title} ==");
            });
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
            _item
                .Description
                .Split(Environment.NewLine)
                .ToList()
                .ForEach(part =>
                    Output.WriteLineWrapping($"{new string(' ', 2)}{part}"));


            Console.WriteLine();
            Output.WithForeground(ConsoleColor.Magenta, () => {
            Console.WriteLine($"== Steps ==");
            });

            _stepStart = Console.CursorTop;
            
            PrintSteps(editor);
        }

        void PrintSteps(Editor editor)
        {
            Console.SetCursorPosition(0, _stepStart);
            _list.Print();
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
                    break;
                case ConsoleKey.E:
                    editor.PushMode(new EditMode());
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
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.J:
                    if(key.Modifiers == ConsoleModifiers.Shift)
                    {
                        if(_item.Steps.Count > 0)
                        {
                            if(editor.MoveStepDown(GetItems(), _list.SelectedItem))
                            {
                                _list.Update();
                                _list.SelectNext();
                                PrintSteps(editor);
                            }
                        }
                    }
                    else
                    {
                        if(_list.SelectNext())
                        {
                            PrintSteps(editor);
                        }
                    }
                    break;
                case ConsoleKey.UpArrow:
                case ConsoleKey.K:
                    if(key.Modifiers == ConsoleModifiers.Shift)
                    {
                        if(_item.Steps.Count > 0)
                        {
                            if(editor.MoveStepUp(GetItems(), _list.SelectedItem))
                            {
                                _list.Update();
                                _list.SelectPrevious();
                                PrintSteps(editor);
                            }
                        }
                    }
                    else
                    {
                        if(_list.SelectPrevious())
                        {
                            PrintSteps(editor);
                        }
                    }
                    break;
                case ConsoleKey.OemPlus:
                    AddStep(editor);
                    break;
                case ConsoleKey.OemMinus:
                    DeleteStep(editor);
                    break;
                case ConsoleKey.T:
                    EditStep(editor);
                    break;
                case ConsoleKey.Spacebar:
                    MarkStep(editor);
                    break;
            }
        }

        int NextOrder => GetItems().Select(s => s.Order).DefaultIfEmpty(0).Max() + 1;

        void AddStep(Editor editor)
        {
            Console.WriteLine("Step text:");
            Console.CursorVisible = true;
            var maybeText = Input.GetString();
            maybeText.Then(text =>
            {
                _item.Steps.Add(new TodoStep{ Text = text, Order = NextOrder });
                _list.Update();
                editor.Save();
            });
            Console.CursorVisible = false;
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
            var delete = Input.GetNonEmptyString();
            Console.CursorVisible = false;
            if(delete.ToUpperInvariant() == "Y")
            {
                _item.Steps.Remove(_list.SelectedItem);
                _list.Update();
                editor.Save();
            }

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
            PrintSteps(editor);
            editor.Save();
        }

        void EditStep(Editor editor)
        {
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
            var text = Input.GetNonEmptyString();
            Console.CursorVisible = false;
            item.Text = text;

            editor.Save();
            Init(editor);
            PrintUI(editor);
        }
    }
}
