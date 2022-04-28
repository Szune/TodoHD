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

namespace TodoHD;

// public interface IListBoxPainter<T>
// {
//     ISpan Paint(T item, string formattedText, bool isSelected);
// }
//
// public class TodoListBoxPainter : IListBoxPainter<TodoStep>
// {
//     public ISpan Paint(TodoStep item, string formattedText, bool isSelected)
//     {
//         if (isSelected)
//         {
//             if (item.Active && !item.Completed)
//             {
//                 return new BackgroundSpan(BackgroundColors.Blue,
//                     new ForegroundSpan(ForegroundColors.White,
//                         new Span(formattedText)
//                     ));
//             }
//             else
//             {
//                 return new ForegroundSpan(ForegroundColors.Cyan, new Span(formattedText));
//             }
//         }
//         if (item.Completed)
//         {
//             return new ForegroundSpan(ForegroundColors.DarkGray, new Span(formattedText));
//         }
//         if (item.Active)
//         {
//                 return new BackgroundSpan(BackgroundColors.DarkGray,
//                     new ForegroundSpan(ForegroundColors.Blue,
//                         new Span(formattedText)
//                     ));
//         }
//         return new Span(formattedText);
//     }
// }
//
// public record Span(string Text) : ISpan;
// public record ForegroundSpan(ForegroundColor Foreground, ISpan Span) : ISpan;
// public record BackgroundSpan(BackgroundColor Background, ISpan Span) : ISpan;
//
// public interface ISpan
// {
//     void Render(IConsoleRender render);
// }
//
// public interface IConsoleRender
// {
//     void Write(string text);
//     void Write(string text, BackgroundColor color);
//     void Write(string text, ForegroundColor color);
//     void Write(string text, ForegroundColor fg, BackgroundColor bg);
// }