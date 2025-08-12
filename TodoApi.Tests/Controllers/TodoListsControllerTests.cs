using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Moq;
using TodoApi.Controllers;
using TodoApi.Models;
using TodoApi.Mediation.TodoList;
using TodoApi.Mediation.TodoList.Dtos;
using TodoApi.Infrastructure;

namespace TodoApi.Tests;

#nullable disable
public class TodoListsControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly TodoListsController _controller;

    public TodoListsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new TodoListsController(_mockMediator.Object);
    }

    #region GetTodoLists Tests

    [Fact]
    public async Task GetTodoLists_WhenCalled_ReturnsTodoListList()
    {
        // Arrange
        var expectedResponse = new GetAllTodoListsQueryResponse
        {
            TodoLists = new List<TodoListResponseDto>
            {
                new TodoListResponseDto { Id = 1, Name = "Task 1" },
                new TodoListResponseDto { Id = 2, Name = "Task 2" }
            },
            TotalCount = 2
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllTodoListsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<GetAllTodoListsQueryResponse>.Success(expectedResponse));

        // Act
        var result = await _controller.GetTodoLists();

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as GetAllTodoListsQueryResponse;
        Assert.Equal(2, response.TodoLists.Count);
    }

    [Fact]
    public async Task GetTodoLists_WithIncludeItems_PassesCorrectParameter()
    {
        // Arrange
        var includeItems = true;
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllTodoListsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<GetAllTodoListsQueryResponse>.Success(new GetAllTodoListsQueryResponse()));

        // Act
        await _controller.GetTodoLists(includeItems);

        // Assert
        _mockMediator.Verify(m => m.Send(It.Is<GetAllTodoListsQuery>(q =>
            q.IncludeItems == includeItems
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetTodoList Tests

    [Fact]
    public async Task GetTodoList_WhenCalled_ReturnsTodoListById()
    {
        // Arrange
        var todoListId = 1L;
        var expectedResponse = new GetTodoListQueryResponse
        {
            TodoList = new TodoListResponseDto { Id = todoListId, Name = "Task 1" }
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetTodoListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<GetTodoListQueryResponse>.Success(expectedResponse));

        // Act
        var result = await _controller.GetTodoList(todoListId);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as GetTodoListQueryResponse;
        Assert.Equal(todoListId, response.TodoList.Id);
    }

    [Fact]
    public async Task GetTodoList_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var todoListId = 999L;
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetTodoListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<GetTodoListQueryResponse>.NotFound($"TodoList with ID {todoListId} not found"));

        // Act
        var result = await _controller.GetTodoList(todoListId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetTodoList_PassesCorrectParameters()
    {
        // Arrange
        var todoListId = 1L;
        var includeItems = true;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetTodoListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<GetTodoListQueryResponse>.Success(new GetTodoListQueryResponse
            {
                TodoList = new TodoListResponseDto { Id = todoListId, Name = "Test" }
            }));

        // Act
        await _controller.GetTodoList(todoListId, includeItems);

        // Assert
        _mockMediator.Verify(m => m.Send(It.Is<GetTodoListQuery>(q =>
            q.Id == todoListId &&
            q.IncludeItems == includeItems
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region PutTodoList Tests

    [Fact]
    public async Task PutTodoList_WhenTodoListDoesntExist_ReturnsNotFound()
    {
        // Arrange
        var todoListId = 3L;
        var updateDto = new UpdateTodoListDto { Name = "Task 3" };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateTodoListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<UpdateTodoListCommandResponse>.NotFound($"TodoList with ID {todoListId} not found"));

        // Act
        var result = await _controller.PutTodoList(todoListId, updateDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task PutTodoList_WhenCalled_UpdatesTheTodoList()
    {
        // Arrange
        var todoListId = 2L;
        var updateDto = new UpdateTodoListDto { Name = "Changed Task 2" };
        var expectedResponse = new UpdateTodoListCommandResponse
        {
            Id = todoListId,
            Name = updateDto.Name,
            UpdatedAt = DateTime.UtcNow
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateTodoListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<UpdateTodoListCommandResponse>.Success(expectedResponse));

        // Act
        var result = await _controller.PutTodoList(todoListId, updateDto);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as UpdateTodoListCommandResponse;
        Assert.Equal(updateDto.Name, response.Name);
    }

    [Fact]
    public async Task PutTodoList_PassesCorrectParameters()
    {
        // Arrange
        var todoListId = 2L;
        var updateDto = new UpdateTodoListDto { Name = "Updated Task" };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateTodoListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<UpdateTodoListCommandResponse>.Success(new UpdateTodoListCommandResponse
            {
                Id = todoListId,
                Name = updateDto.Name,
                UpdatedAt = DateTime.UtcNow
            }));

        // Act
        await _controller.PutTodoList(todoListId, updateDto);

        // Assert
        _mockMediator.Verify(m => m.Send(It.Is<UpdateTodoListCommand>(c =>
            c.Id == todoListId &&
            c.Name == updateDto.Name
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region PostTodoList Tests

    [Fact]
    public async Task PostTodoList_WhenCalled_CreatesTodoList()
    {
        // Arrange
        var createDto = new CreateTodoListDto { Name = "Task 3" };
        var expectedResponse = new TodoListCreateCommandResponse
        {
            Id = 3,
            Name = createDto.Name
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<TodoListCreateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TodoListCreateCommandResponse>.Success(expectedResponse));

        // Act
        var result = await _controller.PostTodoList(createDto);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.Equal(nameof(_controller.GetTodoList), createdResult.ActionName);
        
        var routeValues = createdResult.RouteValues;
        Assert.Equal(expectedResponse.Id, routeValues["id"]);
        
        var response = createdResult.Value as TodoListCreateCommandResponse;
        Assert.Equal(createDto.Name, response.Name);
    }

    [Fact]
    public async Task PostTodoList_WhenValidationFails_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateTodoListDto { Name = "" }; // Invalid empty name

        _mockMediator
            .Setup(m => m.Send(It.IsAny<TodoListCreateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TodoListCreateCommandResponse>.Failure("Name is required", 400));

        // Act
        var result = await _controller.PostTodoList(createDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostTodoList_PassesCorrectParameters()
    {
        // Arrange
        var createDto = new CreateTodoListDto { Name = "New Task" };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<TodoListCreateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TodoListCreateCommandResponse>.Success(new TodoListCreateCommandResponse
            {
                Id = 1,
                Name = createDto.Name
            }));

        // Act
        await _controller.PostTodoList(createDto);

        // Assert
        _mockMediator.Verify(m => m.Send(It.Is<TodoListCreateCommand>(c =>
            c.Name == createDto.Name
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region DeleteTodoList Tests

    [Fact]
    public async Task DeleteTodoList_WhenCalled_RemovesTodoList()
    {
        // Arrange
        var todoListId = 2L;
        var expectedResponse = new DeleteTodoListCommandResponse
        {
            Id = todoListId,
            Message = "TodoList deleted successfully",
            DeletedAt = DateTime.UtcNow,
            DeletedItemsCount = 0
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteTodoListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<DeleteTodoListCommandResponse>.Success(expectedResponse));

        // Act
        var result = await _controller.DeleteTodoList(todoListId);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = result.Result as OkObjectResult;
        var response = okResult.Value as DeleteTodoListCommandResponse;
        Assert.Equal(todoListId, response.Id);
        Assert.Contains("deleted successfully", response.Message);
    }

    [Fact]
    public async Task DeleteTodoList_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var todoListId = 999L;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteTodoListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<DeleteTodoListCommandResponse>.NotFound($"TodoList with ID {todoListId} not found"));

        // Act
        var result = await _controller.DeleteTodoList(todoListId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteTodoList_PassesCorrectParameters()
    {
        // Arrange
        var todoListId = 2L;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteTodoListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<DeleteTodoListCommandResponse>.Success(new DeleteTodoListCommandResponse
            {
                Id = todoListId,
                Message = "TodoList deleted successfully",
                DeletedAt = DateTime.UtcNow,
                DeletedItemsCount = 0
            }));

        // Act
        await _controller.DeleteTodoList(todoListId);

        // Assert
        _mockMediator.Verify(m => m.Send(It.Is<DeleteTodoListCommand>(c =>
            c.Id == todoListId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Fact]
    public async Task GetTodoLists_WhenMediatorThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllTodoListsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<GetAllTodoListsQueryResponse>.Failure("Internal server error", 500));

        // Act
        var result = await _controller.GetTodoLists();

        // Assert
        Assert.IsType<ObjectResult>(result.Result);
        var objectResult = result.Result as ObjectResult;
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task PutTodoList_WhenValidationFails_ReturnsBadRequest()
    {
        // Arrange
        var todoListId = 1L;
        var updateDto = new UpdateTodoListDto { Name = "" }; // Invalid empty name

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateTodoListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<UpdateTodoListCommandResponse>.Failure("Name is required", 400));

        // Act
        var result = await _controller.PutTodoList(todoListId, updateDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WhenMediatorIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TodoListsController(null));
    }

    #endregion
}

