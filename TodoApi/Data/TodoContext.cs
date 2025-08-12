using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

public class TodoContext : DbContext
{
    public TodoContext(DbContextOptions<TodoContext> options)
        : base(options) { }

    public DbSet<TodoList> TodoList { get; set; } = default!;
    public DbSet<TodoListItem> TodoListItem { get; set; } = default!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure TodoListItem with single primary key (much simpler!)
        modelBuilder.Entity<TodoListItem>()
            .HasKey(item => item.Id); // Single auto-increment primary key

        // Configure the relationship
        modelBuilder.Entity<TodoListItem>()
            .HasOne(item => item.TodoList)
            .WithMany(list => list.Items)
            .HasForeignKey(item => item.TodoListId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add indexes for performance
        modelBuilder.Entity<TodoListItem>()
            .HasIndex(item => item.TodoListId);
    }


}
