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
        listBox.Print(Console.BufferWidth, height);
        ConsoleKeyInfo key;
        // wait for enter keypress and use selected
        while ((key = Console.ReadKey(true)) is not { Key: ConsoleKey.Enter })
        {
            switch (key.Key)
            {
                case ConsoleKey.K or ConsoleKey.UpArrow when listBox.SelectPrevious():
                case ConsoleKey.J or ConsoleKey.DownArrow when listBox.SelectNext():
                    Console.SetCursorPosition(0, stepStart);
                    listBox.Print(Console.BufferWidth, Console.BufferHeight - stepStart - 1);
                    break;
            }
        }

        Console.WriteLine($"{listBox.SelectedItem.Number} {listBox.SelectedItem.Name}");

        return (Priority)listBox.SelectedItem.Number;
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

}