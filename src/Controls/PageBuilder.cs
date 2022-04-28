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

namespace TodoHD.Controls;

public class PageBuilder : IDisposable
{
    private readonly List<RenderedPage> _pages = new();
    private readonly int _width;
    private readonly int _height;

    private List<RenderedLine> _currentPage = new();
    private int _currentPageHeight;
    private bool _disposed;

    public PageBuilder(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public int GetLinesLeftOnPage()
    {
        return _height - _currentPageHeight;
    }

    public void AddItem(int maxWidth, int maxHeight, int renderedHeight, string rawText, string renderedText)
    {
        // TODO: rewrite this huge mess of if statements
        if(_currentPageHeight + renderedHeight > _height)
        {
            if(_currentPageHeight > 0) // if page is not empty
            {
                // the item won't fit on current page, so create a new page
                AddPage();
                // add to new page
                _currentPage.Add(new RenderedLine(maxWidth, maxHeight, renderedHeight, rawText, renderedText));
                _currentPageHeight += renderedHeight;
                // start a new page since current is already filled
                AddPage();
            }
            else
            {
                // add item to current empty page
                _currentPage.Add(new RenderedLine(maxWidth, maxHeight, renderedHeight, rawText, renderedText));
                _currentPageHeight += renderedHeight;
                // start a new page since current is already filled
                AddPage();
            }
        }
        else
        {
            if(_currentPageHeight + renderedHeight == _height)
            {
                // fits perfectly, add item to page and start a new page
                _currentPage.Add(new RenderedLine(maxWidth, maxHeight, renderedHeight, rawText, renderedText));
                _currentPageHeight += renderedHeight;
                AddPage();
            }
            else
            {
                _currentPage.Add(new RenderedLine(maxWidth, maxHeight, renderedHeight, rawText, renderedText));
                _currentPageHeight += renderedHeight;
            }
        }
    }

    /// <summary>
    /// Builds the pages and renders the PageBuilder unusable.
    /// </summary>
    public List<RenderedPage> Build()
    {
        if(_disposed)
        {
            throw new InvalidOperationException("Page has already been built.");
        }
        _disposed = true;
        if(_currentPage.Count > 0)
        {
            AddPage();
        }
        return _pages;
    }

    private void AddPage()
    {
        // build page
        _pages.Add(
            new RenderedPage(
                _currentPage,
                _currentPage.Sum(it => it.RenderedHeight),
                _width,
                _height));
        // reset current page and accumulated height
        _currentPage = new();
        _currentPageHeight = 0;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _currentPage = null;
        }
        GC.SuppressFinalize(this);
    }
}
/// <summary>
/// A rendered line in a list box.
/// </summary>
/// <param name="MaxWidth">The potential maximum width if formatting is applied</param>
/// <param name="MaxHeight">The potential maximum height if formatting and wrapping is applied</param>
/// <param name="RenderedHeight">The height with formatting applied</param>
/// <param name="RawText">The line without any formatting applied</param>
/// <param name="RenderedText">The line with formatting applied (wrapping, state, color)</param>
public record RenderedLine(int MaxWidth, int MaxHeight, int RenderedHeight, string RawText, string RenderedText);


/// <summary>
/// A rendered page in a list box.
/// </summary>
/// <param name="Lines">All the rendered lines contained in the page</param>
/// <param name="TotalHeight">The total height of all the rendered lines</param>
/// <param name="RenderWidth">The width when the rendering was performed</param>
/// <param name="RenderHeight">The height when the rendering was performed</param>
public record RenderedPage(List<RenderedLine> Lines, int TotalHeight, int RenderWidth, int RenderHeight);

