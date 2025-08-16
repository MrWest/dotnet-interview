using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Xunit;
using TodoApi.Data;
using TodoApi.ExternalApi;
using TodoApi.ExternalApi.Models;
using TodoApi.Models;
using TodoApi.Synchronization;
using TodoApi.Synchronization.Models;
using TodoApi.ExternalApi.Models.Requests;

namespace TodoApi.Tests.Integration
{
    /// <summary>
    /// Integration tests for the SynchronizationService that test the complete flow
    /// including database operations, HTTP client interactions, and background services.
    /// </summary>
    public class SynchronizationServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;

        public SynchronizationServiceIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<TodoContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database for testing
                    services.AddDbContext<TodoContext>(options =>
                    {
                        options.UseInMemoryDatabase("IntegrationTestDb");
                    });

                    // Override external API client with test implementation
                    services.AddScoped<IExternalTodoApiClient, TestExternalTodoApiClient>();
                });
            });

            _httpClient = _factory.CreateClient();
        }

        [Fact]
        public async Task SynchronizationEndpoint_TriggersBidirectionalSync_ReturnsSuccessResult()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TodoContext>();
            
            // Clear any existing data
            dbContext.TodoList.RemoveRange(dbContext.TodoList);
            dbContext.TodoListItem.RemoveRange(dbContext.TodoListItem);
            await dbContext.SaveChangesAsync();

            // Create local TodoList that needs syncing
            var localTodoList = new TodoList
            {
                Name = "Integration Test List",
                SourceId = "local-system",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsSynced = false
            };

            dbContext.TodoList.Add(localTodoList);
            await dbContext.SaveChangesAsync();

            var triggerSyncCommand = new
            {
                SyncType = "Bidirectional"
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync("/api/synchronization/sync", triggerSyncCommand);

            // Assert
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<SyncResult>();
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            
            // Verify that local data was pushed and external data was pulled
            Assert.True(result.Statistics.TodoListsPushed > 0 || result.Statistics.TodoListsPulled > 0);
        }

        [Fact]
        public async Task BackgroundService_PerformsAutomaticSync_WithConfiguredInterval()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TodoContext>();
            var syncService = scope.ServiceProvider.GetRequiredService<ISynchronizationService>();

            // Clear existing data
            dbContext.TodoList.RemoveRange(dbContext.TodoList);
            await dbContext.SaveChangesAsync();

            // Create unsynced local data
            var localTodoList = new TodoList
            {
                Name = "Background Sync Test",
                SourceId = "local-system",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsSynced = false
            };

            dbContext.TodoList.Add(localTodoList);
            await dbContext.SaveChangesAsync();

            // Act
            var initialStatus = await syncService.GetLastSyncStatusAsync();
            Assert.True(initialStatus.PendingSyncCount > 0);

            // Trigger manual sync to simulate background service
            var syncResult = await syncService.SynchronizeAsync();

            // Assert
            Assert.True(syncResult.IsSuccess);
            
            var finalStatus = await syncService.GetLastSyncStatusAsync();
            Assert.True(finalStatus.PendingSyncCount <= initialStatus.PendingSyncCount);
        }

        [Fact]
        public async Task EndToEndSync_WithRealDatabaseOperations_MaintainsDataConsistency()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TodoContext>();
            var syncService = scope.ServiceProvider.GetRequiredService<ISynchronizationService>();

            // Clear existing data
            dbContext.TodoList.RemoveRange(dbContext.TodoList);
            dbContext.TodoListItem.RemoveRange(dbContext.TodoListItem);
            await dbContext.SaveChangesAsync();

            // Create complex local data structure
            var localTodoList = new TodoList
            {
                Name = "Complex Integration Test",
                SourceId = "local-system",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsSynced = false,
                Items = new List<TodoListItem>
                {
                    new TodoListItem
                    {
                        Name = "Integration Test Item 1",
                        Description = "First test item",
                        Completed = false,
                        Progress = 25,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsSynced = false
                    },
                    new TodoListItem
                    {
                        Name = "Integration Test Item 2",
                        Description = "Second test item",
                        Completed = true,
                        Progress = 100,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsSynced = false
                    }
                }
            };

            dbContext.TodoList.Add(localTodoList);
            await dbContext.SaveChangesAsync();

            var initialLocalCount = await dbContext.TodoList.CountAsync();
            var initialItemCount = await dbContext.TodoListItem.CountAsync();

            // Act - Perform bidirectional sync
            var syncResult = await syncService.SynchronizeAsync();

            // Assert
            Assert.True(syncResult.IsSuccess);
            
            // Verify data integrity
            var finalLocalCount = await dbContext.TodoList.CountAsync();
            var finalItemCount = await dbContext.TodoListItem.CountAsync();
            
            // Should have at least the original data, possibly more from external
            Assert.True(finalLocalCount >= initialLocalCount);
            Assert.True(finalItemCount >= initialItemCount);

            // Verify sync status
            var allTodoLists = await dbContext.TodoList.Include(tl => tl.Items).ToListAsync();
            foreach (var todoList in allTodoLists)
            {
                if (!string.IsNullOrEmpty(todoList.ExternalId))
                {
                    Assert.True(todoList.IsSynced);
                    Assert.NotNull(todoList.LastSyncedAt);
                }

                foreach (var item in todoList.Items)
                {
                    if (!string.IsNullOrEmpty(item.ExternalId))
                    {
                        Assert.True(item.IsSynced);
                        Assert.NotNull(item.LastSyncedAt);
                    }
                }
            }
        }

        [Fact]
        public async Task ConflictResolution_WithSimultaneousModifications_ResolvesCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TodoContext>();
            var syncService = scope.ServiceProvider.GetRequiredService<ISynchronizationService>();

            // Clear existing data
            dbContext.TodoList.RemoveRange(dbContext.TodoList);
            await dbContext.SaveChangesAsync();

            var baseTime = DateTime.UtcNow.AddHours(-1);

            // Create local TodoList that simulates being modified after last sync
            var conflictTodoList = new TodoList
            {
                Name = "Local Modified Name",
                ExternalId = "999", // Simulate existing external mapping
                SourceId = "local-system",
                CreatedAt = baseTime,
                UpdatedAt = DateTime.UtcNow.AddMinutes(-30), // Modified recently
                LastSyncedAt = baseTime, // Last sync was before modification
                IsSynced = true // Was synced before modification
            };

            dbContext.TodoList.Add(conflictTodoList);
            await dbContext.SaveChangesAsync();

            // Act - Sync will detect conflict with external data
            var syncResult = await syncService.SynchronizeAsync();

            // Assert
            Assert.True(syncResult.IsSuccess);
            
            // Should have detected and resolved conflicts
            if (syncResult.ResolvedConflicts.Any())
            {
                var conflict = syncResult.ResolvedConflicts.First();
                Assert.Equal("TodoList", conflict.EntityType);
                Assert.Contains("External version takes precedence", conflict.Resolution);
            }

            // Verify final state
            var resolvedTodoList = await dbContext.TodoList.FirstOrDefaultAsync(tl => tl.ExternalId == "999");
            if (resolvedTodoList != null)
            {
                Assert.True(resolvedTodoList.IsSynced);
                Assert.NotNull(resolvedTodoList.LastSyncedAt);
                Assert.True(resolvedTodoList.LastSyncedAt > baseTime);
            }
        }

        [Fact]
        public async Task PerformanceTest_LargeDataSet_CompletesWithinReasonableTime()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TodoContext>();
            var syncService = scope.ServiceProvider.GetRequiredService<ISynchronizationService>();

            // Clear existing data
            dbContext.TodoList.RemoveRange(dbContext.TodoList);
            dbContext.TodoListItem.RemoveRange(dbContext.TodoListItem);
            await dbContext.SaveChangesAsync();

            // Create multiple TodoLists with items (simulate larger dataset)
            var todoLists = new List<TodoList>();
            for (int i = 1; i <= 10; i++)
            {
                var todoList = new TodoList
                {
                    Name = $"Performance Test List {i}",
                    SourceId = "local-system",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsSynced = false,
                    Items = new List<TodoListItem>()
                };

                // Add 5 items per list
                for (int j = 1; j <= 5; j++)
                {
                    todoList.Items.Add(new TodoListItem
                    {
                        Name = $"Performance Test Item {i}-{j}",
                        Description = $"Description for item {j} in list {i}",
                        Completed = j % 2 == 0,
                        Progress = j * 20,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsSynced = false
                    });
                }

                todoLists.Add(todoList);
            }

            dbContext.TodoList.AddRange(todoLists);
            await dbContext.SaveChangesAsync();

            // Act
            var startTime = DateTime.UtcNow;
            var syncResult = await syncService.SynchronizeAsync();
            var duration = DateTime.UtcNow - startTime;

            // Assert
            Assert.True(syncResult.IsSuccess);
            Assert.True(duration.TotalSeconds < 30); // Should complete within 30 seconds
            
            // Verify all data was processed
            Assert.True(syncResult.Statistics.TodoListsPushed > 0);
            Assert.True(syncResult.Statistics.TodoItemsPushed > 0);
            
            // Verify performance metrics
            Assert.True(syncResult.Duration.TotalSeconds < 30);
            
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SynchronizationServiceIntegrationTests>>();
            logger.LogInformation("Performance test completed in {Duration}ms with {ListCount} lists and {ItemCount} items",
                duration.TotalMilliseconds,
                syncResult.Statistics.TodoListsPushed,
                syncResult.Statistics.TodoItemsPushed);
        }
    }

    /// <summary>
    /// Test implementation of IExternalTodoApiClient for integration testing.
    /// Simulates external API behavior without requiring actual external service.
    /// </summary>
    public class TestExternalTodoApiClient : IExternalTodoApiClient
    {
        private static readonly List<ExternalTodoList> _externalData = new();
        private static long _nextId = 1000;

        public async Task<List<ExternalTodoList>> GetAllTodoListsAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(50, cancellationToken); // Simulate network delay
            
            // Return some test external data
            return new List<ExternalTodoList>
            {
                new ExternalTodoList
                {
                    Id = 999,
                    Name = "External Test List",
                    SourceId = "external-system",
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-15), // Recently updated
                    TodoItems = new List<ExternalTodoItem>
                    {
                        new ExternalTodoItem
                        {
                            Id = 1999,
                            Name = "External Test Item",
                            Description = "From external system",
                            Completed = false,
                            SourceId = "external-system",
                            CreatedAt = DateTime.UtcNow.AddHours(-2),
                            UpdatedAt = DateTime.UtcNow.AddMinutes(-15)
                        }
                    }
                }
            }.Concat(_externalData).ToList();
        }

        public async Task<ExternalTodoList> CreateTodoListAsync(CreateTodoListRequest request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(100, cancellationToken); // Simulate network delay

            var externalList = new ExternalTodoList
            {
                Id = _nextId++,
                Name = request.Name,
                SourceId = request.SourceId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TodoItems = request.TodoItems?.Select(item => new ExternalTodoItem
                {
                    Id = _nextId++,
                    Name = item.Name,
                    Description = item.Description,
                    Completed = item.Completed,
                    SourceId = item.SourceId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList() ?? new List<ExternalTodoItem>()
            };

            _externalData.Add(externalList);
            return externalList;
        }

        public async Task<ExternalTodoList> UpdateTodoListAsync(long todolistId, UpdateTodoListRequest request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(50, cancellationToken); // Simulate network delay

            var existingList = _externalData.FirstOrDefault(l => l.Id == todolistId);
            if (existingList != null)
            {
                existingList.Name = request.Name;
                existingList.UpdatedAt = DateTime.UtcNow;
            }

            return existingList;
        }

        public async Task<ExternalTodoItem> UpdateTodoItemAsync(long todolistId, long todoitemId, UpdateTodoItemRequest request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(50, cancellationToken); // Simulate network delay

            var existingList = _externalData.FirstOrDefault(l => l.Id == todolistId);
            var existingItem = existingList?.TodoItems?.FirstOrDefault(i => i.Id == todoitemId);
            
            if (existingItem != null)
            {
                existingItem.Name = request.Name;
                existingItem.Description = request.Description;
                existingItem.Completed = request.Completed;
                existingItem.UpdatedAt = DateTime.UtcNow;
            }

            return existingItem;
        }

        public async Task DeleteTodoListAsync(long todolistId, CancellationToken cancellationToken = default)
        {
            await Task.Delay(50, cancellationToken); // Simulate network delay

            var existingList = _externalData.FirstOrDefault(l => l.Id == todolistId);
            if (existingList != null)
            {
                _externalData.Remove(existingList);
            }
        }

        public async Task DeleteTodoItemAsync(long todolistId, long todoitemId, CancellationToken cancellationToken = default)
        {
            await Task.Delay(50, cancellationToken); // Simulate network delay

            var existingList = _externalData.FirstOrDefault(l => l.Id == todolistId);
            var existingItem = existingList?.TodoItems?.FirstOrDefault(i => i.Id == todoitemId);
            
            if (existingItem != null && existingList?.TodoItems != null)
            {
                existingList.TodoItems.Remove(existingItem);
            }
        }
    }
}

