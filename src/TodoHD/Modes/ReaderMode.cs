using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TodoHD.Rendering;

namespace TodoHD.Modes;

public class ReaderMode : IMode
{
    private readonly Accumulator _accumulator = new Accumulator(100);
    private readonly string _text;
    private int _scroll;
    private int _renderedScroll;
    private (int Width, int Height) _windowMeasurement;
    private List<string> _wrappedText;

    public ReaderMode(string text)
    {
        _text = text;
        _windowMeasurement = (0, 0);
        _wrappedText = new List<string>();
    }

    public void Init(Editor editor)
    {
        _windowMeasurement = (0, 0);
    }

    public void PrintUI(Editor editor)
    {
        Logger.LogTrace("Entered ReaderMode.PrintUI");
        var windowSize = Terminal.GetWindowSize();
        if (string.IsNullOrWhiteSpace(_text))
        {
            Logger.LogTrace("_text was null");
            _windowMeasurement = windowSize;
            // just clear everything and draw the header
            Console.Clear();
            DrawHeader();
            return;
        }


        if (windowSize.Width == _windowMeasurement.Width &&
            windowSize.Height == _windowMeasurement.Height &&
            _scroll == _renderedScroll)
        {
            Logger.LogTrace(
                $"Did not render, nothing changed (_scroll: {_scroll}, _windowMeasurement: ({_windowMeasurement.Width}, {_windowMeasurement.Height}))");
            return;
        }

        if (windowSize.Width != _windowMeasurement.Width ||
            windowSize.Height != _windowMeasurement.Height)
        {
            Logger.LogTrace("Window size changed, rendering");
            // redraw the whole
            FullRedraw(windowSize);
        }
        else if (_scroll != _renderedScroll)
        {
            Logger.LogTrace("Scroll position changed, rendering");
            ScrollRedraw(windowSize);
        }
    }

    private void DrawHeader()
    {
        Logger.LogTrace("Entered DrawHeader");
        Console.SetCursorPosition(0, 0);
        var header = new Span
        {
            Settings.Instance.Theme.HelpModeHeader.Span("== Reader =="),
            _wrappedText.Count > 0 ? $"(Line {_scroll + 1}/{_wrappedText.Count})" : "(Line 0/0)",
            Settings.Instance.Theme.HelpModeKey.Span("[Q]"),
            Settings.Instance.Theme.HelpModeText.Span("Exit"),
        };
        var measure = header.Measure(" ");
        var write = header.ToString(" ");
        // write new header + clear out previous text with blank spaces
        Console.WriteLine($"{write}{new string(' ', Math.Max(0, _windowMeasurement.Width - measure))}");
    }

    private void ScrollRedraw((int Width, int Height) windowSize)
    {
        Logger.LogTrace("Entered ScrollRedraw");
        _windowMeasurement = windowSize;
        DrawHeader();

        var availableLines = Math.Max(1, _windowMeasurement.Height - 1);
        var renderedLines = Math.Clamp(_wrappedText.Count - _scroll, 1, availableLines);
        var linesToClear = Math.Max(0, _windowMeasurement.Height - renderedLines - 1); // 1 so we don't hide header line

        Logger.LogTrace(
            $"Available lines: {availableLines} Rendered lines: {renderedLines} Lines to clear: {linesToClear}");

        _renderedScroll = _scroll;
        var writeStr =
            _wrappedText
                .Skip(_scroll)
                .Take(renderedLines)
                .Concat(Enumerable.Range(0, linesToClear)
                    .Select(_ => new string(' ', _windowMeasurement.Width)))
                .Aggregate(new StringBuilder(), (sb, el) => sb.AppendLine(el));

        if (writeStr.Length > 0)
        {
            Console.Write(writeStr.ExceptEndingNewline().ToString());
        }
    }

    private void FullRedraw((int Width, int Height) windowSize)
    {
        Logger.LogTrace("Entered FullRedraw");
        _windowMeasurement = windowSize;
        var wrappedLines = Output.WriteLineWrappingList(_text, _windowMeasurement.Width);
        Logger.LogTrace($"FullRedraw, {wrappedLines.LineCount} lines");
        _wrappedText = wrappedLines.Lines;
        _renderedScroll = _scroll = 0;
        DrawHeader();

        var availableLines = Math.Max(1, _windowMeasurement.Height - 1);
        var renderedLines = Math.Clamp(_wrappedText.Count, 1, availableLines);
        var linesToClear = Math.Max(0, _windowMeasurement.Height - renderedLines - 1); // 1 so we don't hide header line
        Logger.LogTrace(
            $"Available lines: {availableLines} Rendered lines: {renderedLines} Lines to clear: {linesToClear}");

        var writeStr =
            _wrappedText
                .Skip(_scroll)
                .Take(renderedLines)
                .Concat(Enumerable.Range(0, linesToClear)
                    .Select(_ => new string(' ', _windowMeasurement.Width)))
                .Aggregate(new StringBuilder(), (sb, el) => sb.AppendLine(el));

        if (writeStr.Length > 0)
        {
            Console.Write(writeStr.ExceptEndingNewline().ToString());
        }
    }

    public void KeyEvent(ConsoleKeyInfo key, Editor editor)
    {
        Logger.LogTrace("Entered ReaderMode.KeyEvent");
        switch (key.Key)
        {
            case ConsoleKey.Backspace:
                editor.PopMode();
                break;
            case ConsoleKey.DownArrow:
            case ConsoleKey.J:
                _accumulator.Execute(() =>
                {
                    _scroll = Math.Clamp(_scroll + 1, 0, Math.Max(_wrappedText.Count - 1, 0));
                });
                PrintUI(editor);
                break;
            case ConsoleKey.UpArrow:
            case ConsoleKey.K:
                _accumulator.Execute(() =>
                {
                    _scroll = Math.Clamp(_scroll - 1, 0, Math.Max(_wrappedText.Count - 1, 0));
                });
                PrintUI(editor);
                break;
            case ConsoleKey.G:
                bool changed;
                if (key.Modifiers == ConsoleModifiers.Shift)
                {
                    // go last
                    changed = _scroll != (_scroll = Math.Max(_wrappedText.Count - 1, 0));
                }
                else
                {
                    // go first
                    changed = _scroll != (_scroll = 0);
                }

                if (changed)
                {
                    PrintUI(editor);
                }

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
                PrintUI(editor);
                break;
        }
    }
}