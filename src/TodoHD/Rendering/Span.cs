using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SaferVariants;

namespace TodoHD.Rendering;

public class Span : IEnumerable<SpanSegment>
{
    private readonly List<SpanSegment> _segments = new List<SpanSegment>();

    public Span()
    {
    }

    public Span(SpanSegment initial)
    {
        _segments.Add(initial);
    }

    public Span(Span other)
    {
        _segments.AddRange(other._segments);
    }

    public int Length => _segments.Sum(_ => _.Length);

    public int Measure(string separator) =>
        _segments.Sum(_ => _.Length) + ((Math.Max(_segments.Count, 1) - 1) * separator.Length);

    public int Measure(SpanSegment separator) =>
        _segments.Sum(_ => _.Length) + ((Math.Max(_segments.Count, 1) - 1) * separator.Length);

    public IEnumerator<SpanSegment> GetEnumerator()
    {
        return _segments.GetEnumerator();
    }

    /// <summary>
    /// Joins together all segments with the given separator.
    /// </summary>
    /// <param name="separator">A segment to put between all of the <see cref="Span"/>'s segments.</param>
    /// <returns></returns>
    public string ToString(SpanSegment separator) =>
        string.Join(separator.ToString(), _segments.Select(_ => _.ToString()));

    /// <summary>
    /// Joins together all segments with the given separator.
    /// </summary>
    /// <param name="separator">A string to put between all of the <see cref="Span"/>'s segments.</param>
    /// <returns></returns>
    public string ToString(string separator) =>
        string.Join(separator, _segments.Select(_ => _.ToString()));

    public override string ToString() =>
        _segments.Aggregate(new StringBuilder(), (acc, el) => acc.Append(el.ToString())).ToString();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_segments).GetEnumerator();
    }

    public static implicit operator string(Span value) => value.ToString();

    public void Add(SpanSegment segment)
    {
        _segments.Add(segment);
    }

    public void Add(string text)
    {
        _segments.Add(new RawSpan(text));
    }

    public void Add(string text, Style style)
    {
        _segments.Add(new StyledSpan(text, style));
    }

    public void AddLine()
    {
        _segments.Add(new LineSpan());
    }
}

public abstract record SpanSegment
{
    public abstract int Length { get; }
}

public record RawSpan(string Text) : SpanSegment
{
    public override int Length => Text.Length;
    public override string ToString() => Text;
}

public record StyledSpan(string Text, Style Style) : SpanSegment
{
    public override int Length => Text.Length;
    public override string ToString() => Style.Apply(Text);
}

public record LineSpan : SpanSegment
{
    public override int Length => 0;

    public override string ToString()
    {
        return Environment.NewLine;
    }
}

public record Style : ITerminalSgrEffect
{
    public IOption<ForegroundColor> Fg { get; }
    public IOption<BackgroundColor> Bg { get; }

    public Style(ForegroundColor fg)
    {
        Fg = Option.NoneIfNull(fg);
        Bg = Option.None<BackgroundColor>();
    }

    public Style(BackgroundColor bg)
    {
        Fg = Option.None<ForegroundColor>();
        Bg = Option.NoneIfNull(bg);
    }

    public Style(ForegroundColor fg, BackgroundColor bg)
    {
        Fg = Option.NoneIfNull(fg);
        Bg = Option.NoneIfNull(bg);
    }

    public Style(Color color)
    {
        if (color == null) throw new ArgumentNullException(nameof(color));
        Fg = Option.NoneIfNull(color.Foreground);
        Bg = Option.NoneIfNull(color.Background);
    }

    public string Apply(string input)
    {
        // more fun, but also more bad..
        // return input
        //     .Apply(Fg, fg => fg.Apply)
        //     .Apply(Bg, bg => bg.Apply);


        return Terminal.Style(Fg, Bg, input);
        //var output = input;
        //var output = Terminal.Style(Fg, Bg, input);
        //Fg.Then(_ => output = _.Apply(output));
        //Bg.Then(_ => output = _.Apply(output));
        //return output;
    }

    public static implicit operator Style(Color color) => new Style(color);
    public static implicit operator Style(ForegroundColor fg) => new Style(fg);
    public static implicit operator Style(BackgroundColor bg) => new Style(bg);
}

/// <summary>
/// Represents a Select Graphic Rendition effect
/// </summary>
public interface ITerminalSgrEffect
{
    string Apply(string input);
}