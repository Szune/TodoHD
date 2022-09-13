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
using System.Text;
using SaferVariants;
using SaferVariants.Extensions;

namespace TodoHD;

public class SyntaxError
{
    public readonly string Message;

    public SyntaxError(string message)
    {
        Message = message;
    }
}

public class TodoSerializer
{
    enum DeserializerState
    {
        Title,
        Description,
        Steps
    }

    enum ParseResult
    {
        Continue,
        Done
    }


    public static string Serialize(TodoItem todo)
    {
        var sb = new StringBuilder();
        sb.Append($"== {todo.Title} ==").Append('\n');
        sb.Append(todo.Description).Append('\n');
        sb.Append("== Steps ==").Append('\n');
        var steps = todo.Steps ?? Enumerable.Empty<TodoStep>();
        foreach (var step in steps.OrderBy(x => x.Order))
        {
            sb.Append($"- [{(step.Completed ? "x" : step.Active ? "o" : " ")}] ");
            var escaped = string.Join('\n', step
                .Text
                .ReplaceLineEndings("\n")
                .Split('\n')
                .Select(x => x.StartsWith("- [") ? "\\" + x : x));
            sb.Append(escaped).Append('\n');
        }

        return sb.ToString();
    }

    public static IResult<TodoItem, SyntaxError> Deserialize(string todo)
    {
        var state = DeserializerState.Title;
        var item = new TodoItem();
        var titleParser = new TitleParser();
        var descriptionParser = new DescriptionParser();
        var stepsParser = new StepsParser();
        var lineNum = 0;
        foreach (var line in todo.Lines())
        {
            switch (state)
            {
                case DeserializerState.Title:
                {
                    var result = titleParser.Parse(line, lineNum);
                    switch (result)
                    {
                        case Ok<ParseResult, SyntaxError> { Value: ParseResult.Done }:
                            item.Title = titleParser.Title;
                            state = DeserializerState.Description;
                            break;
                        case Err<ParseResult, SyntaxError> err:
                            return Result.Err<TodoItem, SyntaxError>(err.Error);
                    }

                    break;
                }
                case DeserializerState.Description:
                {
                    var result = descriptionParser.Parse(line, lineNum);
                    switch (result)
                    {
                        case Ok<ParseResult, SyntaxError> { Value: ParseResult.Done }:
                            item.Description = descriptionParser.Description;
                            state = DeserializerState.Steps;
                            break;
                        case Err<ParseResult, SyntaxError> err:
                            return Result.Err<TodoItem, SyntaxError>(err.Error);
                    }

                    break;
                }

                case DeserializerState.Steps:
                {
                    var result = stepsParser.Parse(line, lineNum);
                    switch (result)
                    {
                        case Some<SyntaxError> err:
                            return Result.Err<TodoItem, SyntaxError>(err.Value);
                        case None<SyntaxError>:
                            break;
                    }

                    break;
                }
                default:
                    throw new NotImplementedException();
            }

            lineNum++;
        }

        stepsParser.Complete();
        item.Steps = stepsParser.Steps;
        return item.ToOk<TodoItem, SyntaxError>();
    }

    class TitleParser
    {
        private string _text = "";

        public string Title => _text.Length > 0
            ? _text
            : throw new InvalidOperationException(
                "Bug: Tried to get the title before it was found.");

        public IResult<ParseResult, SyntaxError> Parse(string line, int lineNum)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            if (_text.Length > 0)
            {
                throw new InvalidOperationException(
                    "Bug: Tried to parse title after it was found.");
            }

            if (string.IsNullOrWhiteSpace(line) &&
                _text.Length < 1)
            {
                return ParseResult.Continue.ToOk<ParseResult, SyntaxError>();
            }

            if (line.Length < 5) // 2 equals + at least 1 char for the title
            {
                return new SyntaxError(
                        $"Expected title in the format '== text ==' but found '{line}' on line {lineNum}")
                    .ToErr<ParseResult, SyntaxError>();
            }

            if (line[0] != '=' || line[1] != '=')
            {
                return new SyntaxError(
                        $"Expected title in the format '== text ==' but line does not start with '==': '{line}' on line {lineNum}")
                    .ToErr<ParseResult, SyntaxError>();
            }


            var lastEqualsIndex = line.LastIndexOf('=');
            if (lastEqualsIndex < 4 || line[lastEqualsIndex - 1] != '=')
                // last equals has to be on at least index 4 (5th character)
            {
                return new SyntaxError(
                        $"Expected title in the format '== text ==' but line does not end with '==': '{line}' on line {lineNum}")
                    .ToErr<ParseResult, SyntaxError>();
            }

            var title = line.Substring(2, lastEqualsIndex - 3).Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return new SyntaxError(
                        $"Title has to contain non-whitespace characters: '{line}' on line {lineNum}")
                    .ToErr<ParseResult, SyntaxError>();
            }

            _text = title;

            return ParseResult.Done.ToOk<ParseResult, SyntaxError>();
        }
    }

    class DescriptionParser
    {
        private readonly StringBuilder _text = new();
        private bool _foundStepsStart;

        public string Description => _foundStepsStart
            ? _text.ToString()
            : throw new InvalidOperationException(
                "Bug: Tried to get the description before it was found.");

        public IResult<ParseResult, SyntaxError> Parse(string line, int lineNum)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            if (_foundStepsStart)
            {
                throw new InvalidOperationException(
                    "Bug: Tried to parse description after it was found.");
            }

            if (line.Trim().Equals("== Steps =="))
            {
                _foundStepsStart = true;
                return ParseResult.Done.ToOk<ParseResult, SyntaxError>();
            }

            _text.Append(line).Append('\n');

            return ParseResult.Continue.ToOk<ParseResult, SyntaxError>();
        }
    }

    class StepsParser
    {
        enum StepState
        {
            Error,
            None,
            Active,
            Completed
        }

        private StringBuilder _text = new();
        private StepState _currentState = StepState.Error;

        public List<TodoStep> Steps { get; } = new();

        public void Complete()
        {
            if (_text.Length <= 0) return;
            AddStep();
        }

        public IOption<SyntaxError> Parse(string line, int lineNum)
        {
            const string stepFormat = "- [";

            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            if (line.TrimStart().StartsWith(stepFormat))
            {
                if (!line.Contains(']'))
                {
                    return new SyntaxError(
                            $"Step is missing ending bracket for step state '- []': '{line}' on line {lineNum}")
                        .ToSome();
                }

                // new step
                if (_text.Length > 0)
                {
                    AddStep();
                }

                line = line.TrimStart()[stepFormat.Length..];

                var stateStr = line[..line.IndexOf(']')].Trim();
                var stepStr = line[(line.IndexOf(']') + 1)..].TrimStart();

                if (string.IsNullOrWhiteSpace(stepStr))
                {
                    return new SyntaxError(
                            $"Step has to contain non-whitespace characters: '{line}' on line {lineNum}")
                        .ToSome();
                }

                StepState state;
                switch (stateStr)
                {
                    case "":
                        state = StepState.None;
                        break;
                    case "o":
                    case "O":
                        state = StepState.Active;
                        break;
                    case "X":
                    case "x":
                        state = StepState.Completed;
                        break;
                    default:
                        return new SyntaxError(
                                $"Step has invalid state '{stateStr}' (valid states are ' ', 'x', 'o'): {line} on line {lineNum}")
                            .ToSome();
                }

                _currentState = state;
                _text.Append(stepStr).Append('\n');
            }
            else
            {
                if (_currentState == StepState.Error)
                {
                    return new SyntaxError(
                        $"Steps have to start with the step state '- [ ]': '{line}' on line {lineNum}.").ToSome();
                }
                else
                {
                    _text.Append(
                        line.TrimStart().StartsWith("\\- [")
                            ? line.Replace("\\- [", stepFormat)
                            : line).Append('\n');
                }
            }

            return Option.None<SyntaxError>();
        }

        private void AddStep()
        {
            Steps.Add(new TodoStep
            {
                Active = _currentState == StepState.Active,
                Completed = _currentState == StepState.Completed,
                Order = Steps.Count + 1,
                Text = _text.ToString().Trim()
            });
            _text.Clear();
            _currentState = StepState.Error;
        }
    }
}