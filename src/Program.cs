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
	public enum Priority
	{
		Whenever,
		Urgent
	}

	public class TodoStep
	{
		public string Text {get;set;}
		public bool Completed {get;set;}
	}

	public class TodoItem
	{
		public TodoItem() { }
		public TodoItem(int id, string title, string description, string category, Priority priority)
		{
			Id = id;
			Title = title;
			Description = description;
			Category = category;
			Priority = priority;
		}
		public int Id {get;set;}
		public string Title {get;set;}
		public string Description {get;set;}
		public string Category {get;set;}
		public Priority Priority {get;set;}
		public List<TodoStep> Steps {get;set;}
	}

	public class Todo
	{
		public List<TodoItem> Items {get;set;}
		public List<string> Categories {get;set;}
	}


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
