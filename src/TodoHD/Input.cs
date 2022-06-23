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
using SaferVariants;
using TodoHD.Controls;

namespace TodoHD;

public record EnumOption(int Number, string Name);

public enum Confirm
{
    No,
    Yes
}

public record SelectItem<T>(string Text, T Item);

public static class Input
{
    public static string GetNonEmptyString()
    {
        string text;
        do
        {
            Console.Write(">");
        } while (string.IsNullOrWhiteSpace(text = Console.ReadLine()));

        return text;
    }

    public static IOption<string> GetString()
    {
        Console.Write(">");
        string text = Console.ReadLine();
        return string.IsNullOrWhiteSpace(text)
            ? Option.None<string>()
            : Option.Some(text);
    }

    public static Confirm Confirm(int height)
    {
        var options =
            Enum
                .GetValues<Confirm>()
                .Cast<int>()
                .Zip(Enum.GetNames(typeof(Confirm)))
                .Select(p => new EnumOption(p.Item1, p.Item2))
                .ToList();

        var listBox = new PagedListBox<EnumOption>(
            () => options,
            priority => " " + priority.Name,
            (_, formattedString, isSelected) =>
            {
                if (isSelected)
                {
                    // TODO: use theme here and in GetPriority
                    return Terminal.Foreground(ForegroundColors.Cyan, formattedString);
                }
                else
                {
                    return formattedString;
                }
            }
        )
        {
            HidePageNumberIfSinglePage = true
        };
        var stepStart = Console.CursorTop;
        listBox.Print(Console.WindowWidth, height);
        ConsoleKeyInfo key;
        // wait for enter keypress and use selected
        while ((key = Console.ReadKey(true)) is not { Key: ConsoleKey.Enter })
        {
            switch (key.Key)
            {
                case ConsoleKey.K or ConsoleKey.UpArrow when listBox.SelectPrevious():
                case ConsoleKey.J or ConsoleKey.DownArrow when listBox.SelectNext():
                    Console.SetCursorPosition(0, stepStart);
                    listBox.Print(Console.WindowWidth, Console.WindowHeight - stepStart - 1);
                    break;
            }
        }


        var selectedItem = listBox.SelectedItem.ValueOr(new EnumOption(-1, "Unknown"));
        Console.WriteLine($"{selectedItem.Number} {selectedItem.Name}");

        return (Confirm)selectedItem.Number;
    }

    public static Priority GetPriority(int height)
    {
        var priorities =
            Enum
                .GetValues<Priority>()
                .Cast<int>()
                .Zip(Enum.GetNames(typeof(Priority)))
                .Select(p => new EnumOption(p.Item1, p.Item2))
                .ToList();
        var listBox = new PagedListBox<EnumOption>(
            () => priorities,
            priority => " " + priority.Name,
            (priority, formattedString, isSelected) =>
            {
                if (isSelected)
                {
                    return Terminal.Foreground(ForegroundColors.Cyan, formattedString);
                }
                else
                {
                    return formattedString;
                }
            }
        )
        {
            HidePageNumberIfSinglePage = true
        };
        var stepStart = Console.CursorTop;
        listBox.Print(Console.WindowWidth, height);
        ConsoleKeyInfo key;
        // wait for enter keypress and use selected
        while ((key = Console.ReadKey(true)) is not { Key: ConsoleKey.Enter })
        {
            switch (key.Key)
            {
                case ConsoleKey.K or ConsoleKey.UpArrow when listBox.SelectPrevious():
                case ConsoleKey.J or ConsoleKey.DownArrow when listBox.SelectNext():
                    Console.SetCursorPosition(0, stepStart);
                    listBox.Print(Console.WindowWidth, Console.WindowHeight - stepStart - 1);
                    break;
            }
        }


        var selectedItem = listBox.SelectedItem.ValueOr(new EnumOption(-1, "Unknown"));
        Console.WriteLine($"{selectedItem.Number} {selectedItem.Name}");

        return (Priority)selectedItem.Number;
    }

    // public static Priority GetPriority()
    // {
    //     var priorities = 
    //         Enum
    //             .GetValues<Priority>()
    //             .Cast<int>()
    //             .Zip(Enum.GetNames(typeof(Priority)))
    //             .Select(p => new EnumOption(p.Item1, p.Item2))
    //             .ToList();
    //     PrintOptions(priorities);
    //     Console.Write(">");
    //     string text;
    //     int number;
    //     while(string.IsNullOrWhiteSpace((text = Console.ReadLine())) || !int.TryParse(text, out number) || !priorities.Any(p => p.Number == number))
    //     {
    //         PrintOptions(priorities);
    //         Console.Write(">");
    //     }
    //     return (Priority)number;
    // }


    public static void PrintOptions(IEnumerable<EnumOption> options)
    {
        options
            .Select(o => $"{o.Number} {o.Name}")
            .ToList()
            .ForEach(Console.WriteLine);
    }

    public static IOption<SelectItem<T>> Select<T>(IEnumerable<SelectItem<T>> options, int height)
    {
        var accumulator = new Accumulator(100);

        var listBox = new PagedListBox<SelectItem<T>>(
            () => options,
            option => $" {option.Text}",
            (_, formattedString, isSelected) =>
            {
                if (isSelected)
                {
                    // TODO: use theme here and in Input.Confirm in Input.GetPriority
                    return Terminal.Foreground(ForegroundColors.Cyan, formattedString);
                }
                else
                {
                    return formattedString;
                }
            }
        )
        {
            HidePageNumberIfSinglePage = true
        };
        var stepStart = Console.CursorTop;
        listBox.Print(Console.WindowWidth, height);
        ConsoleKeyInfo key;

        // wait for enter keypress and use selected
        while ((key = Console.ReadKey(true)) is not { Key: ConsoleKey.Enter })
        {
            switch (key.Key)
            {
                case ConsoleKey.K or ConsoleKey.UpArrow:
                    accumulator.Execute(() => listBox.SelectPrevious());
                    Console.SetCursorPosition(0, stepStart);
                    listBox.Print(Console.WindowWidth, Console.WindowHeight - stepStart - 1);
                    break;
                case ConsoleKey.J or ConsoleKey.DownArrow:
                    accumulator.Execute(() => listBox.SelectNext());
                    Console.SetCursorPosition(0, stepStart);
                    listBox.Print(Console.WindowWidth, Console.WindowHeight - stepStart - 1);
                    break;
                case ConsoleKey.G:
                    if (key.Modifiers == ConsoleModifiers.Shift)
                    {
                        listBox.SelectLast();
                    }
                    else
                    {
                        listBox.SelectFirst();
                    }

                    accumulator.Reset();
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
                    accumulator.AccumulateDigit((uint)key.Key - 48);
                    break;
                case ConsoleKey.Q:
                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    return Option.None<SelectItem<T>>();
                default:
                    accumulator.Reset();
                    break;
            }
        }


        return listBox.SelectedItem;
    }
}