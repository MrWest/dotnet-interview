using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MediatR;
using Moq;
using TodoApi.Controllers;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Mediation.TodoListItem;
using TodoApi.Mediation.TodoListItem.Dtos;
using TodoApi.Infrastructure;

namespace TodoApi.Tests;

#nullable disable
public class TodoListItemControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly TodoListItemController _controller;

    public TodoListItemControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new TodoListItemController(_mockMediator.Object);
    }

    private DbContextOptions<TodoContext> DatabaseContextOptions()
    {
        return new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private void PopulateDatabaseContext(TodoContext context)
    {
        // Add TodoLists
        context.TodoList.Add(new TodoList { Id = 1, Name = "Work Tasks" });
        context.TodoList.Add(new TodoList { Id = 2, Name = "Personal Tasks" });

        // Add TodoListItems
        context.TodoListItem.Add(new TodoListItem 
        { 
            Id = 1, 
            TodoListId = 1, 
            Name = "Complete project", 
            Description = "Finish the current project",
            Completed = false,
            Progress = 50
        });
        context.TodoListItem.Add(new TodoListItem 
        { 
            Id = 2, 
            TodoListId = 1, 
            Name = "Review code", 
            Description = "Review team's code",
            Completed = true,
            Progress = 100
        });
        context.TodoListItem.Add(new TodoListItem 
        { 
            Id = 3, 
            TodoListId = 2, 
            Name = "Buy groceries", 
            Description = "Weekly grocery shopping",
            Completed = false,
            Progress = 0
        });

        context.SaveChanges();
    }

    #region GetTodoListItems Tests

    [Fact]
    public async Task GetTodoListItems_WhenCalled_ReturnsSuccessResult()
    {
        // Arrange
        var todoListId = 1L;
        var expectedResponse = new GetAllTodoListItemsQueryResponse
        {
            TodoListId = todoListId,
            TotalCount = 2,
            Page = 1,
            PageSize = 50,
            TotalPages = 1,
            Items = new List<TodoListItemDetailResponseDto>
            {
                new TodoListItemDetailResponseDto
                {
                    Id = 1,
                    TodoListId = todoListId,
                    Name = "Complete project",
                    Description = "Finish the current project",
                    Completed = false,
                    Progress = 50
                },
                new TodoListItemDetailResponseDto
                {
                    Id = 2,
                    TodoListId = todoListId,
                    Name = "Review code",
                    Description = "Review team's code",
                    Completed = true,
                    Progress = 100
                }
            }
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllTodoListItemsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<GetAllTodoListItemsQueryResponse>.Success(expectedResponse));

        // Act
        var result = await _controller.GetTodoListItems(todoListId);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as GetAllTodoListItemsQueryResponse;
        Assert.Equal(2, response.Items.Count);
        Assert.Equal(todoListId, response.TodoListId);
    }

    [Fact]
    public async Task GetTodoListItems_WithPagination_PassesCorrectParameters()
    {
        // Arrange
        var todoListId = 1L;
        var page = 2;
        var pageSize = 10;
        var completedFilter = true;
        var includeTodoList = true;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllTodoListItemsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<GetAllTodoListItemsQueryResponse>.Success(new GetAllTodoListItemsQueryResponse()));

        // Act
        await _controller.GetTodoListItems(todoListId, includeTodoList, completedFilter, page, pageSize);

        // Assert
        _mockMediator.Verify(m => m.Send(It.Is<GetAllTodoListItemsQuery>(q =>
            q.TodoListId == todoListId &&
            q.Page == page &&
            q.PageSize == pageSize &&
            q.CompletedFilter == completedFilter &&
            q.IncludeTodoList == includeTodoList
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTodoListItems_WhenTodoListNotFound_ReturnsNotFound()
    {
        // Arrange
        var todoListId = 999L;
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllTodoListItemsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<GetAllTodoListItemsQueryResponse>.NotFound($"TodoList with ID {todoListId} not found"));

        // Act
        var result = await _controller.GetTodoListItems(todoListId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    #endregion

    #region GetTodoListItem Tests

    [Fact]
    public async Task GetTodoListItem_WhenCalled_ReturnsSuccessResult()
    {
        // Arrange
        var todoListId = 1L;
        var itemId = 1L;
        var expectedResponse = new GetTodoListItemQueryResponse
        {
            Item = new TodoListItemDetailResponseDto
            {
                Id = itemId,
                TodoListId = todoListId,
                Name = "Complete project",
                Description = "Finish the current project",
                Completed = false,
                Progress = 50
            }
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetTodoListItemQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<GetTodoListItemQueryResponse>.Success(expectedResponse));

        // Act
        var result = await _controller.GetTodoListItem(todoListId, itemId);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as GetTodoListItemQueryResponse;
        Assert.Equal(itemId, response.Item.Id);
        Assert.Equal(todoListId, response.Item.TodoListId);
    }

    [Fact]
    public async Task GetTodoListItem_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var todoListId = 1L;
        var itemId = 999L;
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetTodoListItemQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<GetTodoListItemQueryResponse>.NotFound($"TodoListItem with ID {itemId} not found in TodoList {todoListId}"));

        // Act
        var result = await _controller.GetTodoListItem(todoListId, itemId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetTodoListItem_PassesCorrectParameters()
    {
        // Arrange
        var todoListId = 1L;
        var itemId = 1L;
        var includeTodoList = true;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetTodoListItemQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<GetTodoListItemQueryResponse>.Success(new GetTodoListItemQueryResponse
            {
                Item = new TodoListItemDetailResponseDto { Id = itemId, TodoListId = todoListId, Name = "Test" }
            }));

        // Act
        await _controller.GetTodoListItem(todoListId, itemId, includeTodoList);

        // Assert
        _mockMediator.Verify(m => m.Send(It.Is<GetTodoListItemQuery>(q =>
            q.TodoListId == todoListId &&
            q.Id == itemId &&
            q.IncludeTodoList == includeTodoList
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region PutTodoListItem Tests

    [Fact]
    public async Task PutTodoListItem_WhenCalled_ReturnsSuccessResult()
    {
        // Arrange
        var todoListId = 1L;
        var itemId = 1L;
        var updateDto = new UpdateTodoListItemDto
        {
            Name = "Updated task",
            Description = "Updated description",
            Completed = true,
            Progress = 100
        };

        var expectedResponse = new UpdateTodoListItemCommandResponse
        {
            Id = itemId,
            TodoListId = todoListId,
            Name = updateDto.Name,
            Description = updateDto.Description,
            Completed = updateDto.Completed,
            Progress = updateDto.Progress,
            UpdatedAt = DateTime.UtcNow
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateTodoListItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<UpdateTodoListItemCommandResponse>.Success(expectedResponse));

        // Act
        var result = await _controller.PutTodoListItem(todoListId, itemId, updateDto);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as UpdateTodoListItemCommandResponse;
        Assert.Equal(updateDto.Name, response.Name);
        Assert.Equal(updateDto.Completed, response.Completed);
    }

    [Fact]
    public async Task PutTodoListItem_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var todoListId = 1L;
        var itemId = 999L;
        var updateDto = new UpdateTodoListItemDto { Name = "Updated task" };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateTodoListItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<UpdateTodoListItemCommandResponse>.NotFound($"TodoListItem with ID {itemId} not found in TodoList {todoListId}"));

        // Act
        var result = await _controller.PutTodoListItem(todoListId, itemId, updateDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task PutTodoListItem_PassesCorrectParameters()
    {
        // Arrange
        var todoListId = 1L;
        var itemId = 1L;
        var updateDto = new UpdateTodoListItemDto
        {
            Name = "Updated task",
            Description = "Updated description",
            Completed = true,
            Progress = 75
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateTodoListItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<UpdateTodoListItemCommandResponse>.Success(new UpdateTodoListItemCommandResponse
            {
                Id = itemId,
                TodoListId = todoListId,
                Name = updateDto.Name,
                Description = updateDto.Description,
                Completed = updateDto.Completed,
                Progress = updateDto.Progress,
                UpdatedAt = DateTime.UtcNow
            }));

        // Act
        await _controller.PutTodoListItem(todoListId, itemId, updateDto);

        // Assert
        _mockMediator.Verify(m => m.Send(It.Is<UpdateTodoListItemCommand>(c =>
            c.TodoListId == todoListId &&
            c.Id == itemId &&
            c.Name == updateDto.Name &&
            c.Description == updateDto.Description &&
            c.Completed == updateDto.Completed &&
            c.Progress == updateDto.Progress
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region PostTodoListItem Tests

    [Fact]
    public async Task PostTodoListItem_WhenCalled_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var todoListId = 1L;
        var createDto = new CreateTodoListItemDto
        {
            Name = "New task",
            Description = "New task description",
            Completed = false,
            Progress = 0
        };

        var expectedResponse = new TodoListItemCreateCommandResponse
        {
            Id = 4,
            TodoListId = todoListId,
            Name = createDto.Name,
            Description = createDto.Description,
            Completed = createDto.Completed,
            Progress = createDto.Progress
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<TodoListItemCreateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TodoListItemCreateCommandResponse>.Success(expectedResponse));

        // Act
        var result = await _controller.PostTodoListItem(todoListId, createDto);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.Equal(nameof(_controller.GetTodoListItem), createdResult.ActionName);
        
        var routeValues = createdResult.RouteValues;
        Assert.Equal(todoListId, routeValues["todoListId"]);
        Assert.Equal(expectedResponse.Id, routeValues["id"]);
        
        var response = createdResult.Value as TodoListItemCreateCommandResponse;
        Assert.Equal(createDto.Name, response.Name);
    }

    [Fact]
    public async Task PostTodoListItem_WhenTodoListNotFound_ReturnsNotFound()
    {
        // Arrange
        var todoListId = 999L;
        var createDto = new CreateTodoListItemDto { Name = "New task" };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<TodoListItemCreateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TodoListItemCreateCommandResponse>.NotFound($"TodoList with ID {todoListId} not found"));

        // Act
        var result = await _controller.PostTodoListItem(todoListId, createDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostTodoListItem_PassesCorrectParameters()
    {
        // Arrange
        var todoListId = 1L;
        var createDto = new CreateTodoListItemDto
        {
            Name = "New task",
            Description = "New task description",
            Completed = false,
            Progress = 25
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<TodoListItemCreateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TodoListItemCreateCommandResponse>.Success(new TodoListItemCreateCommandResponse
            {
                Id = 1,
                TodoListId = todoListId,
                Name = createDto.Name,
                Description = createDto.Description,
                Completed = createDto.Completed,
                Progress = createDto.Progress
            }));

        // Act
        await _controller.PostTodoListItem(todoListId, createDto);

        // Assert
        _mockMediator.Verify(m => m.Send(It.Is<TodoListItemCreateCommand>(c =>
            c.TodoListId == todoListId &&
            c.Name == createDto.Name &&
            c.Description == createDto.Description &&
            c.Completed == createDto.Completed &&
            c.Progress == createDto.Progress
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region DeleteTodoListItem Tests

    [Fact]
    public async Task DeleteTodoListItem_WhenCalled_ReturnsSuccessResult()
    {
        // Arrange
        var todoListId = 1L;
        var itemId = 1L;
        var expectedResponse = new DeleteTodoListItemCommandResponse
        {
            Id = itemId,
            TodoListId = todoListId,
            Message = "TodoListItem deleted successfully",
            DeletedAt = DateTime.UtcNow,
            DeletedItemName = "Complete project"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteTodoListItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<DeleteTodoListItemCommandResponse>.Success(expectedResponse));

        // Act
        var result = await _controller.DeleteTodoListItem(todoListId, itemId);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as DeleteTodoListItemCommandResponse;
        Assert.Equal(itemId, response.Id);
        Assert.Equal(todoListId, response.TodoListId);
        Assert.Contains("deleted successfully", response.Message);
    }

    [Fact]
    public async Task DeleteTodoListItem_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var todoListId = 1L;
        var itemId = 999L;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteTodoListItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<DeleteTodoListItemCommandResponse>.NotFound($"TodoListItem with ID {itemId} not found in TodoList {todoListId}"));

        // Act
        var result = await _controller.DeleteTodoListItem(todoListId, itemId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteTodoListItem_PassesCorrectParameters()
    {
        // Arrange
        var todoListId = 1L;
        var itemId = 1L;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteTodoListItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<DeleteTodoListItemCommandResponse>.Success(new DeleteTodoListItemCommandResponse
            {
                Id = itemId,
                TodoListId = todoListId,
                Message = "TodoListItem deleted successfully",
                DeletedAt = DateTime.UtcNow,
                DeletedItemName = "Test item"
            }));

        // Act
        await _controller.DeleteTodoListItem(todoListId, itemId);

        // Assert
        _mockMediator.Verify(m => m.Send(It.Is<DeleteTodoListItemCommand>(c =>
            c.TodoListId == todoListId &&
            c.Id == itemId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Fact]
    public async Task GetTodoListItems_WhenMediatorThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var todoListId = 1L;
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllTodoListItemsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<GetAllTodoListItemsQueryResponse>.Failure("Internal server error", 500));

        // Act
        var result = await _controller.GetTodoListItems(todoListId);

        // Assert
        Assert.IsType<ObjectResult>(result.Result);
        var objectResult = result.Result as ObjectResult;
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task PostTodoListItem_WhenValidationFails_ReturnsBadRequest()
    {
        // Arrange
        var todoListId = 1L;
        var createDto = new CreateTodoListItemDto { Name = "New task" };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<TodoListItemCreateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TodoListItemCreateCommandResponse>.Failure("Name is required", 400));

        // Act
        var result = await _controller.PostTodoListItem(todoListId, createDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PutTodoListItem_WhenValidationFails_ReturnsBadRequest()
    {
        // Arrange
        var todoListId = 1L;
        var itemId = 1L;
        var updateDto = new UpdateTodoListItemDto { Name = "" }; // Invalid empty name

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateTodoListItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<UpdateTodoListItemCommandResponse>.Failure("Name is required", 400));

        // Act
        var result = await _controller.PutTodoListItem(todoListId, itemId, updateDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WhenMediatorIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TodoListItemController(null));
    }

    #endregion
}

