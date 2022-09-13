using System.Text;
using Xunit;

namespace TodoHD.Tests;

public class TodoSerializerTests
{
    [Fact]
    public void Serialize()
    {
        // Arrange
        var todo = new TodoItem(1, 1, "hello title world", "this is some cool description\nfor sure", "",
            Priority.Urgent);
        todo.Steps.Add(new TodoStep
        {
            Active = true,
            Text = "step 1 active",
            Order = 2,
        });
        todo.Steps.Add(new TodoStep
        {
            Completed = true,
            Text = "step 2 completed",
            Order = 1,
        });
        todo.Steps.Add(new TodoStep
        {
            Text = "step 3 inactive",
            Order = 4,
        });
        // Act
        var result = TodoSerializer.Serialize(todo);
        // Assert
        var sb = new StringBuilder();
        sb.Append("== hello title world ==").Append('\n');
        sb.Append("this is some cool description").Append('\n');
        sb.Append("for sure").Append('\n');
        sb.Append("== Steps ==").Append('\n');
        sb.Append("- [x] step 2 completed").Append('\n');
        sb.Append("- [o] step 1 active").Append('\n');
        sb.Append("- [ ] step 3 inactive").Append('\n');
        Assert.Equal(sb.ToString(), result);
    }

    [Fact]
    public void Deserialize()
    {
        // Arrange
        var todo =
            @"== HeaderHere == true ==
Description here
 and here
== Steps ==
- [ ] Item1
- [ ] Item2
- [ ] Item3
- [o] ItemActive
- [x] ItemComplete
";

        // Act
        Assert.True(TodoSerializer.Deserialize(todo).IsOk(out var result));

        // Assert
        Assert.Equal("HeaderHere == true", result.Title);
        Assert.Equal("Description here\n and here\n", result.Description);
        Assert.NotEmpty(result.Steps);
        Assert.False(result.Steps[0].Active);
        Assert.False(result.Steps[0].Completed);
        Assert.Equal("Item1", result.Steps[0].Text);
        Assert.True(result.Steps[3].Active);
        Assert.Equal("ItemActive", result.Steps[3].Text);
        Assert.True(result.Steps[4].Completed);
        Assert.Equal("ItemComplete", result.Steps[4].Text);
    }
}