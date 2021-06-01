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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TodoHD
{
	public class ViewMode : IMode
	{
		int _step = 0;
		int _stepStart = 0;
		TodoItem _item;

		void PrintHelpLine()
		{
			Helpers.WithForeground(ConsoleColor.Green, () => {
					Console.WriteLine($"[+] Add step [-] Remove step [E] Edit [Space] Mark step [Q] Quit");
					});
		}

		public void Init(Editor editor) {
			_step = 0;
			_item = editor.GetSelectedItem();
			_item.Steps ??= new();
			Console.Clear();
			PrintHelpLine();
		}

		public void PrintUI(Editor editor)
		{
			Console.SetCursorPosition(0, 1);
			Helpers.WithForeground(ConsoleColor.Magenta, () => {
			Console.WriteLine($"== {_item.Title} ==");
			});
			switch(_item.Priority)
			{
				case Priority.Whenever:
					Console.Write("   ");
					Helpers.WithBackground(ConsoleColor.Green, () => {
					Console.WriteLine($"<{_item.Priority}>");
					});
					break;
				case Priority.Urgent:
					Console.Write("   ");
					Helpers.WithBackground(ConsoleColor.Red, () => {
					Console.WriteLine($"*{_item.Priority}*");
					});
					break;
			}
			_item
				.Description
				.Split(Environment.NewLine)
				.ToList()
				.ForEach(part =>
					Console.WriteLine($"{new string(' ', 2)}{part}{new string(' ', Console.BufferWidth - 1 - 2 - part.Length)}"));

			_stepStart = Console.CursorTop + 1;
			
			PrintSteps(editor);

		}

		void PrintSteps(Editor editor)
		{
			Console.SetCursorPosition(0, _stepStart);
			if(_item.Steps == null)
			{
				return;
			}

			var sb = new StringBuilder();
			_item
				.Steps
				.Select((step,index) => new{step,index})
				.ToList()
				.ForEach(it => {
					var c = 0;
					if(_step == it.index) {
						sb.Append("> ");
						c += 2;
					}
					c += "[x] ".Length + it.step.Text.Length;
					sb.AppendLine($"[{(it.step.Completed ? 'x' : ' ')}] {it.step.Text}{new string(' ', Console.BufferWidth - 1 - c)}");
				});
			Console.Write(sb);
		}

		public void KeyEvent(ConsoleKeyInfo key, Editor editor)
		{
			switch(key.Key)
			{
				case ConsoleKey.Backspace:
					editor.PopMode();
					break;
				case ConsoleKey.E:
					editor.PushMode(new EditMode());
					break;
				case ConsoleKey.DownArrow:
				case ConsoleKey.J:
					NextStep(editor);
					break;
				case ConsoleKey.UpArrow:
				case ConsoleKey.K:
					PrevStep(editor);
					break;
				case ConsoleKey.OemPlus:
					AddStep(editor);
					break;
				case ConsoleKey.OemMinus:
					DeleteStep(editor);
					break;
				case ConsoleKey.Spacebar:
					MarkStep(editor);
					break;
			}
		}

		void AddStep(Editor editor)
		{
			Console.WriteLine("Step text:");
			Console.CursorVisible = true;
			var text = Helpers.GetNonEmptyString();
			Console.CursorVisible = false;
			_item.Steps.Add(new(){ Text = text });
			Init(editor);
			PrintUI(editor);
			editor.Save();
		}

		void DeleteStep(Editor editor)
		{
			if(_item.Steps.Count == 0)
			{
				return;
			}
			Console.WriteLine();
			Console.WriteLine("Are you sure you want to delete this step? (y/n)");
			Console.WriteLine(_item.Steps[_step].Text);
			Console.CursorVisible = true;
			var delete = Helpers.GetNonEmptyString();
			Console.CursorVisible = false;
			if(delete.ToUpperInvariant() == "Y")
			{
				_item.Steps.RemoveAt(_step);
				editor.Save();
			}

			Init(editor);
			PrintUI(editor);
		}

		void MarkStep(Editor editor)
		{
			if(_item.Steps.Count == 0)
			{
				return;
			}
			_item.Steps[_step].Completed = !_item.Steps[_step].Completed;
			PrintSteps(editor);
			editor.Save();
		}

		void NextStep(Editor editor)
		{
			_step = Math.Clamp(_step + 1, 0, Math.Max(0, _item.Steps.Count - 1));
			PrintSteps(editor);
		}

		void PrevStep(Editor editor)
		{
			_step = Math.Clamp(_step - 1, 0, Math.Max(0, _item.Steps.Count - 1));
			PrintSteps(editor);
		}
	}
}
