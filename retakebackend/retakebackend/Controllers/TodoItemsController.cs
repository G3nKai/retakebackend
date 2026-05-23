using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using retakebackend.Data;
using retakebackend.Dtos;
using retakebackend.Models;
using retakebackend.Services;

namespace retakebackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodoItemsController(AppDbContext dbContext, RabbitPublisher publisher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetAll()
    {
        var items = await dbContext.TodoItems
            .AsNoTracking()
            .Select(i => new TodoItemDto(i.Id, i.Title, i.IsCompleted, i.TodoListId))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TodoItemDto>> GetById(int id)
    {
        var item = await dbContext.TodoItems
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new TodoItemDto(i.Id, i.Title, i.IsCompleted, i.TodoListId))
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<TodoItemDto>> Create(CreateTodoItemDto dto)
    {
        var listExists = await dbContext.TodoLists.AnyAsync(l => l.Id == dto.TodoListId);
        if (!listExists)
        {
            return BadRequest($"TodoList with id {dto.TodoListId} does not exist.");
        }

        var item = new TodoItem
        {
            Title = dto.Title,
            IsCompleted = dto.IsCompleted,
            TodoListId = dto.TodoListId
        };

        dbContext.TodoItems.Add(item);
        await dbContext.SaveChangesAsync();
        publisher.Publish(new("TodoItem", "Created", item.Id, JsonSerializer.Serialize(item)));

        var result = new TodoItemDto(item.Id, item.Title, item.IsCompleted, item.TodoListId);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateTodoItemDto dto)
    {
        var item = await dbContext.TodoItems.FindAsync(id);
        if (item is null) return NotFound();

        var listExists = await dbContext.TodoLists.AnyAsync(l => l.Id == dto.TodoListId);
        if (!listExists)
        {
            return BadRequest($"TodoList with id {dto.TodoListId} does not exist.");
        }

        item.Title = dto.Title;
        item.IsCompleted = dto.IsCompleted;
        item.TodoListId = dto.TodoListId;

        await dbContext.SaveChangesAsync();
        publisher.Publish(new("TodoItem", "Updated", item.Id, JsonSerializer.Serialize(item)));
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await dbContext.TodoItems.FindAsync(id);
        if (item is null) return NotFound();

        dbContext.TodoItems.Remove(item);
        await dbContext.SaveChangesAsync();
        publisher.Publish(new("TodoItem", "Deleted", id, "{}"));
        return NoContent();
    }
}
