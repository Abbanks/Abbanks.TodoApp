using Abbanks.TodoApp.Application.DTOs;
using Abbanks.TodoApp.Application.Services;
using Abbanks.TodoApp.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Abbanks.TodoApp.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _todoService;
        private readonly ILogger<TodosController> _logger;

        public TodosController(ITodoService todoService, ILogger<TodosController> logger)
        {
            _todoService = todoService ?? throw new ArgumentNullException(nameof(todoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves todos based on optional filters
        /// </summary>
        /// <returns>List of todo items</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTodos(
            [FromQuery] TodoStatus? status = null,
            [FromQuery] Priority? priority = null,
            [FromQuery] DateTime? dueDateBefore = null)
        {
            try
            {
                var userId = GetUserIdFromToken();
                _logger.LogInformation("Retrieving todos for user {UserId} with filters: status={Status}, priority={Priority}",
                    userId, status, priority);

                var todos = await _todoService.GetFilteredTodosAsync(userId, status, priority, dueDateBefore);
                return Ok(todos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving todos");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving todos" });
            }
        }

        /// <summary>
        /// Retrieves a specific todo by ID
        /// </summary>
        /// <param name="id">The todo ID</param>
        /// <returns>The requested todo item</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTodo(Guid id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                _logger.LogInformation("Retrieving todo {TodoId} for user {UserId}", id, userId);

                var todo = await _todoService.GetTodoByIdAsync(id, userId);

                if (todo == null)
                {
                    _logger.LogWarning("Todo {TodoId} not found for user {UserId}", id, userId);
                    return NotFound(new { message = "Todo item not found" });
                }

                return Ok(todo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving todo {TodoId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the todo" });
            }
        }

        /// <summary>
        /// Creates a new todo item
        /// </summary>
        /// <param name="createTodoDto">Todo creation data</param>
        /// <returns>The newly created todo</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTodo([FromBody] CreateTodoDto createTodoDto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                _logger.LogInformation("Creating new todo for user {UserId}", userId);

                var createdTodo = await _todoService.CreateTodoAsync(createTodoDto, userId);

                _logger.LogInformation("Created todo {TodoId} for user {UserId}", createdTodo.Id, userId);
                return CreatedAtAction(nameof(GetTodo), new { id = createdTodo.Id }, createdTodo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating todo");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the todo" });
            }
        }

        /// <summary>
        /// Updates an existing todo item
        /// </summary>
        /// <param name="id">The todo ID</param>
        /// <param name="updateTodoDto">Updated todo data</param>
        /// <returns>No content on success</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTodo(Guid id, [FromBody] UpdateTodoDto updateTodoDto)
        {
            try
            {
                if (id != updateTodoDto.Id)
                {
                    _logger.LogWarning("ID mismatch in update request: route ID {RouteId} != body ID {BodyId}", id, updateTodoDto.Id);
                    return BadRequest(new { message = "ID mismatch between route and body" });
                }

                var userId = GetUserIdFromToken();
                _logger.LogInformation("Updating todo {TodoId} for user {UserId}", id, userId);

                await _todoService.UpdateTodoAsync(updateTodoDto, userId);

                _logger.LogInformation("Successfully updated todo {TodoId} for user {UserId}", id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Todo {TodoId} not found for update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to todo {TodoId} by user {UserId}", id, GetUserIdFromToken());
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating todo {TodoId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the todo" });
            }
        }

        /// <summary>
        /// Deletes a todo item
        /// </summary>
        /// <param name="id">The todo ID to delete</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTodo(Guid id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                _logger.LogInformation("Deleting todo {TodoId} for user {UserId}", id, userId);

                await _todoService.DeleteTodoAsync(id, userId);

                _logger.LogInformation("Successfully deleted todo {TodoId} for user {UserId}", id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Todo {TodoId} not found for deletion", id);
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized deletion attempt for todo {TodoId} by user {UserId}", id, GetUserIdFromToken());
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting todo {TodoId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the todo" });
            }
        }

        /// <summary>
        /// Extracts the user ID from the authentication token
        /// </summary>
        private Guid GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User ID claim missing from token");
                throw new UnauthorizedAccessException("User identity not found");
            }

            return Guid.Parse(userIdClaim);
        }
    }
}