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
public class TodoListsController(AppDbContext dbContext, RabbitPublisher publisher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoListDto>>> GetAll()
    {
        var lists = await dbContext.TodoLists
            .AsNoTracking()
            .Select(l => new TodoListDto(l.Id, l.Title))
            .ToListAsync();

        return Ok(lists);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TodoList>> GetById(int id)
    {
        var list = await dbContext.TodoLists
            .Include(l => l.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

        return list is null ? NotFound() : Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<TodoListDto>> Create(CreateTodoListDto dto)
    {
        var list = new TodoList { Title = dto.Title };
        dbContext.TodoLists.Add(list);
        await dbContext.SaveChangesAsync();
        publisher.Publish(new("TodoList", "Created", list.Id, JsonSerializer.Serialize(list)));

        var result = new TodoListDto(list.Id, list.Title);
        return CreatedAtAction(nameof(GetById), new { id = list.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateTodoListDto dto)
    {
        var list = await dbContext.TodoLists.FindAsync(id);
        if (list is null) return NotFound();

        list.Title = dto.Title;
        await dbContext.SaveChangesAsync();
        publisher.Publish(new("TodoList", "Updated", list.Id, JsonSerializer.Serialize(list)));
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var list = await dbContext.TodoLists.FindAsync(id);
        if (list is null) return NotFound();

        dbContext.TodoLists.Remove(list);
        await dbContext.SaveChangesAsync();
        publisher.Publish(new("TodoList", "Deleted", id, "{}"));
        return NoContent();
    }
}
