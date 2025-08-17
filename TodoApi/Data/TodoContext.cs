using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options) : base(options) { }

        public DbSet<TodoList> TodoList { get; set; } = default!;
        public DbSet<TodoListItem> TodoListItem { get; set; } = default!;
        private bool _isSyncOperation = false;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TodoList
            modelBuilder.Entity<TodoList>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Sync-related indexes for performance
                entity.HasIndex(e => e.ExternalId);
                entity.HasIndex(e => e.SourceId);
                entity.HasIndex(e => e.UpdatedAt);
                entity.HasIndex(e => e.LastSyncedAt);
                entity.HasIndex(e => e.IsSynced);
                
                // Composite index for sync queries
                entity.HasIndex(e => new { e.IsSynced, e.UpdatedAt });
            });

            // Configure TodoListItem
            modelBuilder.Entity<TodoListItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Relationship configuration
                entity.HasOne(e => e.TodoList)
                      .WithMany(e => e.Items)
                      .HasForeignKey(e => e.TodoListId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Sync-related indexes for performance
                entity.HasIndex(e => e.ExternalId);
                entity.HasIndex(e => e.SourceId);
                entity.HasIndex(e => e.UpdatedAt);
                entity.HasIndex(e => e.LastSyncedAt);
                entity.HasIndex(e => e.IsSynced);
                entity.HasIndex(e => e.TodoListId);
                
                // Composite indexes for sync queries
                entity.HasIndex(e => new { e.IsSynced, e.UpdatedAt });
                entity.HasIndex(e => new { e.TodoListId, e.IsSynced });
            });

            // Configure automatic timestamp updates
            ConfigureTimestamps(modelBuilder);
        }

        /// <summary>
        /// Configures automatic timestamp updates for SyncItem entities.
        /// </summary>
        private void ConfigureTimestamps(ModelBuilder modelBuilder)
        {
            // Configure default values and update behavior for timestamps
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(SyncItem).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(SyncItem.CreatedAt))
                        .HasDefaultValueSql("GETUTCDATE()");
                    
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(SyncItem.UpdatedAt))
                        .HasDefaultValueSql("GETUTCDATE()");
                }
            }
        }

        /// <summary>
        /// Override SaveChanges to automatically update timestamps.
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically update timestamps.
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Automatically updates timestamps for modified SyncItem entities.
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<SyncItem>();
            
            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        if (!_isSyncOperation) // makes no practical sense but To use in tests
                        {
                            entry.Entity.UpdatedAt = DateTime.UtcNow;
                        }
                        break;
                    
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        if (!_isSyncOperation) // Respeta modo sync
                        {
                            entry.Entity.IsSynced = false;
                        }
                        break;
                }
            }
        }

        public void SetSyncMode(bool isSyncMode)
        {
            _isSyncOperation = isSyncMode;
        }

    }
}