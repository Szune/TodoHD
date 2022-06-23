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

using TodoHD.Rendering;

namespace TodoHD;

// TODO: this should be in the new settings format when it's implemented
public class Settings
{
    private const string SETTINGS_FILE_NAME = ".todorc";

    public bool SaveOnModified { get; set; }
    public bool SaveOnNavigated { get; set; }
    public int SaveIntervalInMinutes { get; set; } = 5;
    public Theme Theme { get; set; } = Theme.Light;
    public bool Trace { get; set; } = false;

    public static void Load()
    {
        Instance = SettingsReader.Read(SETTINGS_FILE_NAME);
    }

    public static Settings Instance { get; private set; } = new Settings();
}

public class Theme
{
    public ThemeName Name { get; set; } = "Init";
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
        Name = "Light",
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
        Name = "Dark",
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

public record Color(ForegroundColor Foreground, BackgroundColor Background) : ITerminalSgrEffect
{
    public Color(ForegroundColor foreground) : this(foreground, BackgroundColors.None)
    {
    }

    public Color(BackgroundColor background) : this(ForegroundColors.None, background)
    {
    }

    public static readonly Color Default = new Color(ForegroundColors.None, BackgroundColors.None);
    public string Apply(string input) => Terminal.Color(this, input);

    public StyledSpan Span(string text)
    {
        return new StyledSpan(text, this);
    }
}

public record ThemeName(string Value, bool Custom)
{
    public static implicit operator ThemeName(string value)
    {
        return new ThemeName(value, false);
    }
}