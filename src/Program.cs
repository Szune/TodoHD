//
// TodoHD is a CLI tool/TUI to organize stuff you need to do.
// Copyright (C) 2021  Carl Erik Patrik Iwarson
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
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TodoHD
{
	class Program
	{
		static void Main(string[] args)
		{
			string path;
			if(args.Length == 1 && args[0] == ".")
			{
				path = "todohd.json";
			}
			else
			{
				path = Path.Combine(AppContext.BaseDirectory, "todohd.json");
			}
			Console.InputEncoding = Console.OutputEncoding = System.Text.Encoding.Unicode;
			var editor = new Editor(path);
			editor.Load();
			editor.PushMode(new NormalMode());
			editor.Start();
		}
	}
}
