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
using System.IO;
using System.Linq;
using TodoHD.Modes;

namespace TodoHD;

public class Program
{
    public static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--version")
        {
            Console.WriteLine(Version.Current);
            return 0;
        }

        string path;
        const string fileName = "todohd.json";
        if (args.Length > 0 && args[0] == ".")
        {
            path = fileName;
        }
        else
        {
            path = Path.Combine(AppContext.BaseDirectory, fileName);
        }

        if (OperatingSystem.IsWindows())
        {
            Console.InputEncoding = Console.OutputEncoding = System.Text.Encoding.Unicode;
        }
        else
        {
            Console.InputEncoding = Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        Logger.Initialize(path);
        var editor = new Editor(path);
        editor.Load();

        if (args.Contains("--list"))
        {
            var items = editor.GetItems();
            Console.WriteLine(string.Join("\r\n", items.Select(i => i.Title)));
            return 0;
        }

        if (args.Contains("--single"))
        {
            var input = string.Join(" ", args.SkipWhile(it => it != "--single").Skip(1));
            var single = input.ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(single))
            {
                Console.WriteLine("--single missing value");
                return 1;
            }

            var item = editor.GetItems().FirstOrDefault(it => it.Title.ToUpperInvariant().StartsWith(single));

            if (item == null)
            {
                Console.WriteLine($"failed to find '{input}'");
                return 1;
            }

            Console.WriteLine($"> {item.Title}");
            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                Console.WriteLine($"| {item.Description.ExceptEndingNewline()}");
            }

            Console.WriteLine(string.Join("\r\n",
                item
                    .Steps
                    .Select(step =>
                        $"[{(step.Completed ? 'x' : step.Active ? 'o' : ' ')}] {step.Text.ExceptEndingNewline()}")));
            return 0;
        }


        Settings.Load();
        editor.PushMode(new NormalMode());

        try
        {
            Console.CancelKeyPress += (_, args) =>
            {
                editor.Save(); // might not actually want this behavior, should be a setting
#if DEBUG
                Logger.DebugSave();
#endif
            };
            editor.Start();
        }
        catch (Exception ex)
        {
            editor.Save(true);
            Logger.LogException(ex);
            Console.WriteLine(ex);
        }
        finally
        {
            editor.Save();
#if DEBUG
            Logger.DebugSave();
#endif
        }

        return 0;
    }
}