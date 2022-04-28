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
using System.Text;

namespace TodoHD;

public class HelpMode : IMode
{
    private readonly Color _keyColor;
    private readonly Color _textColor;
    private readonly Color _headerColor;

    public HelpMode()
    {
        _headerColor = Settings.Instance.Theme.HelpModeHeader;
        _keyColor = Settings.Instance.Theme.HelpModeKey;
        _textColor = Settings.Instance.Theme.HelpModeText;
    }

    public void Init(Editor editor)
    {
        // nothing to initialize
    }
    public void PrintUI(Editor editor)
    {
        Console.Clear();
        Console.WriteLine(Terminal.Color(_headerColor, "== Keybindings =="));

        var sb = new StringBuilder();
        AppendKey("Enter", "Show item", sb, AppendOptions.AppendLine);
        AppendKey("Backspace", "Back", sb, AppendOptions.AppendLine);
        AppendKey("I", "New item", sb);
        AppendKey("H", "Help", sb);
        AppendKey("Q", "Quit", sb, AppendOptions.AppendLine);
        AppendKey("E", "Edit item", sb);
        AppendKey("D", "Delete item", sb, AppendOptions.AppendLine);
        AppendKey("G", "First item", sb);
        AppendKey("Shift+G", "Last item", sb, AppendOptions.AppendLine);
        AppendKey("G", "First item", sb);
        AppendKey("Shift+G", "Last item", sb, AppendOptions.AppendLine);
        AppendKey("J", "Next item", sb);
        AppendKey("K", "Prev item", sb, AppendOptions.AppendLine);
        AppendKey("Shift+J", "Move item down", sb);
        AppendKey("Shift+K", "Move item up", sb, AppendOptions.AppendLine);
        AppendKey("N", "Next page", sb);
        AppendKey("P", "Prev page", sb, AppendOptions.AppendLine);
        
        Console.WriteLine(sb.ToString());
    }

    private void AppendKey(string key, string text, StringBuilder sb, AppendOptions opts = AppendOptions.Append)
    {
        sb.Append(" " + Terminal.Color(_keyColor, $"[{key}]"));
        if (opts == AppendOptions.AppendLine)
        {
            sb.AppendLine(" " + Terminal.Color(_textColor, text));
        }
        else
        {
            sb.Append(" " + Terminal.Color(_textColor, text));
        }
    }

    public void KeyEvent(ConsoleKeyInfo key, Editor editor)
    {
        switch(key.Key)
        {
            case ConsoleKey.Backspace:
                editor.PopMode();
                break;
        }
    }

    private enum AppendOptions
    {
        Append,
        AppendLine,
    }

}