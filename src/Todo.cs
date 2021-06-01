using System;
using System.Collections.Generic;

namespace TodoHD
{
	public enum Priority
	{
		Whenever,
		Urgent
	}

	public class TodoStep
	{
		public int Order {get;set;}
		public string Text {get;set;}
		public bool Completed {get;set;}
	}

	public class TodoItem
	{
		public TodoItem() { }
		public TodoItem(int id, int order, string title, string description, string category, Priority priority)
		{
			Id = id;
			Title = title;
			Description = description;
			Category = category;
			Priority = priority;
		}
		public int Id {get;set;}
		public int Order {get;set;}
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
}
