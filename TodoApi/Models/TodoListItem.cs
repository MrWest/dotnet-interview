namespace TodoApi.Models;

public class TodoListItem: SyncItem
{
    public long Id { get; set; }
    public long TodoListId { get; set; }
    public TodoList? TodoList { get; set; }
    public required string Name { get; set; }
    public  bool Completed { get; set; } = false;
     public  int Progress { get; set; } = 0;
    public required string Description { get; set; }
}
