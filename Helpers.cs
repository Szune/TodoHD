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
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace TodoHD
{
	public record Option(int Number, string Name);

	public static class Helpers
	{
		public static void WithBackground(ConsoleColor color, Action print)
		{
			var old = Console.BackgroundColor;
			Console.BackgroundColor = color;
			print();
			Console.BackgroundColor = old;
		}

		public static void WithForeground(ConsoleColor color, Action print)
		{
			var old = Console.ForegroundColor;
			Console.ForegroundColor = color;
			print();
			Console.ForegroundColor = old;
		}

		public static string GetNonEmptyString()
		{
			Console.Write(">");
			string text;
			while(string.IsNullOrWhiteSpace((text = Console.ReadLine())))
			{
				Console.Write(">");
			}
			return text;
		}

		public static Priority GetPriority()
		{
			var priorities = 
				Enum
				.GetValues<Priority>()
				.Cast<int>()
				.Zip(Enum.GetNames(typeof(Priority)))
				.Select(p => new Option(p.Item1, p.Item2))
				.ToList();
			Helpers.PrintOptions(priorities);
			Console.Write(">");
			string text;
			int number;
			while(string.IsNullOrWhiteSpace((text = Console.ReadLine())) || !int.TryParse(text, out number) || !priorities.Any(p => p.Number == number))
			{
				Helpers.PrintOptions(priorities);
				Console.Write(">");
			}
			return (Priority)number;
		}


		public static void PrintOptions(IEnumerable<Option> options)
		{
			options
				.Select(o => $"{o.Number} {o.Name}")
				.ToList()
				.ForEach(Console.WriteLine);
		}

	}
}
