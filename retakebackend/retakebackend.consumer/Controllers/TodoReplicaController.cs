using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using retakebackend.consumer.Data;

namespace retakebackend.consumer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodoReplicaController(ReplicaDbContext db) : ControllerBase
{
    [HttpGet("lists")]
    public async Task<IActionResult> Lists() => Ok(await db.TodoLists.Include(x => x.Items).AsNoTracking().ToListAsync());

    [HttpGet("items")]
    public async Task<IActionResult> Items() => Ok(await db.TodoItems.AsNoTracking().ToListAsync());
}
