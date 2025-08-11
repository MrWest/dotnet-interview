namespace TodoApi.Models;

public class TodoListItem
{
    public long Id { get; set; }

    public TodoList? TodoList { get; set; }
    public required string Name { get; set; }

    public  bool Completed { get; set; }

     public  int Progress { get; set; }

    public required string Description { get; set; }
}
