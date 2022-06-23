using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace TodoHD.Tests;

public class SettingsTests
{
    [Fact]
    public void SmokeTest()
    {
        // Arrange
        var sut = (string[] lines) => SettingsReader.Read(lines);
        var testLines = @"
Theme = dark

Trace = true

HelpModeKey = Color(Red,Yellow)
".ReplaceLineEndings("\n").Split('\n');

        // Act
        var result = sut(testLines);
        // Assert
        Assert.True(result.Trace);
        Assert.Equal("Custom(Dark)", result.Theme.Name.Value);
        Assert.Equal(ForegroundColors.Red, result.Theme.HelpModeKey.Foreground);
        Assert.Equal(BackgroundColors.Yellow, result.Theme.HelpModeKey.Background);
    }

    [Fact]
    public void CanCustomizeAllColors()
    {
        // Arrange
        var sut = (string[] lines) => SettingsReader.Read(lines);
        var colors = typeof(Theme)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.PropertyType == typeof(Color));
        var testLines = string.Join("\n",
            colors.Select(x => $"{x.Name} = Color(Magenta,Magenta)")
        ).Split("\n");
        var expectedColor = new Color(ForegroundColors.Magenta, BackgroundColors.Magenta);

        // Act
        var result = sut(testLines);
        // Assert
        Assert.NotEmpty(colors);
        colors.ToList().ForEach(x =>
            Assert.Equal(
                expectedColor,
                x.GetValue(result.Theme)));
    }
}