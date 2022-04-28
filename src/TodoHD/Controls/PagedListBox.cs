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

namespace TodoHD.Controls;

public class PagedListBox<T>
{
    private int _page;
    private int _maxPage;
    private List<RenderedPage> _pages = new();

    private readonly Func<IEnumerable<T>> _itemsFactory;
    public List<T> Items { get; private set; }
    private readonly Func<T,string> _formatter;
    private readonly Func<T, string, bool, string> _colorFunc;

    private int _selected;
    private int _lastLineAmount;

    public T SelectedItem => Items[_selected];

    public IOption<Func<IEnumerable<T>, IEnumerable<T>>> OrderBy {get; set;} =
        Option.None<Func<IEnumerable<T>, IEnumerable<T>>>();

    public bool HidePageNumberIfSinglePage { get; set; }

    public PagedListBox(Func<IEnumerable<T>> itemsFactory, Func<T,string> formatter, Func<T, string, bool, string> colorFunc)
    {
        _itemsFactory = itemsFactory;
        _formatter = formatter;
        _colorFunc = colorFunc;
        Update();
    }

    public void Update()
    {
        var items = _itemsFactory();
        if(OrderBy.IsSome(out var ordering))
        {
            items = ordering(items);
        }
        Items = items.ToList();
        // fix selection after removal
        _selected = Math.Clamp(_selected, 0, Math.Max(0, Items.Count - 1));
    }

    private List<RenderedPage> PrerenderPages(int availableWidth, int availableHeight)
    {
        using var pb = new PageBuilder(availableWidth, availableHeight);

        for(var i = 0; i < Items.Count; i++)
        {
            RenderItem(availableWidth, availableHeight, i, pb);
        }
        return pb.Build();
    }

    private void RenderItem(int availableWidth, int availableHeight, int index, PageBuilder pb)
    {
        var formatted = _formatter(Items[index]);
        var measured = Output.Measure(FormatSelected(formatted), availableWidth);
        var isSelected = _selected == index;

        var sb = new StringBuilder();

        // add "selected" format
        if (isSelected)
        {
            _ = Output.WriteLineWrapping(
                sb,
                FormatSelected(formatted),
                availableWidth);
        }
        else
        {
            _ = Output.WriteLineWrapping(sb, formatted, availableWidth);
        }

        // update formatted string
        var rendered = sb.ToString();

        // TODO: this abstraction is a bit weird, should rethink it at some point
        // as it is now, layout logic has been split between both this method and PageBuilder

        // collapse to fit on page if needed
        var linesLeft = pb.GetLinesLeftOnPage();
        var renderedHeight = measured.MaxHeight;
        if (linesLeft > 0 &&
            linesLeft < measured.MaxHeight)
        {
            renderedHeight = measured.MaxHeight - linesLeft;
            rendered = CollapseItem(rendered, renderedHeight);
        }
        else if (linesLeft == 0 && availableHeight < measured.MaxHeight)
        {
            renderedHeight = availableHeight;
            rendered = CollapseItem(rendered, renderedHeight);
        }

        // add color format
        rendered = _colorFunc(Items[index], rendered, isSelected);

        pb.AddItem(measured.MaxWidth, measured.MaxHeight, renderedHeight, formatted, rendered);
    }

    private static string CollapseItem(string item, int collapseHeight)
    {
        var formatted =
            string.Join(
                Environment.NewLine,
                item
                    .ReplaceLineEndings("\n")
                    .Split('\n')
                    .Take(collapseHeight));
        if (formatted.Length >= 6)
            formatted = formatted[..^6];
        // int placeToReplace = -1;
        // var last = ' ';
        // for (var i = 0; i < formatted.Length; ++i)
        // {
        //     if (last != ' ' && formatted[i] == ' ')
        //     {
        //         placeToReplace = i;
        //     }
        //     last = formatted[i];
        // }
        return formatted +
               (formatted.EndsWith(" ") ? "" : " ") +
               "[...]" +
               Environment.NewLine;
    }

    public void Print(int availableWidth, int availableHeight)
    {
        if(Items == null || Items.Count < 1)
        {
            return;
        }

        // TODO: don't re-render pages _every time_ you print, only when something has changed
        // e.g. selection changed, item was added/removed etc
        _pages = PrerenderPages(availableWidth, availableHeight - 1); // keeping last line for page info
        if (_pages.Count < 1)
        {
            return;
        }
        _maxPage = _pages.Count - 1;

        var page = _pages[_page];
        var lines = page.TotalHeight;
        var sb = new StringBuilder()
            .AppendJoin(
                "",
                page.Lines.Select(it => it.RenderedText));

        if (!HidePageNumberIfSinglePage || _maxPage > 0)
        {
            var pageDisplay = $"Page {_page+1}/{_maxPage+1}";
            var clearOut = availableWidth - pageDisplay.Length;
            sb.Append(ColorGreen(pageDisplay));
            sb.Append(new string(' ', clearOut));
            ++lines;
        }

        /*
        // if fewer lines on this render than previous one
        if (_lastLineAmount > lines)
        {
            // then write enough lines to clear out the missing lines as well
            for(var aa = 0; aa < _lastLineAmount - lines; aa++)
                sb.AppendLine(new string(' ', width));
        }
        _lastLineAmount = lines;
        */
        if (lines < availableHeight)
        {
            // then write enough lines to clear out the missing lines as well
            for(var aa = lines; aa < availableHeight; aa++)
                sb.AppendLine(new string(' ', availableWidth));
        }
        Console.Write(sb);
    }

    // separated from coloring to make it easier to render visible parts of lines
    private static string FormatSelected(string selectedLine) => $" >{selectedLine}";

    private static string ColorGreen(string selectedLine) => 
        Terminal.Foreground(ForegroundColors.DarkGreen, selectedLine);

    public bool NextPage()
    {
        var change = _page != (_page = Math.Clamp(_page + 1, 0, Math.Max(0, _pages.Count - 1)));
        if(change)
        {
            var toSelect = _pages.Take(_page).Select(it => it.Lines.Count).Sum();
            _selected = toSelect;
        }
        return change;
    }

    public bool PreviousPage()
    {
        var change = _page != (_page = Math.Clamp(_page - 1, 0, Math.Max(0, _pages.Count - 1)));
        if(change)
        {
            var toSelect = _pages.Take(_page).Select(it => it.Lines.Count).Sum();
            _selected = toSelect;
        }
        return change;
    }

    // TODO: rethink the selection code, feels like there is a much better way to do all this
    public bool SelectPrevious()
    {
        var changed = _selected != (_selected = Math.Clamp(_selected - 1, 0, Math.Max(0, Items.Count - 1)));

        if(changed)
        {
            var accumulatedItems = 0;
            for(var i = 0; i < _pages.Count; i++)
            {
                accumulatedItems += _pages[i].Lines.Count - 1;
                if(accumulatedItems >= _selected)
                {
                    _page = i;
                    break;
                }
            }
        }
        return changed;
    }

    // TODO: rethink the selection code, feels like there is a much better way to do all this
    public bool SelectNext()
    {
        // TODO: can't move to next page using j-k if there is only one item on the next page
        
        var changed = _selected != (_selected = Math.Clamp(_selected + 1, 0, Math.Max(0, Items.Count - 1)));
        
        if(changed)
        {
            var accumulatedItems = 0;
            for(var i = 0; i < _pages.Count; i++)
            {
                accumulatedItems += _pages[i].Lines.Count - 1;
                if(accumulatedItems >= _selected)
                {
                    _page = i;
                    break;
                }
            }
        }
        return changed;
    }

    public bool SelectFirst()
    {
        if(_pages.Count > 0)
        {
            var toSelect = _pages.Take(_page).Select(it => it.Lines.Count).Sum();
            return _selected != (_selected = toSelect);
        }
        else
        {
            _selected = 0;
            return false;
        }
    }

    public bool SelectLast()
    {
        if(_pages.Count > 0)
        {
            var toSelect = 
                Math.Max(
                    0,
                    _pages.Take(_page + 1).Select(it => it.Lines.Count).Sum() - 1);
            return _selected != (_selected = toSelect);
        }
        else
        {
            _selected = Items.Count;
            return false;
        }
    }
}
