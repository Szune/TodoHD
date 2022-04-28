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

namespace TodoHD;

public class ListBox<T>
{
    private Func<IEnumerable<T>> _itemsFactory;
    private List<T> _items;
    private readonly Func<T,string> _formatter;

    private int _selected;
    private int _lastLineAmount;

    public T SelectedItem => _items[_selected];

    public IOption<Func<IEnumerable<T>, IEnumerable<T>>> OrderBy {get; set;} =
        Option.None<Func<IEnumerable<T>, IEnumerable<T>>>();

    public ListBox(Func<IEnumerable<T>> itemsFactory, Func<T,string> formatter)
    {
        _itemsFactory = itemsFactory;
        _formatter = formatter;
        Update();
    }

    public void Update()
    {
        var items = _itemsFactory();
        if(OrderBy.IsSome(out var ordering))
        {
            items = ordering(items);
        }
        _items = items.ToList();
        // fix selection after removal
        _selected = Math.Clamp(_selected, 0, Math.Max(0, _items.Count - 1));
    }

    public void Print()
    {
        if(_items == null || _items.Count < 1)
        {
            return;
        }
        
        var bufferWidth = Console.BufferWidth;
        var sb = new StringBuilder();
        var lines = 0;
        
        for(var i = 0; i < _items.Count; i++)
        {
            var formatted = _formatter(_items[i]);
            var isSelected = _selected == i;
            var adding = new StringBuilder();
            if(isSelected)
            {
                lines += Output.WriteLineWrapping(adding,
                        FormatSelected(formatted),
                        bufferWidth);
                sb.Append(ColorSelected(adding.ToString()));
            }
            else
            {
                lines += Output.WriteLineWrapping(sb, formatted, bufferWidth);
            }
        }

        if (_lastLineAmount > lines)
        {
            for(var aa = 0; aa < _lastLineAmount - lines; aa++)
                sb.AppendLine(new string(' ', bufferWidth));
        }
        _lastLineAmount = lines;
        Console.Write(sb);
    }

    private static string FormatSelected(string selectedLine) => $" >{selectedLine}";

    private static string ColorSelected(string selectedLine) => 
        $"\x1b[1;36m{selectedLine}\x1b[0m";

    public bool SelectPrevious()
    {
        return _selected != (_selected = Math.Clamp(_selected - 1, 0, Math.Max(0, _items.Count - 1)));
    }

    public bool SelectNext()
    {
        return _selected != (_selected = Math.Clamp(_selected + 1, 0, Math.Max(0, _items.Count - 1)));
    }
    
    public bool SelectFirst()
    {
        return _selected != (_selected = 0);
    }
    
    public bool SelectLast()
    {
        return _selected != (_selected = Math.Max(0, _items.Count - 1));
    }
}

