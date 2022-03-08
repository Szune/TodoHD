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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace TodoHD
{
    public class Editor
    {
        private readonly string _savePath;
        public Editor(string path)
        {
            _savePath = path;
        }
        Stack<IMode> _modes = new();
        Dictionary<int, TodoItem> _items = new();
        List<string> _categories = new();

        public int NextId => _items.Values.Select(s => s.Id).DefaultIfEmpty(0).Max() + 1;
        public int NextOrder => _items.Values.Select(s => s.Order).DefaultIfEmpty(0).Max() + 1;

        public IEnumerable<TodoItem> GetItems() =>
            _items.Values
                .OrderByDescending(i => (int) i.Priority)
                .ThenBy(i => i.Order);

        public void Load()
        {
            try
            {
                var json = File.ReadAllText(_savePath);
                var deserialized = JsonSerializer.Deserialize<Todo>(json, TodoHdContext.Default.Todo);
                _items = deserialized.Items.ToDictionary(i => i.Id);
                NormalizeItemOrder(_items.Values);
            }
            catch(FileNotFoundException)
            {
                File.WriteAllText(_savePath, JsonSerializer.Serialize<Todo>(new() { Items = new() }, TodoHdContext.Default.Todo));
            }
        }

        private void NormalizeItemOrder(IEnumerable<TodoItem> items)
        {
            items
                .OrderBy(i => i.Order)
                .Select((item,index) => new { item, index })
                .ToList()
                .ForEach(it => {
                        it.item.Order = it.index + 1;
                        it.item
                            .Steps?
                            .OrderBy(i => i.Order)
                            .Select((item,index) => new { item, index })
                            .ToList()
                            .ForEach(step => step.item.Order = step.index + 1);
                    });
        }

        public int ItemsPerPage => Math.Max(1, Console.BufferHeight - 3);
        public int Item {get;private set;} = 0;
        public int MaxItem => ItemsPerPage - 1;
        public int Page {get;private set;} = 0;
        public int MaxPage
        {
            get
            {
                var itemsPerPage = Math.Max(1, ItemsPerPage);
                var pages = _items.Count / itemsPerPage;
                if(pages > 0 &&
                   pages * itemsPerPage >= _items.Count)
                    --pages;

                return pages;
            }
        }
        public int PageDisplay => Page + 1;
        public int MaxPageDisplay => MaxPage + 1;

        public TodoItem GetSelectedItem()
        {
            return GetItems()
                .Skip(ItemsPerPage * Page)
                .Take(ItemsPerPage)
                .Select((item,index) => new{item,index})
                .First(it => Item == it.index)
                .item;
        }

        public IEnumerable<TodoItem> GetItemsByPriority(Priority priority)
        {
            return _items.Values
                .Where(it => it.Priority == priority);
        }
        
        public IEnumerable<string> GetCategories() => _categories;
        public void AddCategory(string name)
        {
            _categories.Add(name);
        }

        public void RenameCategory(string oldName, string newName)
        {
            var index = _categories.FindIndex(n => string.Equals(n, oldName, StringComparison.InvariantCultureIgnoreCase));
            if(index < 0)
            {
                return;
            }

            _categories[index] = newName;

            _items
                .Values
                .ToList()
                .ForEach(i => {
                        if(string.Equals(i.Category, oldName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            i.Category = newName;
                        }
                    });
        }
        
        private void FullUpdateCurrent()
        {
            var current = _modes.Peek();
            Item = ClampItem(Item);
            current.Init(this);
            current.PrintUI(this);
        }
        
        private void PrintCurrent()
        {
            _modes.Peek().PrintUI(this);
        }

        public void PushMode(IMode mode, bool immediate = false)
        {
            _modes.Push(mode);
            _modes.Peek().Init(this);
            PrintCurrent();
            if(immediate)
            {
                _modes.Peek().KeyEvent(new(' ', 0, false, false, false), this);
            }
        }

        public void NextPage()
        {
            Page = Math.Clamp(Page + 1, 0, MaxPage);
            FullUpdateCurrent();
        }

        public void PrevPage()
        {
            Page = Math.Clamp(Page - 1, 0, MaxPage);
            FullUpdateCurrent();
        }

        private static bool MoveUp(IHaveOrder current, IEnumerable<IHaveOrder> items)
        {
            // TODO: might want to use a more efficient data structure,
            // possibly a mix of a hashmap and a sorted list
            // where you can correlate an id with an index
            // and do a couple of if-checks on the previous item
            // instead of iterating through everything every time
            var currentOrder = current.Order;
            var next = items
                .Where(i => i.Order < currentOrder)
                .Aggregate(new { distance = 9999, winner = null as IHaveOrder },
                        (acc, it) => {
                            var    dist = currentOrder - it.Order;
                            if(dist < acc.distance)
                            {
                                return new { distance = dist, winner = it };
                            }
                            return acc;
                        });
            if(next.winner == null)
            {
                return false;
            }
            current.Order = next.winner.Order;
            next.winner.Order = currentOrder;
            return true;
        }


        private static bool MoveDown(IHaveOrder current, IEnumerable<IHaveOrder> items)
        {
            var currentOrder = current.Order;
            var next = items
                .Where(i => i.Order > currentOrder)
                .Aggregate(new { distance = 9999, winner = null as IHaveOrder },
                        (acc, it) => {
                            var    dist = it.Order - currentOrder;
                            if(dist < acc.distance)
                            {
                                return new { distance = dist, winner = it };
                            }
                            return acc;
                        });
            if(next.winner == null)
            {
                return false;
            }
            current.Order = next.winner.Order;
            next.winner.Order = currentOrder;
            return true;
        }

        public bool MoveItemUp()
        {
            var current = GetSelectedItem();
            if(MoveUp(current, GetItemsByPriority(current.Priority)))
            {
                Save();
                PrevItem();
                return true;
            }
            return false;
        }


        public bool MoveItemDown()
        {
            var current = GetSelectedItem();
            if(MoveDown(current, GetItemsByPriority(current.Priority)))
            {
                Save();
                NextItem();
                return true;
            }
            return false;
        }

        // TODO: should probably have an id on the steps as well
        public bool MoveStepUp(IEnumerable<TodoStep> steps, TodoStep item)
        {
            if(steps == null)
            {
                return false;
            }
            if(MoveUp(item, steps))
            {
                Save();
                return true;
            }
            return false;
        }


        public bool MoveStepDown(IEnumerable<TodoStep> steps, TodoStep item)
        {
            if(steps == null)
            {
                return false;
            }
            if(MoveDown(item, steps))
            {
                Save();
                return true;
            }
            return false;
        }

        public void NextItem()
        {
            Item = ClampItem(Item + 1);
            PrintCurrent();
        }

        public void PrevItem()
        {
            Item = ClampItem(Item - 1);
            PrintCurrent();
        }

        public void LastItem()
        {
            Item = MaxItem;
            PrintCurrent();
        }

        public void FirstItem()
        {
            Item = 0;
            PrintCurrent();
        }

        private int ClampItem(int newValue)
        {
            return Math.Clamp(
                    value: newValue,
                    min: 0,
                    max: Math.Max(0, MaxItem));
        }

        public void InsertItem(TodoItem item)
        {
            item.Id = NextId;
            item.Order = NextOrder;
            _items[item.Id] = item;
            PrintCurrent();
        }

        public void DeleteItemById(int id)
        {
            _items.Remove(id);
            Save();
        }

        public void PrintHelpLine(bool clear)
        {
            if(clear)
            {
                Console.Clear();
            }
            Output.WithForeground(ConsoleColor.Green, () => {
                    Console.WriteLine($"Page {PageDisplay}/{MaxPageDisplay} | [I] New item [H] Help [Q] Quit");
                    });
        }



        public void Save()
        {
            var items = new Todo {Items = _items.Values.ToList(), Categories = _categories};
            // TODO: save a couple of backups
            File.WriteAllText(_savePath, JsonSerializer.Serialize<Todo>(items, 
                        TodoHdContext.Default.Todo));
        }

        public void PopMode()
        {
            _modes.Pop();
            FullUpdateCurrent();
        }

        public void Start()
        {
            Console.CursorVisible = false;
            PrintCurrent();
            while(true)
            {
                var key = Console.ReadKey(true);
                if(key.Key == ConsoleKey.Q)
                {
                    _modes.Pop();
                    if(!_modes.Any())
                    {
                        return;
                    }
                    _modes.Peek().Init(this);
                    PrintCurrent();
                }
                else
                {
                    PrintCurrent();
                    _modes.Peek().KeyEvent(key, this);
                }
            }
        }
    }
}
