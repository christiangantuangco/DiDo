using System.Security.Claims;
using DiDo.Data;
using DiDo.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiDo.Controllers;

public class UserController(DiDoDbContext dbContext) : Controller
{
    private readonly DiDoDbContext _dbContext = dbContext;

    public IActionResult Index()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction(nameof(Login));
        }


        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([Bind("UsernameOrEmail, Password")] LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        User? user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.UserName == model.UsernameOrEmail || u.Email == model.UsernameOrEmail);

        if (user is null)
        {
            ModelState.AddModelError("", "Invalid username/email or password.");
            return View(model);
        }

        // Password Hasher
        PasswordHasher<User>? hasher = new();
        PasswordVerificationResult result = hasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, model.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Invalid username/email or password.");
            return View(model);
        }

        // Claims
        List<Claim> claims = [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName)
        ];

        ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        ClaimsPrincipal principal = new(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToAction("Create", "Entry");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([Bind("FirstName, LastName, UserName, Email, Password")] RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (!string.IsNullOrEmpty(model.Username))
        {
            User? existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.UserName == model.Username);

            if (existingUser != null)
            {
                ModelState.AddModelError("", "Username already exist.");
                return View(model);
            }
        }

        User newUser = new()
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            CreatedAt = DateTime.UtcNow
        };

        PasswordHasher<User> passwordHasher = new();
        string passwordHash = passwordHasher.HashPassword(newUser, model.Password);
        newUser.PasswordHash = passwordHash;

        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync();

        List<Claim> claims = [
            new(ClaimTypes.NameIdentifier, newUser.Id.ToString()),
            new(ClaimTypes.GivenName, newUser.FirstName),
            new(ClaimTypes.Surname, newUser.LastName)
        ];
        ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        ClaimsPrincipal principal = new(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToAction("Index", "Entry");
    }

    public async Task<IActionResult> Update(
        [Bind("Id, FirstName, LastName, UserName, Email, PasswordHash, CreatedAt")] User user
    )
    {
        if (ModelState.IsValid)
        {
            _dbContext.Update(user);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(user);
    }

    public async Task<IActionResult> Delete(int userId)
    {
        User? user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return NotFound();
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}