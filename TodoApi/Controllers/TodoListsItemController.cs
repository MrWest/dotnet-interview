using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/todolists/{todoListId}/items")]
    [ApiController]
    public class TodoListsItemController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoListsItemController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/todolists/5/items
        [HttpGet]
        public async Task<ActionResult<IList<TodoListItem>>> GetTodoListItems()
        {
            return Ok(await _context.TodoListItem.ToListAsync());
        }

        // GET: api/todolists/5/items/3
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoListItem>> GetTodoList(long id)
        {
            var todoListItem = await _context.TodoListItem.FindAsync(id);

            if (todoListItem == null)
            {
                return NotFound();
            }

            return Ok(todoListItem);
        }

        // PUT: api/todolists/5/items/4
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult> PutTodoList(long id, UpdateTodoListItem payload)
        {
            var todoListItem = await _context.TodoListItem.FindAsync(id);

            if (todoListItem == null)
            {
                return NotFound();
            }

            todoListItem.Name = payload.Name;
            todoListItem.Description = payload.Description;
            await _context.SaveChangesAsync();

            return Ok(todoListItem);
        }

        // POST: api/todolists
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TodoList>> PostTodoList([FromRoute] int todoListId, CreateTodoListItem payload)
        {
            var todoList = await _context.TodoList.FirstOrDefaultAsync(x => x.Id == todoListId);

            if (todoList == null)
            {
                 return NotFound();
            }
           
            var todoListItem = new TodoListItem { Name = payload.Name, Description = payload.Description, TodoList = todoList };

            _context.TodoListItem.Add(todoListItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTodoListItem", new { id = todoListItem.Id }, todoListItem);
        }

        // DELETE: api/todolists/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTodoList(long id)
        {
            var todoListItem = await _context.TodoListItem.FindAsync(id);
            if (todoListItem == null)
            {
                return NotFound();
            }

            _context.TodoListItem.Remove(todoListItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
