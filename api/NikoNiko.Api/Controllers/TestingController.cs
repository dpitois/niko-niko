#if E2E
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NikoNiko.Core.Interfaces;
using NikoNiko.Core.Models;
using NikoNiko.Data;
using NikoNiko.Services;

namespace NikoNiko.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TestingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<TestingController> _logger;

        public TestingController(ApplicationDbContext context, ITokenService tokenService, ILogger<TestingController> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("reset")]
        public async Task<IActionResult> Reset()
        {
            _logger.LogInformation("Resetting database for E2E tests...");
            
            // 1. Wipe and recreate Database
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.MigrateAsync();

            // 2. Seed initial data
            await SeedTestData();

            _logger.LogInformation("Database reset complete.");
            return Ok(new { message = "E2E Reset Complete" });
        }

        public class LoginRequest
        {
            public string Email { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.TeamUsers)
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound("User not found via Backdoor Login: " + request.Email);
            }

            var token = _tokenService.CreateToken(user);
            return Ok(new { token });
        }

        private async Task SeedTestData()
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                OAuthId = "e2e-test-admin-id",
                Email = "admin@test.com",
                Name = "Admin Test",
                Provider = "Local",
                IsSuperAdmin = true,
                IsOnboarded = true,
                CreatedAt = DateTime.UtcNow
            };

            var memberUser = new User
            {
                Id = Guid.NewGuid(),
                OAuthId = "e2e-test-member-id",
                Email = "member@test.com",
                Name = "Member Test",
                Provider = "Local",
                IsSuperAdmin = false,
                IsOnboarded = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.AddRange(adminUser, memberUser);
            await _context.SaveChangesAsync();
        }
    }
}
#endif