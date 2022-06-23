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

public class HelpLine
{
    private const string SHORT_EXPAND_TEXT = "[Z] Expand";
    private const string EXPAND_TEXT = "[Z] Expand help line";
    private string _text;
    private bool _expanded;

    private int _height;
    public int Height => _height;

    public HelpLine(string text)
    {
        _text = text;
    }

    public void UpdateHelp(string newText)
    {
        _height = 0;
        _text = newText;
    }

    public void Print()
    {
        if (_text.Length > Console.WindowWidth && !_expanded)
        {
            PrintExpandHelp();
        }
        else if (_text.Length > Console.WindowWidth && _expanded)
        {
            var sb = new StringBuilder();
            Output.WriteLineWrapping(sb, _text, out _height);
            Console.Write(Settings.Instance.Theme.HelpLine.Apply(sb.ToString()));
            _height -= 1;
            // Output.WithForeground(ConsoleColor.Green, () => Output.WriteLineWrapping(_text, out _height));
            // _height -= 1;
        }
        else
        {
            _height = 1;
            Console.WriteLine(Settings.Instance.Theme.HelpLine.Apply(_text));
            //Output.WithForeground(ConsoleColor.Green, () => Console.WriteLine(_text));
        }
    }

    private void PrintExpandHelp()
    {
        if (EXPAND_TEXT.Length > Console.WindowWidth)
        {
            var sb = new StringBuilder();
            Output.WriteLineWrapping(sb, SHORT_EXPAND_TEXT, out _height);
            Console.Write(Settings.Instance.Theme.HelpLine.Apply(sb.ToString()));
            // Output.WithForeground(ConsoleColor.Green, () => Output.WriteLineWrapping(SHORT_EXPAND_TEXT, out _height));
        }
        else
        {
            var sb = new StringBuilder();
            Output.WriteLineWrapping(sb, EXPAND_TEXT, out _height);
            Console.Write(Settings.Instance.Theme.HelpLine.Apply(sb.ToString()));
            // Output.WithForeground(ConsoleColor.Green, () => Output.WriteLineWrapping(EXPAND_TEXT, out _height));
        }
    }

    public bool KeyEvent(ConsoleKeyInfo key, Editor editor)
    {
        switch (key.Key)
        {
            case ConsoleKey.Z:
                _expanded = !_expanded;
                return true;
            default:
                return false;
        }
    }
}