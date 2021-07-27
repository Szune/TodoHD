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

namespace TodoHD
{
    public class EditMode : IMode
    {
        TodoItem _item;
        bool _changed = false;
        string _tmpTitle;
        string _tmpDescription;
        Priority _tmpPriority;
        HelpLine _help;

        public EditMode()
        {
            _help = new HelpLine("[T] Title [D] Description [P] Toggle Priority [S] Save [W] Write & Quit [Q] Quit");
        }

        void _initUI()
        {
            Console.Clear();
            _help.Print();
        }

        public void Init(Editor editor)
        { 
            _item = editor.GetSelectedItem();
            _tmpTitle = _item.Title;
            _tmpDescription = _item.Description;
            _tmpPriority = _item.Priority;
            _initUI();
        }

        public void PrintUI(Editor editor)
        {
            Console.SetCursorPosition(0, _help.Height);
            if(_changed)
            {
                Console.WriteLine($"<< Editing (changed!) >>");
            }
            else
            {
                Console.WriteLine($"<< Editing >>");
            }
            Output.WithForeground(ConsoleColor.Magenta, () => {
            Console.WriteLine($"== {_tmpTitle} ==");
            });

            switch(_tmpPriority)
            {
                case Priority.Whenever:
                    Console.Write("   ");
                    Output.WithBackground(ConsoleColor.Green, () => {
                    Console.WriteLine($"<{_tmpPriority}>");
                    });
                    break;
                case Priority.Urgent:
                    Console.Write("   ");
                    Output.WithBackground(ConsoleColor.Red, () => {
                    Console.WriteLine($"*{_tmpPriority}*");
                    });
                    break;
            }
            _tmpDescription
                .Split(Environment.NewLine)
                .ToList()
                .ForEach(part =>
                    Console.WriteLine($"{new string(' ', 2)}{part}{new string(' ', Console.BufferWidth - 1 - 2 - part.Length)}"));
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
                case ConsoleKey.T:
                    SetTitle(editor);
                    break;
                case ConsoleKey.D:
                    SetDescription(editor);
                    break;
                case ConsoleKey.P:
                    TogglePriority(editor);
                    break;
                case ConsoleKey.W:
                    SaveAndQuit(editor);
                    break;
                case ConsoleKey.S:
                    Save(editor);
                    break;
                case ConsoleKey.Backspace:
                    if(_changed)
                    {
                        Output.WithForeground(ConsoleColor.Red, () => Console.WriteLine("Unsaved changes! Quit with Q to discard changes."));
                    }
                    else
                    {
                        editor.PopMode();
                    }
                    break;
            }
        }

        void SetTitle(Editor editor)
        {
            Console.WriteLine("New title:");
            Console.CursorVisible = true;
            var text = Input.GetNonEmptyString();
            Console.CursorVisible = false;
            _changed = true;
            _tmpTitle = text;
            _initUI();
            PrintUI(editor);
        }

        void SetDescription(Editor editor)
        {
            Console.WriteLine("New description (<br> for newlines):");
            Console.CursorVisible = true;
            var text = Input.GetNonEmptyString().Replace("<br>", Environment.NewLine);
            Console.CursorVisible = false;
            _changed = true;
            _tmpDescription = text;
            _initUI();
            PrintUI(editor);
        }
        
        void TogglePriority(Editor editor)
        {
            _tmpPriority = _tmpPriority switch {
                Priority.Whenever => Priority.Urgent,
                Priority.Urgent => Priority.Whenever,
                _ => Priority.Whenever,
            };
            _changed = true;
            _initUI();
            PrintUI(editor);
        }

        void Save(Editor editor)
        {
            _item.Title = _tmpTitle;
            _item.Description = _tmpDescription;
            _item.Priority = _tmpPriority;
            editor.Save();
            _changed = false;
            _initUI();
            PrintUI(editor);
        }

        void SaveAndQuit(Editor editor)
        {
            _item.Title = _tmpTitle;
            _item.Description = _tmpDescription;
            _item.Priority = _tmpPriority;
            editor.Save();
            editor.PopMode();
        }
    }
}
