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

namespace TodoHD
{
    public class ViewMode : IMode
    {
        int Step {get;set;} = 1;
        int _stepStart = 0;
        TodoItem _item;

        void PrintHelpLine()
        {
            Helpers.WithForeground(ConsoleColor.Green, () => {
                    Console.WriteLine($"[+] Add step [-] Remove step [E] Edit [Space] Mark step [Q] Quit");
                    });
        }

        public void Init(Editor editor) {
            Step = 1;
            _item = editor.GetSelectedItem();
            _item.Steps ??= new();
            Console.Clear();
            PrintHelpLine();
        }

        IEnumerable<TodoStep> GetItems() => (_item.Steps ?? new()).OrderBy(i => i.Order);

        TodoStep GetSelectedItem() => 
            GetItems()
                .Select((item,index) => new{item,index})
                .First(it => Step == it.index + 1)
                .item;

        public void PrintUI(Editor editor)
        {
            Console.SetCursorPosition(0, 1);
            Helpers.WithForeground(ConsoleColor.Magenta, () => {
            Console.WriteLine($"== {_item.Title} ==");
            });
            switch(_item.Priority)
            {
                case Priority.Whenever:
                    Console.Write("   ");
                    Helpers.WithBackground(ConsoleColor.Green, () => {
                    Console.WriteLine($"<{_item.Priority}>");
                    });
                    break;
                case Priority.Urgent:
                    Console.Write("   ");
                    Helpers.WithBackground(ConsoleColor.Red, () => {
                    Console.WriteLine($"*{_item.Priority}*");
                    });
                    break;
            }
            _item
                .Description
                .Split(Environment.NewLine)
                .ToList()
                .ForEach(part =>
                    Console.WriteLine($"{new string(' ', 2)}{part}{new string(' ', Console.BufferWidth - 1 - 2 - part.Length)}"));


            Console.WriteLine();
            Helpers.WithForeground(ConsoleColor.Magenta, () => {
            Console.WriteLine($"== Steps ==");
            });

            _stepStart = Console.CursorTop;
            
            PrintSteps(editor);

        }

        void PrintSteps(Editor editor)
        {
            Console.SetCursorPosition(0, _stepStart);
            if(_item.Steps == null)
            {
                return;
            }

            var sb = new StringBuilder();
            GetItems()
                .Select((step,index) => new{step,index})
                .ToList()
                .ForEach(it => {
                    var c = 0;
                    if(Step == it.index + 1) {
                        sb.Append("> ");
                        c += 2;
                    }
                    c += "[x] ".Length + it.step.Text.Length;
                    sb.AppendLine($"[{(it.step.Completed ? 'x' : ' ')}] {it.step.Text}{new string(' ', Console.BufferWidth - 1 - c)}");
                });
            Console.Write(sb);
        }

        public void KeyEvent(ConsoleKeyInfo key, Editor editor)
        {
            switch(key.Key)
            {
                case ConsoleKey.Backspace:
                    editor.PopMode();
                    break;
                case ConsoleKey.E:
                    editor.PushMode(new EditMode());
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.J:
                    if(key.Modifiers == ConsoleModifiers.Shift)
                    {
                        if(_item.Steps.Count > 0)
                        {
                            editor.MoveStepDown(GetItems(), GetSelectedItem());
                            NextStep(editor);
                        }
                    }
                    else
                    {
                        NextStep(editor);
                    }
                    break;
                case ConsoleKey.UpArrow:
                case ConsoleKey.K:
                    if(key.Modifiers == ConsoleModifiers.Shift)
                    {
                        if(_item.Steps.Count > 0)
                        {
                            editor.MoveStepUp(GetItems(), GetSelectedItem());
                            PrevStep(editor);
                        }
                    }
                    else
                    {
                        PrevStep(editor);
                    }
                    break;
                case ConsoleKey.OemPlus:
                    AddStep(editor);
                    break;
                case ConsoleKey.OemMinus:
                    DeleteStep(editor);
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
            var text = Helpers.GetNonEmptyString();
            Console.CursorVisible = false;
            _item.Steps.Add(new(){ Text = text, Order = NextOrder });
            Init(editor);
            PrintUI(editor);
            editor.Save();
        }

        void DeleteStep(Editor editor)
        {
            if(_item.Steps.Count == 0)
            {
                return;
            }
            Console.WriteLine();
            Console.WriteLine("Are you sure you want to delete this step? (y/n)");
            Console.WriteLine(GetSelectedItem().Text);
            Console.CursorVisible = true;
            var delete = Helpers.GetNonEmptyString();
            Console.CursorVisible = false;
            if(delete.ToUpperInvariant() == "Y")
            {
                _item.Steps.Remove(GetSelectedItem());
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
            var selected = GetSelectedItem();
            selected.Completed = !selected.Completed;
            PrintSteps(editor);
            editor.Save();
        }

        void NextStep(Editor editor)
        {
            Step = Math.Clamp(Step + 1, 1, Math.Max(1, _item.Steps.Count));
            PrintSteps(editor);
        }

        void PrevStep(Editor editor)
        {
            Step = Math.Clamp(Step - 1, 1, Math.Max(1, _item.Steps.Count));
            PrintSteps(editor);
        }

        void EditStep(Editor editor)
        {
            // TODO: do some steppin
        }
    }
}
