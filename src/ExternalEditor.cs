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
using System.Diagnostics;
using System.IO;
using System.Text;
using SaferVariants;

namespace TodoHD;

public class ExternalEditor
{
    private readonly string _initialText;
    private static readonly string _editor = GetEditor();

    public ExternalEditor(string initialText)
    {
        _initialText = initialText;
    }

    public IOption<string> Edit(bool hideCursorAfterwards = true)
    {
        if (_editor == null)
        {
            return Option.None<string>();
        }
        var tempFilePath = GetTempFile();
        // write initial text to edit
        File.WriteAllText(tempFilePath, _initialText);
        // open in editor
        var procStart = new ProcessStartInfo(_editor, $"\"{tempFilePath}\"")
        {
            UseShellExecute = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
        };

        try
        {
            Console.OutputEncoding = Encoding.UTF8;
            // attempt to start editor
            var proc = Process.Start(procStart);
            if (proc == null)
            {
                return Option.None<string>();
            }

            // wait for editor to close
            proc.WaitForExit();
            // read file text
            var editedText = File.ReadAllText(tempFilePath);
            File.Delete(tempFilePath); // note: if adding extension to temp file later, 
            // we should remove the original temp file as well as the one with the extension, to reduce clutter
            
            
            return string.Equals(editedText, _initialText) 
                ? Option.None<string>() // no edit
                : Option.Some(editedText);
        }
        finally
        {
            Console.OutputEncoding = Encoding.UTF8;
            if (hideCursorAfterwards)
            {
                Console.CursorVisible = false;
            }
        }
    }

    private static string GetTempFile()
    {
        var path = Path.GetTempFileName(); // if adding a file extension to the file in the future,
        // note that you will have to loop until you are sure you have a unique file,
        // as currently that is handled by `GetTempFileName`
        // also note that files and directories cannot have the same name on unix, so take that into account
        
        return path;
    }

    private static string GetEditor()
    {
        var editor = Environment.GetEnvironmentVariable("VISUAL");
        return !string.IsNullOrWhiteSpace(editor)
            ? editor
            : Environment.GetEnvironmentVariable("EDITOR");
    }

}