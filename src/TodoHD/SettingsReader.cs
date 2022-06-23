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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SaferVariants;

namespace TodoHD;

public class SettingsReader
{
    private static readonly Dictionary<string, Theme> ThemesMap = new Dictionary<string, Theme>
    {
        ["dark"] = Theme.Dark,
        ["light"] = Theme.Light
    };

    private static IOption<string[]> TryReadLines(string path)
    {
        try
        {
            return Option.Some(File.ReadAllLines(path));
        }
        catch
        {
            // shouldn't fail for lack of a .todorc, just use defaults
            return Option.None<string[]>();
        }
    }

    /// <summary>
    /// Begins by looking in the current working directory for a .todorc file, then in TodoHD's binary directory.
    /// <para>Returns the contents of whichever of those files it finds, or nothing.</para>
    /// </summary>
    /// <param name="fileName"></param>
    private static IOption<string[]> ReadSettingsLines(string fileName)
    {
        // first look for settings file in current working dir
        var lines = TryReadLines(fileName);

        if (lines.IsSome())
        {
            return lines;
        }

        // then look for settings file in the TodoHD binary's dir
        return TryReadLines(Path.Combine(AppContext.BaseDirectory, fileName));
    }

    public static Settings Read(string[] lines)
    {
        var settingsProperties =
            lines
                .Where(s => !s.TrimStart().StartsWith('#'))
                .Select(s => new KeyValuePair<string, string>(
                    s.Split('=', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault()?.ToLowerInvariant(),
                    string.Join('=', s.Split('=').Skip(1)).Trim()
                ))
                .Where(kvp => kvp.Key != default)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value);

        var theme = ThemesMap.GetValueOrDefault(
            settingsProperties.GetValueOrDefault("theme", "dark").ToLowerInvariant(),
            Theme.Light);

        var trace = bool.Parse(settingsProperties.GetValueOrDefault("trace", "false").ToLowerInvariant());

        //throw new InvalidOperationException(string.Join("\n", settingsProperties.Select(kvp => $"'{kvp.Key}':'{kvp.Value}'")) );

        var tmpColor = ReadColor(nameof(Theme.HelpModeHeader).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.HelpModeHeader = color);

        tmpColor = ReadColor(nameof(Theme.HelpModeText).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.HelpModeText = color);

        tmpColor = ReadColor(nameof(Theme.HelpModeKey).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.HelpModeKey = color);

        tmpColor = ReadColor(nameof(Theme.HelpLine).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.HelpLine = color);

        tmpColor = ReadColor(nameof(Theme.TodoItemHeader).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.TodoItemHeader = color);

        tmpColor = ReadColor(nameof(Theme.TodoItem).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.TodoItem = color);

        tmpColor = ReadColor(nameof(Theme.TodoItemSelected).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.TodoItemSelected = color);

        tmpColor = ReadColor(nameof(Theme.TodoItemUrgent).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.TodoItemUrgent = color);

        tmpColor = ReadColor(nameof(Theme.TodoItemUrgentSelected).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.TodoItemUrgentSelected = color);

        tmpColor = ReadColor(nameof(Theme.Step).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.Step = color);

        tmpColor = ReadColor(nameof(Theme.StepSelected).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.StepSelected = color);

        tmpColor = ReadColor(nameof(Theme.StepCompleted).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.StepCompleted = color);

        tmpColor = ReadColor(nameof(Theme.StepCompletedSelected).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.StepCompletedSelected = color);

        tmpColor = ReadColor(nameof(Theme.StepActive).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.StepActive = color);

        tmpColor = ReadColor(nameof(Theme.StepActiveSelected).ToLowerInvariant(), settingsProperties, theme);
        tmpColor.Then(color => theme.StepActiveSelected = color);

        var settings = new Settings
        {
            Trace = trace,
            Theme = theme,
        };
        return settings;
    }

    public static Settings Read(string fileName)
    {
        var settingsLines = ReadSettingsLines(fileName);

        if (!settingsLines.IsSome(out var lines))
        {
            return new Settings();
        }

        return Read(lines);
    }

    private static readonly Dictionary<string, BackgroundColor> BackgroundColorsMap =
        new Dictionary<string, BackgroundColor>()
        {
            ["none"] = BackgroundColors.None,
            ["reset"] = BackgroundColors.Reset,
            ["black"] = BackgroundColors.Black,
            ["darkred"] = BackgroundColors.DarkRed,
            ["darkgreen"] = BackgroundColors.DarkGreen,
            ["darkyellow"] = BackgroundColors.DarkYellow,
            ["darkblue"] = BackgroundColors.DarkBlue,
            ["darkmagenta"] = BackgroundColors.DarkMagenta,
            ["darkcyan"] = BackgroundColors.DarkCyan,
            ["gray"] = BackgroundColors.Gray,
            ["darkgray"] = BackgroundColors.DarkGray,
            ["red"] = BackgroundColors.Red,
            ["green"] = BackgroundColors.Green,
            ["yellow"] = BackgroundColors.Yellow,
            ["blue"] = BackgroundColors.Blue,
            ["magenta"] = BackgroundColors.Magenta,
            ["cyan"] = BackgroundColors.Cyan,
            ["white"] = BackgroundColors.White,
        };

    private static readonly Dictionary<string, ForegroundColor> ForegroundColorsMap =
        new Dictionary<string, ForegroundColor>()
        {
            ["none"] = ForegroundColors.None,
            ["reset"] = ForegroundColors.Reset,
            ["black"] = ForegroundColors.Black,
            ["darkred"] = ForegroundColors.DarkRed,
            ["darkgreen"] = ForegroundColors.DarkGreen,
            ["darkyellow"] = ForegroundColors.DarkYellow,
            ["darkblue"] = ForegroundColors.DarkBlue,
            ["darkmagenta"] = ForegroundColors.DarkMagenta,
            ["darkcyan"] = ForegroundColors.DarkCyan,
            ["gray"] = ForegroundColors.Gray,
            ["darkgray"] = ForegroundColors.DarkGray,
            ["red"] = ForegroundColors.Red,
            ["green"] = ForegroundColors.Green,
            ["yellow"] = ForegroundColors.Yellow,
            ["blue"] = ForegroundColors.Blue,
            ["magenta"] = ForegroundColors.Magenta,
            ["cyan"] = ForegroundColors.Cyan,
            ["white"] = ForegroundColors.White,
        };

    private static readonly Regex ColorSettingRegex =
        new Regex(@"[Cc]olor\(\s?(?'fg'[A-Za-z]{1,20})\s?,\s?(?'bg'[A-Za-z]{1,20})\s?\)", RegexOptions.Compiled);

    private static readonly Regex FgSettingRegex =
        new Regex(@"[Ff][Gg]\(\s?(?'fg'[A-Za-z]{1,20})\s?\)", RegexOptions.Compiled);

    private static readonly Regex BgSettingRegex =
        new Regex(@"[Bb][Gg]\(\s?(?'bg'[A-Za-z]{1,20})\s?\)", RegexOptions.Compiled);

    private static IOption<Color> ReadColor(string property, Dictionary<string, string> properties, Theme theme)
    {
        if (property == null || !properties.ContainsKey(property))
            return Option.None<Color>();

        var val = properties[property];
        var colors = ColorSettingRegex.Match(val);

        if (colors.Success &&
            ForegroundColorsMap.TryGetValue(colors.Groups["fg"].Value.ToLowerInvariant(), out var fg) &&
            BackgroundColorsMap.TryGetValue(colors.Groups["bg"].Value.ToLowerInvariant(), out var bg))
        {
            if (!theme.Name.Custom)
            {
                theme.Name = new ThemeName($"Custom({theme.Name.Value})", true);
            }

            return Option.Some(new Color(fg, bg));
        }

        var fgprop = FgSettingRegex.Match(val);

        if (fgprop.Success &&
            ForegroundColorsMap.TryGetValue(fgprop.Groups["fg"].Value.ToLowerInvariant(), out fg))
        {
            if (!theme.Name.Custom)
            {
                theme.Name = new ThemeName($"Custom({theme.Name.Value})", true);
            }

            return Option.Some(new Color(fg));
        }

        var bgprop = BgSettingRegex.Match(val);

        if (bgprop.Success &&
            BackgroundColorsMap.TryGetValue(bgprop.Groups["bg"].Value.ToLowerInvariant(), out bg))
        {
            if (!theme.Name.Custom)
            {
                theme.Name = new ThemeName($"Custom({theme.Name.Value})", true);
            }

            return Option.Some(new Color(bg));
        }

        return Option.None<Color>();
    }
}