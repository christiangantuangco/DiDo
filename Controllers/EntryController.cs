using Microsoft.AspNetCore.Mvc;
using DiDo.Data;
using DiDo.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DiDo.Controllers;

public class EntryController(DiDoDbContext dbContext) : Controller
{
    private readonly DiDoDbContext _dbContext = dbContext;

    public async Task<IActionResult> Index()
    {
        string? claimsUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(claimsUserId)) return RedirectToAction("Logout", "User");

        List<Entry> entries = await _dbContext.Entries
            .Where(e => e.UserId == int.Parse(claimsUserId))
            .ToListAsync();

        return View(entries);
    }

    public IActionResult Create()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "User");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Description")] Entry entry)
    {
        if (ModelState.IsValid)
        {
            string? userIdClaims = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaims)) return RedirectToAction("Login", "User");

            entry.UserId = int.Parse(userIdClaims);
            entry.Date = DateTime.UtcNow;

            _dbContext.Add(entry);
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = entry.Id });
    }

    public IActionResult Details(int id)
    {
        Entry? entry = _dbContext.Entries.FirstOrDefault(e => e.Id == id);
        if (entry == null)
        {
            return NotFound();
        }

        return View(entry);
    }

    public IActionResult Update(int id)
    {
        Entry? entry = _dbContext.Entries.FirstOrDefault(e => e.Id == id);
        if (entry == null)
        {
            return NotFound();
        }

        EntryViewModel model = new()
        {
            Id = entry.Id,
            Description = entry.Description,
            Date = entry.Date
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, [Bind("Description")] EntryViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        Entry? existingEntry = await _dbContext.Entries
            .FirstOrDefaultAsync(e => e.Id == id);

        if (existingEntry == null) return NotFound();

        existingEntry.Description = model.Description;
        existingEntry.Date = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = existingEntry.Id });
    }

    public IActionResult Delete(int id)
    {
        Entry? entry = _dbContext.Entries.FirstOrDefault(e => e.Id == id);
        if (entry == null)
        {
            return NotFound();
        }

        return View(entry);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        Entry? entry = await _dbContext.Entries.FirstOrDefaultAsync(e => e.Id == id);
        if (entry == null)
        {
            return NotFound();
        }

        _dbContext.Entries.Remove(entry);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Entries()
    {
        List<Entry> entries = [];

        string? existingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(existingUserId))
        {
            int userId = int.Parse(existingUserId);

            entries = await _dbContext.Entries
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        return View(entries);
    }
}
