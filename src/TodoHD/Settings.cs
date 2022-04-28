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

// TODO: this should be in settings.aeon
public class Settings
{
    private const string SETTINGS_FILE_NAME = ".todorc"; 
    private static readonly Dictionary<string, Theme> ThemesMap = new Dictionary<string, Theme>
    {
        ["dark"] = Theme.Dark,
        ["light"] = Theme.Light
    };
    public bool SaveOnModified { get; set; }
    public bool SaveOnNavigated { get; set; }
    public int SaveIntervalInMinutes { get; set; } = 5;
    public Theme Theme { get; set; } = Theme.Light;

    /// <summary>
    /// Begins by looking in the current working directory for a .todorc file, then in TodoHD's binary directory.
    /// <para>Returns the contents of whichever of those files it finds, or nothing.</para>
    /// </summary>
    private static IOption<string[]> ReadLines()
    {
        try
        {
            // first look in current working dir
            var settingsLines = File.ReadAllLines(SETTINGS_FILE_NAME);
            return Option.Some(settingsLines);
        }
        catch
        {
            try
            {
                // then look for settings file in the TodoHD binary's dir
                var binaryPath = AppContext.BaseDirectory;
                var rcPath = Path.Combine(binaryPath, SETTINGS_FILE_NAME);
                var settingsLines = File.ReadAllLines(rcPath);
                return Option.Some(settingsLines);
            }
            catch
            {
                // shouldn't fail for lack of a .todorc, just use defaults
            }
            return Option.None<string[]>();
        }
    }

    public static void Load()
    {
        var settingsLines = ReadLines();
        if (!settingsLines.IsSome(out var lines))
        {
            Instance = new Settings();
            return;
        }

        var settingsProperties =
            lines
                .Where(s => !s.TrimStart().StartsWith("#"))
                .Select(s => new KeyValuePair<string,string>(
                    s.Split('=', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToLowerInvariant(),
                    string.Join("", s.Split('=').Skip(1)).Trim()
                    ))
                .Where(kvp => kvp.Key != default)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value);

        var theme = ThemesMap.GetValueOrDefault(
            settingsProperties.GetValueOrDefault("theme", "dark").ToLowerInvariant(),
            Theme.Dark);

        //throw new InvalidOperationException(string.Join("\n", settingsProperties.Select(kvp => $"'{kvp.Key}':'{kvp.Value}'")) );
        
        var tmpColor = ReadColor(nameof(TodoHD.Theme.HelpModeHeader).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.HelpModeHeader = color);
        
        tmpColor = ReadColor(nameof(TodoHD.Theme.HelpModeText).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.HelpModeText = color);
        
        tmpColor = ReadColor(nameof(TodoHD.Theme.HelpModeKey).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.HelpModeKey = color);
            
        tmpColor = ReadColor(nameof(TodoHD.Theme.HelpLine).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.HelpLine = color);
        
        tmpColor = ReadColor(nameof(TodoHD.Theme.TodoItemHeader).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.TodoItemHeader = color);
        
        tmpColor = ReadColor(nameof(TodoHD.Theme.TodoItem).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.TodoItem = color);
        
        tmpColor = ReadColor(nameof(TodoHD.Theme.TodoItemSelected).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.TodoItemSelected = color);
        
        tmpColor = ReadColor(nameof(TodoHD.Theme.TodoItemUrgent).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.TodoItemUrgent = color);
        
        tmpColor = ReadColor(nameof(TodoHD.Theme.TodoItemUrgentSelected).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.TodoItemUrgentSelected = color);
        
        tmpColor = ReadColor(nameof(TodoHD.Theme.Step).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.Step = color);
        
        tmpColor = ReadColor(nameof(TodoHD.Theme.StepSelected).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.StepSelected = color);
        
        tmpColor = ReadColor(nameof(TodoHD.Theme.StepCompleted).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.StepCompleted = color);
        
        tmpColor = ReadColor(nameof(TodoHD.Theme.StepCompletedSelected).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.StepCompletedSelected = color);
        
        tmpColor = ReadColor(nameof(TodoHD.Theme.StepActive).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.StepActive = color);
        
        tmpColor = ReadColor(nameof(TodoHD.Theme.StepActiveSelected).ToLowerInvariant(), settingsProperties);
        tmpColor.Then(color => theme.StepActiveSelected = color);

        var settings = new Settings
        {
            Theme = theme,
        };
        Instance = settings;
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

    private static IOption<Color> ReadColor(string property, Dictionary<string, string> properties)
    {
        if (property == null || !properties.ContainsKey(property))
            return Option.None<Color>();

        var val = properties[property];
        var colors = ColorSettingRegex.Match(val);
        
        if (colors.Success &&
            ForegroundColorsMap.TryGetValue(colors.Groups["fg"].Value.ToLowerInvariant(), out var fg) &&
            BackgroundColorsMap.TryGetValue(colors.Groups["bg"].Value.ToLowerInvariant(), out var bg))
        {
            return Option.Some(new Color(fg, bg));
        }

        var fgprop = FgSettingRegex.Match(val);

        if (fgprop.Success &&
            ForegroundColorsMap.TryGetValue(fgprop.Groups["fg"].Value.ToLowerInvariant(), out fg))
        {
            return Option.Some(new Color(fg));
        }

        var bgprop = BgSettingRegex.Match(val);

        if (bgprop.Success &&
            BackgroundColorsMap.TryGetValue(bgprop.Groups["bg"].Value.ToLowerInvariant(), out bg))
        {
            return Option.Some(new Color(bg));
        }

        return Option.None<Color>();
    }
    
    public static Settings Instance { get; private set; } = new Settings();
}

public class Theme
{
    public Color HelpModeHeader { get; set; } = new Color(ForegroundColors.DarkCyan);
    public Color HelpModeText { get; set; } = new Color(ForegroundColors.White);
    public Color HelpModeKey { get; set; } = new Color(ForegroundColors.Green);
    public Color HelpLine { get; set; } = new Color(ForegroundColors.Green);
    public Color TodoItemHeader { get; set; } = new Color(ForegroundColors.Magenta);
    public Color TodoItem { get; set; } = Color.Default;
    public Color TodoItemSelected { get; set; } = new Color(ForegroundColors.Cyan);
    public Color TodoItemUrgent { get; set; } = Color.Default;
    public Color TodoItemUrgentSelected { get; set; } = new Color(ForegroundColors.Cyan);
    public Color Step { get; set; } = Color.Default;
    public Color StepSelected { get; set; } = new Color(ForegroundColors.Cyan);
    public Color StepCompleted { get; set; } = new Color(ForegroundColors.DarkGray);
    public Color StepCompletedSelected { get; set; } = new Color(ForegroundColors.Cyan);
    public Color StepActive { get; set; } = new Color(ForegroundColors.Blue, BackgroundColors.DarkGray);
    public Color StepActiveSelected { get; set; } = new Color(ForegroundColors.White, BackgroundColors.Blue);
    
    public static Theme Light => new Theme
    {
        HelpModeHeader = new Color(ForegroundColors.DarkBlue),
        HelpModeText = new Color(ForegroundColors.Black),
        HelpModeKey = new Color(ForegroundColors.Green),
        HelpLine = new Color(ForegroundColors.Green),
        TodoItemHeader = new Color(ForegroundColors.Magenta),
        TodoItem = Color.Default,
        TodoItemSelected = new Color(ForegroundColors.Cyan),
        TodoItemUrgent = Color.Default,
        TodoItemUrgentSelected = new Color(ForegroundColors.Cyan),
        Step = Color.Default,
        StepSelected = new Color(ForegroundColors.Cyan),
        StepCompleted = new Color(ForegroundColors.DarkGray),
        StepCompletedSelected = new Color(ForegroundColors.Cyan),
        StepActive = new Color(ForegroundColors.Black, BackgroundColors.Yellow),
        StepActiveSelected = new Color(ForegroundColors.White, BackgroundColors.Blue),
    };

    public static Theme Dark => new Theme
    {
        HelpModeHeader = new Color(ForegroundColors.DarkCyan),
        HelpModeText = new Color(ForegroundColors.White),
        HelpModeKey = new Color(ForegroundColors.Green),
        HelpLine = new Color(ForegroundColors.Green),
        TodoItemHeader = new Color(ForegroundColors.Magenta),
        TodoItem = Color.Default,
        TodoItemSelected = new Color(ForegroundColors.Cyan),
        TodoItemUrgent = Color.Default,
        TodoItemUrgentSelected = new Color(ForegroundColors.Cyan),
        Step = Color.Default,
        StepSelected = new Color(ForegroundColors.Cyan),
        StepCompleted = new Color(ForegroundColors.DarkGray),
        StepCompletedSelected = new Color(ForegroundColors.Cyan),
        StepActive = new Color(ForegroundColors.Blue, BackgroundColors.DarkGray),
        StepActiveSelected = new Color(ForegroundColors.White, BackgroundColors.Blue),
    };
}

public record Color(ForegroundColor Foreground, BackgroundColor Background)
{
    public Color(ForegroundColor foreground) : this(foreground, BackgroundColors.None)
    {
    }
    public Color(BackgroundColor background) : this(ForegroundColors.None, background)
    {
    }
    public static readonly Color Default = new Color(ForegroundColors.None, BackgroundColors.None);
}
