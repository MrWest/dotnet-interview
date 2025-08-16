using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using TodoApi.Data;
using TodoApi.ExternalApi;
using TodoApi.ExternalApi.Models;
using TodoApi.Models;
using TodoApi.Synchronization;
using TodoApi.Synchronization.Models;

namespace TodoApi.Tests.Synchronization
{
    /// <summary>
    /// Comprehensive unit tests for the SynchronizationService.
    /// Tests cover all major scenarios including success cases, error handling, and conflict resolution.
    /// </summary>
    public class SynchronizationServiceTests : IDisposable
    {
        private readonly TodoContext _dbContext;
        private readonly Mock<IExternalTodoApiClient> _mockExternalApiClient;
        private readonly Mock<ILogger<SynchronizationService>> _mockLogger;
        private readonly SynchronizationService _synchronizationService;

        public SynchronizationServiceTests()
        {
            // Setup in-memory database for testing
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _dbContext = new TodoContext(options);
            _mockExternalApiClient = new Mock<IExternalTodoApiClient>();
            _mockLogger = new Mock<ILogger<SynchronizationService>>();
            
            _synchronizationService = new SynchronizationService(
                _dbContext,
                _mockExternalApiClient.Object,
                _mockLogger.Object);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        #region Pull Tests

        [Fact]
        public async Task PullFromExternalAsync_WithNewExternalTodoList_CreatesLocalTodoList()
        {
            // Arrange
            var externalTodoList = new ExternalTodoList
            {
                Id = 1,
                Name = "External Test List",
                SourceId = "external-system",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-1),
                TodoItems = new List<ExternalTodoItem>
                {
                    new ExternalTodoItem
                    {
                        Id = 1,
                        Name = "External Test Item",
                        Description = "Test Description",
                        Completed = false,
                        SourceId = "external-system",
                        CreatedAt = DateTime.UtcNow.AddHours(-1),
                        UpdatedAt = DateTime.UtcNow.AddHours(-1)
                    }
                }
            };

            _mockExternalApiClient
                .Setup(x => x.GetAllTodoListsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ExternalTodoList> { externalTodoList });

            // Act
            var result = await _synchronizationService.PullFromExternalAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Statistics.TodoListsPulled);
            Assert.Equal(1, result.Statistics.TodoItemsPulled);

            var localTodoList = await _dbContext.TodoList
                .Include(tl => tl.Items)
                .FirstOrDefaultAsync();

            Assert.NotNull(localTodoList);
            Assert.Equal("External Test List", localTodoList.Name);
            Assert.Equal("1", localTodoList.ExternalId);
            Assert.True(localTodoList.IsSynced);
            Assert.Single(localTodoList.Items);

            var localItem = localTodoList.Items.First();
            Assert.Equal("External Test Item", localItem.Name);
            Assert.Equal("Test Description", localItem.Description);
            Assert.False(localItem.Completed);
            Assert.True(localItem.IsSynced);
        }

        [Fact]
        public async Task PullFromExternalAsync_WithExistingLocalTodoList_UpdatesWhenExternalIsNewer()
        {
            // Arrange
            var baseTime = DateTime.UtcNow.AddHours(-2);
            
            // Create existing local TodoList
            var localTodoList = new TodoList
            {
                Name = "Old Local Name",
                ExternalId = "1",
                SourceId = "local-system",
                CreatedAt = baseTime,
                UpdatedAt = baseTime,
                LastSyncedAt = baseTime,
                IsSynced = true
            };
            
            _dbContext.TodoList.Add(localTodoList);
            await _dbContext.SaveChangesAsync();

            // External TodoList with newer timestamp
            var externalTodoList = new ExternalTodoList
            {
                Id = 1,
                Name = "Updated External Name",
                SourceId = "external-system",
                CreatedAt = baseTime,
                UpdatedAt = DateTime.UtcNow, // Newer than local
                TodoItems = new List<ExternalTodoItem>()
            };

            _mockExternalApiClient
                .Setup(x => x.GetAllTodoListsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ExternalTodoList> { externalTodoList });

            // Act
            var result = await _synchronizationService.PullFromExternalAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Statistics.TodoListsPulled);

            var updatedLocalList = await _dbContext.TodoList.FirstOrDefaultAsync();
            Assert.NotNull(updatedLocalList);
            Assert.Equal("Updated External Name", updatedLocalList.Name);
            Assert.True(updatedLocalList.UpdatedAt > baseTime);
        }

        [Fact]
        public async Task PullFromExternalAsync_WithConflict_ResolvesWithExternalWins()
        {
            // Arrange
            var baseTime = DateTime.UtcNow.AddHours(-2);
            var localUpdateTime = DateTime.UtcNow.AddMinutes(-30);
            var externalUpdateTime = DateTime.UtcNow.AddMinutes(-15);
            
            // Create local TodoList that was modified after last sync (conflict scenario)
            var localTodoList = new TodoList
            {
                Name = "Local Modified Name",
                ExternalId = "1",
                SourceId = "local-system",
                CreatedAt = baseTime,
                UpdatedAt = localUpdateTime, // Modified after last sync
                LastSyncedAt = baseTime, // Last sync was before local modification
                IsSynced = true
            };
            
            _dbContext.TodoList.Add(localTodoList);
            await _dbContext.SaveChangesAsync();

            // External TodoList also modified (newer than local)
            var externalTodoList = new ExternalTodoList
            {
                Id = 1,
                Name = "External Modified Name",
                SourceId = "external-system",
                CreatedAt = baseTime,
                UpdatedAt = externalUpdateTime, // Newer than local
                TodoItems = new List<ExternalTodoItem>()
            };

            _mockExternalApiClient
                .Setup(x => x.GetAllTodoListsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ExternalTodoList> { externalTodoList });

            // Act
            var result = await _synchronizationService.PullFromExternalAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.ResolvedConflicts);
            
            var conflict = result.ResolvedConflicts.First();
            Assert.Equal("TodoList", conflict.EntityType);
            Assert.Contains("External version takes precedence", conflict.Resolution);

            var updatedLocalList = await _dbContext.TodoList.FirstOrDefaultAsync();
            Assert.Equal("External Modified Name", updatedLocalList.Name); // External wins
        }

        [Fact]
        public async Task PullFromExternalAsync_WithExternalApiError_ReturnsFailureResult()
        {
            // Arrange
            _mockExternalApiClient
                .Setup(x => x.GetAllTodoListsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("External API unavailable"));

            // Act
            var result = await _synchronizationService.PullFromExternalAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Pull operation failed", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("System", result.Errors.First().EntityType);
        }

        #endregion

        #region Push Tests

        [Fact]
        public async Task PushToExternalAsync_WithNewLocalTodoList_CreatesExternalTodoList()
        {
            // Arrange
            var localTodoList = new TodoList
            {
                Name = "New Local List",
                SourceId = "local-system",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsSynced = false, // Needs syncing
                Items = new List<TodoListItem>
                {
                    new TodoListItem
                    {
                        Name = "Local Item",
                        Description = "Local Description",
                        Completed = false,
                        Progress = 50,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsSynced = false
                    }
                }
            };

            _dbContext.TodoList.Add(localTodoList);
            await _dbContext.SaveChangesAsync();

            var createdExternalList = new ExternalTodoList
            {
                Id = 100,
                Name = "New Local List",
                SourceId = "local-todo-api",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TodoItems = new List<ExternalTodoItem>
                {
                    new ExternalTodoItem
                    {
                        Id = 200,
                        Name = "Local Item",
                        Description = "Local Description",
                        Completed = false,
                        SourceId = "local-todo-api",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                }
            };

            _mockExternalApiClient
                .Setup(x => x.CreateTodoListAsync(It.IsAny<CreateTodoListRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdExternalList);

            // Act
            var result = await _synchronizationService.PushToExternalAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Statistics.TodoListsPushed);
            Assert.Equal(1, result.Statistics.TodoItemsPushed);

            var updatedLocalList = await _dbContext.TodoList
                .Include(tl => tl.Items)
                .FirstOrDefaultAsync();

            Assert.NotNull(updatedLocalList);
            Assert.Equal("100", updatedLocalList.ExternalId);
            Assert.True(updatedLocalList.IsSynced);

            var updatedLocalItem = updatedLocalList.Items.First();
            Assert.Equal("200", updatedLocalItem.ExternalId);
            Assert.True(updatedLocalItem.IsSynced);
        }

        [Fact]
        public async Task PushToExternalAsync_WithExistingExternalTodoList_UpdatesExternalTodoList()
        {
            // Arrange
            var localTodoList = new TodoList
            {
                Name = "Updated Local List",
                ExternalId = "100", // Already has external ID
                SourceId = "local-system",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                UpdatedAt = DateTime.UtcNow,
                IsSynced = false, // Needs syncing
                Items = new List<TodoListItem>()
            };

            _dbContext.TodoList.Add(localTodoList);
            await _dbContext.SaveChangesAsync();

            _mockExternalApiClient
                .Setup(x => x.UpdateTodoListAsync(100, It.IsAny<UpdateTodoListRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _synchronizationService.PushToExternalAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Statistics.TodoListsPushed);

            var updatedLocalList = await _dbContext.TodoList.FirstOrDefaultAsync();
            Assert.True(updatedLocalList.IsSynced);
            Assert.NotNull(updatedLocalList.LastSyncedAt);

            _mockExternalApiClient.Verify(
                x => x.UpdateTodoListAsync(100, It.Is<UpdateTodoListRequest>(req => req.Name == "Updated Local List"), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task PushToExternalAsync_WithExternalApiError_ReturnsPartialFailure()
        {
            // Arrange
            var localTodoList = new TodoList
            {
                Name = "Test List",
                SourceId = "local-system",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsSynced = false
            };

            _dbContext.TodoList.Add(localTodoList);
            await _dbContext.SaveChangesAsync();

            _mockExternalApiClient
                .Setup(x => x.CreateTodoListAsync(It.IsAny<CreateTodoListRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("External API error"));

            // Act
            var result = await _synchronizationService.PushToExternalAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Single(result.Errors);
            Assert.Equal("TodoList", result.Errors.First().EntityType);
            Assert.Contains("External API error", result.Errors.First().Message);
        }

        #endregion

        #region Bidirectional Sync Tests

        [Fact]
        public async Task SynchronizeAsync_WithBothPullAndPushNeeded_ExecutesBothPhases()
        {
            // Arrange
            // Setup external data to pull
            var externalTodoList = new ExternalTodoList
            {
                Id = 1,
                Name = "External List",
                SourceId = "external-system",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-1),
                TodoItems = new List<ExternalTodoItem>()
            };

            _mockExternalApiClient
                .Setup(x => x.GetAllTodoListsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ExternalTodoList> { externalTodoList });

            // Setup local data to push
            var localTodoList = new TodoList
            {
                Name = "Local List",
                SourceId = "local-system",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsSynced = false
            };

            _dbContext.TodoList.Add(localTodoList);
            await _dbContext.SaveChangesAsync();

            var createdExternalList = new ExternalTodoList
            {
                Id = 100,
                Name = "Local List",
                SourceId = "local-todo-api",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TodoItems = new List<ExternalTodoItem>()
            };

            _mockExternalApiClient
                .Setup(x => x.CreateTodoListAsync(It.IsAny<CreateTodoListRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdExternalList);

            // Act
            var result = await _synchronizationService.SynchronizeAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Statistics.TodoListsPulled); // From external
            Assert.Equal(1, result.Statistics.TodoListsPushed); // To external
            Assert.Contains("Bidirectional synchronization completed successfully", result.Message);

            // Verify both TodoLists exist locally
            var localTodoLists = await _dbContext.TodoList.ToListAsync();
            Assert.Equal(2, localTodoLists.Count);
        }

        [Fact]
        public async Task SynchronizeAsync_WithConcurrentAccess_ReturnsInProgressError()
        {
            // Arrange
            _mockExternalApiClient
                .Setup(x => x.GetAllTodoListsAsync(It.IsAny<CancellationToken>()))
                .Returns(async (CancellationToken ct) =>
                {
                    // Simulate long-running operation
                    await Task.Delay(1000, ct);
                    return new List<ExternalTodoList>();
                });

            // Act
            var task1 = _synchronizationService.SynchronizeAsync();
            var task2 = _synchronizationService.SynchronizeAsync(); // Should fail immediately

            var results = await Task.WhenAll(task1, task2);

            // Assert
            var successCount = results.Count(r => r.IsSuccess);
            var failureCount = results.Count(r => !r.IsSuccess);

            Assert.Equal(1, successCount);
            Assert.Equal(1, failureCount);

            var failedResult = results.First(r => !r.IsSuccess);
            Assert.Contains("Another synchronization operation is already in progress", failedResult.Message);
        }

        #endregion

        #region Status Tests

        [Fact]
        public async Task GetLastSyncStatusAsync_WithPendingEntities_ReturnsCorrectCount()
        {
            // Arrange
            var syncedTodoList = new TodoList
            {
                Name = "Synced List",
                IsSynced = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var unsyncedTodoList = new TodoList
            {
                Name = "Unsynced List",
                IsSynced = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<TodoListItem>
                {
                    new TodoListItem
                    {
                        Name = "Unsynced Item",
                        IsSynced = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                }
            };

            _dbContext.TodoList.AddRange(syncedTodoList, unsyncedTodoList);
            await _dbContext.SaveChangesAsync();

            // Act
            var status = await _synchronizationService.GetLastSyncStatusAsync();

            // Assert
            Assert.Equal(2, status.PendingSyncCount); // 1 TodoList + 1 TodoItem
            Assert.Contains("2 entities pending", status.StatusMessage);
            Assert.False(status.IsSyncInProgress);
        }

        #endregion

        #region Edge Cases and Error Handling

        [Fact]
        public async Task PullFromExternalAsync_WithEmptyExternalResponse_ReturnsSuccessWithZeroStats()
        {
            // Arrange
            _mockExternalApiClient
                .Setup(x => x.GetAllTodoListsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ExternalTodoList>());

            // Act
            var result = await _synchronizationService.PullFromExternalAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Statistics.TodoListsPulled);
            Assert.Equal(0, result.Statistics.TodoItemsPulled);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task PushToExternalAsync_WithNoLocalChanges_ReturnsSuccessWithZeroStats()
        {
            // Arrange
            var syncedTodoList = new TodoList
            {
                Name = "Already Synced",
                IsSynced = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.TodoList.Add(syncedTodoList);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _synchronizationService.PushToExternalAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Statistics.TodoListsPushed);
            Assert.Equal(0, result.Statistics.TodoItemsPushed);
        }

        [Fact]
        public async Task ProcessExternalTodoItem_WithNullDescription_HandlesGracefully()
        {
            // Arrange
            var externalTodoList = new ExternalTodoList
            {
                Id = 1,
                Name = "Test List",
                SourceId = "external-system",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TodoItems = new List<ExternalTodoItem>
                {
                    new ExternalTodoItem
                    {
                        Id = 1,
                        Name = "Test Item",
                        Description = null, // Null description
                        Completed = false,
                        SourceId = "external-system",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                }
            };

            _mockExternalApiClient
                .Setup(x => x.GetAllTodoListsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ExternalTodoList> { externalTodoList });

            // Act
            var result = await _synchronizationService.PullFromExternalAsync();

            // Assert
            Assert.True(result.IsSuccess);
            
            var localItem = await _dbContext.TodoListItem.FirstOrDefaultAsync();
            Assert.NotNull(localItem);
            Assert.Equal(string.Empty, localItem.Description); // Should default to empty string
        }

        #endregion
    }
}

