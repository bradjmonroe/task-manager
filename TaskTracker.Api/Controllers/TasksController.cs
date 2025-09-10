using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Contracts;
using TaskTracker.Api.Infrastructure;
using TaskTracker.Data;
using TaskTracker.Data.Models;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly TasksDb _db;
    public TasksController(TasksDb db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<TaskItem>>> GetMine()
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var tasks = await _db.Tasks
            .Where(t => t.CreatedBy == userId)
            .OrderBy(t => t.IsDone)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TaskCreateDto dto)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Title required");

        var item = new TaskItem { Title = dto.Title.Trim(), CreatedBy = userId.Value };
        _db.Tasks.Add(item);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMine), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var item = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.CreatedBy == userId);
        if (item is null) return NotFound();

        item.IsDone = !item.IsDone;
        await _db.SaveChangesAsync();
        return Ok(item);
    }
}
