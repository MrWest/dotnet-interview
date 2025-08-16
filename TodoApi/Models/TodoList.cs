namespace TodoApi.Models;

public class TodoList : SyncItem
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public ICollection<TodoListItem> Items { get; set; } = new List<TodoListItem>();
    public override void MarkAsModified()
    {
        base.MarkAsModified();
        // Note: Items will be handled separately in the sync process
    }
}
