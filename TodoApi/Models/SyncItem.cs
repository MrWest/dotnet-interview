namespace TodoApi.Models;

public abstract class SyncItem
{
    public string? SourceId { get; set; } = null;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSyncedAt { get; set; }
    public string? ExternalId { get; set; }
    public bool IsSynced { get; set; } = false;

    public virtual void MarkAsModified()
    {
        UpdatedAt = DateTime.UtcNow;
        IsSynced = false; // Entity needs to be re-synced
    }
    public virtual void MarkAsSynced()
    {
        LastSyncedAt = DateTime.UtcNow;
        IsSynced = true;
    }
    public virtual bool NeedsSync => !IsSynced || (LastSyncedAt.HasValue && UpdatedAt > LastSyncedAt.Value);

}
