using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using NikoNiko.Core.DTOs.Team.Invitation;
using NikoNiko.Core.Models;
using NikoNiko.Data;
using NikoNiko.Services;

namespace NikoNiko.Services.UnitTests;

public class TeamInvitationServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly TeamInvitationService _service;
    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _teamId = Guid.NewGuid();

    public TeamInvitationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _service = new TeamInvitationService(_context);

        // Seed basic data
        var admin = new User
        {
            Id = _adminId,
            Email = "admin@example.com",
            Name = "Admin User",
            OAuthId = "oauth|admin"
        };
        var team = new Team
        {
            Id = _teamId,
            Name = "Test Team",
            AdminId = _adminId
        };
        _context.Users.Add(admin);
        _context.Teams.Add(team);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateTeamInvitationAsync_ShouldCreateInvitation_WhenUserIsAdmin()
    {
        // Arrange
        var dto = new CreateTeamInvitationDto { ExpirationInDays = 7 };

        // Act
        var result = await _service.CreateTeamInvitationAsync(_teamId, _adminId, dto);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Pending");
        result.Token.Should().NotBeNullOrEmpty();
        _context.TeamInvitations.Should().ContainSingle(i => i.Token == result.Token);
    }

    [Fact]
    public async Task AcceptTeamInvitationAsync_ShouldAddUserToTeam_WhenTokenIsValid()
    {
        // Arrange
        var token = "valid-token";
        var invitation = new TeamInvitation
        {
            Id = Guid.NewGuid(),
            TeamId = _teamId,
            CreatorUserId = _adminId,
            Token = token,
            ExpirationDate = DateTime.UtcNow.AddDays(1),
            Status = "Pending"
        };
        _context.TeamInvitations.Add(invitation);

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "new@example.com",
            Name = "New User",
            OAuthId = "oauth|new"
        };
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AcceptTeamInvitationAsync(token, newUser.Id);

        // Assert
        result.Status.Should().Be("Accepted");
        _context.TeamUsers.Should().ContainSingle(tu => tu.TeamId == _teamId && tu.UserId == newUser.Id);
    }

    [Fact]
    public async Task AcceptTeamInvitationAsync_ShouldThrowInvalidOperation_WhenInvitationIsExpired()
    {
        // Arrange
        var token = "expired-token";
        var invitation = new TeamInvitation
        {
            Id = Guid.NewGuid(),
            TeamId = _teamId,
            CreatorUserId = _adminId,
            Token = token,
            ExpirationDate = DateTime.UtcNow.AddDays(-1), // Expired
            Status = "Pending"
        };
        _context.TeamInvitations.Add(invitation);
        await _context.SaveChangesAsync();

        var userId = Guid.NewGuid();

        // Act
        var act = () => _service.AcceptTeamInvitationAsync(token, userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*expired*");

        var updatedInvitation = await _context.TeamInvitations.IgnoreQueryFilters().FirstAsync(i => i.Token == token);
        updatedInvitation.Status.Should().Be("Expired");
    }
}