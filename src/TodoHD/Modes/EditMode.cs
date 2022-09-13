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
using System.Linq;

namespace TodoHD.Modes;

public class EditMode : IMode
{
    private readonly TodoItem _item;
    private bool _changed;
    private string _tmpTitle;
    private string _tmpDescription;
    private Priority _tmpPriority;
    private readonly HelpLine _help;

    public EditMode(TodoItem item)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _help = new HelpLine("[T] Title [R] Description [P] Toggle Priority [S] Save [W] Write & Quit [Q] Quit");
    }

    private void _initUI()
    {
        Console.Clear();
        _help.Print();
    }

    public void Init(Editor editor)
    {
        _tmpTitle = _item.Title;
        _tmpDescription = _item.Description;
        _tmpPriority = _item.Priority;
        _initUI();
    }

    public void PrintUI(Editor editor)
    {
        Console.SetCursorPosition(0, _help.Height);
        if (_changed)
        {
            Console.WriteLine($"<< Editing (changed!) >>");
        }
        else
        {
            Console.WriteLine($"<< Editing >>");
        }

        Console.WriteLine(Settings.Instance.Theme.TodoItemHeader.Apply($"== {_tmpTitle} =="));

        switch (_tmpPriority)
        {
            case Priority.Whenever:
                Console.WriteLine($"   {BackgroundColors.Green.Apply($"<{_tmpPriority}>")}");
                break;
            case Priority.Urgent:
                Console.WriteLine($"   {BackgroundColors.Red.Apply($"*{_tmpPriority}*")}");
                break;
        }

        _tmpDescription
            .ExceptEndingNewlines()
            .ReplaceLineEndings("\n")
            .Split('\n')
            .ToList()
            .ForEach(part =>
                Console.WriteLine(
                    $"{new string(' ', 2)}{part}{new string(' ', Console.WindowWidth - 1 - 2 - part.Length)}"));
        Console.WriteLine(Settings.Instance.Theme.TodoItemHeader.Apply($"== {_tmpTitle} =="));
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
            case ConsoleKey.T:
                SetTitle(editor);
                break;
            case ConsoleKey.R:
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
            case ConsoleKey.V:
                VisualEditDescription(editor);
                break;
            case ConsoleKey.Backspace:
                if (_changed)
                {
                    Output.WithForeground(ConsoleColor.Red,
                        () => Console.WriteLine("Unsaved changes! Quit with Q to discard changes."));
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
        var maybeText = Input.GetString();
        Console.CursorVisible = false;
        maybeText.Then(text =>
        {
            _changed = true;
            _tmpTitle = text;
        });
        _initUI();
        PrintUI(editor);
    }

    void SetDescription(Editor editor)
    {
        Console.WriteLine("New description (<br> for newlines):");
        Console.CursorVisible = true;
        var maybeText = Input.GetString();
        Console.CursorVisible = false;
        maybeText.Then(text =>
        {
            _changed = true;
            _tmpDescription = text.Replace("<br>", Environment.NewLine);
        });
        _initUI();
        PrintUI(editor);
    }

    void VisualEditDescription(Editor editor)
    {
        var extEditor = new ExternalEditor(_tmpDescription);
        var edit = extEditor.Edit();
        edit.Then(text =>
        {
            _changed = true;
            _tmpDescription = text;
        });

        _initUI();
        PrintUI(editor);
    }

    void TogglePriority(Editor editor)
    {
        _tmpPriority = _tmpPriority switch
        {
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