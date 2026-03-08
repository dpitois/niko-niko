using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using NikoNiko.Core.DTOs.Sprint;
using NikoNiko.Core.Models;
using NikoNiko.Data;
using NikoNiko.Services;

namespace NikoNiko.Services.UnitTests;

public class SprintServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly SprintService _service;
    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _teamId = Guid.NewGuid();

    public SprintServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _service = new SprintService(_context);

        // Seed basic data
        var team = new Team
        {
            Id = _teamId,
            Name = "Test Team",
            AdminId = _adminId
        };
        _context.Teams.Add(team);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateSprintAsync_ShouldCreateSprint_WhenUserIsAdmin()
    {
        // Arrange
        var dto = new CreateSprintDto
        {
            Name = "Sprint 1",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 1, 14),
            TeamId = _teamId
        };

        // Act
        var result = await _service.CreateSprintAsync(dto, _adminId, false);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Sprint 1");
        _context.Sprints.Should().ContainSingle(s => s.Name == "Sprint 1");
    }

    [Fact]
    public async Task CreateSprintAsync_ShouldThrowUnauthorized_WhenUserIsNotAdmin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateSprintDto
        {
            Name = "Sprint 1",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 1, 14),
            TeamId = _teamId
        };

        // Act
        var act = () => _service.CreateSprintAsync(dto, userId, false);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateSprintAsync_ShouldThrowInvalidOperation_WhenStartDateAfterExistingMoodEntries()
    {
        // Arrange
        var sprintId = Guid.NewGuid();
        var sprint = new Sprint
        {
            Id = sprintId,
            Name = "Sprint 1",
            StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 1, 14, 0, 0, 0, DateTimeKind.Utc),
            TeamId = _teamId
        };
        _context.Sprints.Add(sprint);

        var moodEntry = new MoodEntry
        {
            Id = Guid.NewGuid(),
            SprintId = sprintId,
            UserId = _adminId,
            Date = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc),
            Mood = MoodType.VeryHappy
        };
        _context.MoodEntries.Add(moodEntry);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateSprintDto
        {
            Name = "Sprint 1 Updated",
            StartDate = new DateOnly(2026, 1, 6), // After mood entry date
            EndDate = new DateOnly(2026, 1, 14)
        };

        // Act
        var act = () => _service.UpdateSprintAsync(sprintId, updateDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot set start date after*");
    }

    [Fact]
    public async Task CreateSprintAsync_ShouldThrowInvalidOperation_WhenEndDateBeforeStartDate()
    {
        // Arrange
        var dto = new CreateSprintDto
        {
            Name = "Invalid Sprint",
            StartDate = new DateOnly(2026, 1, 14),
            EndDate = new DateOnly(2026, 1, 1), // End before Start
            TeamId = _teamId
        };

        // Act
        var act = () => _service.CreateSprintAsync(dto, _adminId, false);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateSprintAsync_ShouldThrowInvalidOperation_WhenDatesOverlap()
    {
        // Arrange
        var existingSprint = new Sprint
        {
            Id = Guid.NewGuid(),
            Name = "Existing Sprint",
            StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 1, 14, 0, 0, 0, DateTimeKind.Utc),
            TeamId = _teamId
        };
        _context.Sprints.Add(existingSprint);
        await _context.SaveChangesAsync();

        var dto = new CreateSprintDto
        {
            Name = "Overlapping Sprint",
            StartDate = new DateOnly(2026, 1, 10), // Overlaps with existing
            EndDate = new DateOnly(2026, 1, 20),
            TeamId = _teamId
        };

        // Act
        var act = () => _service.CreateSprintAsync(dto, _adminId, false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*overlap*");
    }

    [Fact]
    public async Task CreateSprintAsync_ShouldThrowInvalidOperation_WhenDatesTouch()
    {
        // Arrange
        var existingSprint = new Sprint
        {
            Id = Guid.NewGuid(),
            Name = "Existing Sprint",
            StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 1, 14, 0, 0, 0, DateTimeKind.Utc),
            TeamId = _teamId
        };
        _context.Sprints.Add(existingSprint);
        await _context.SaveChangesAsync();

        var dto = new CreateSprintDto
        {
            Name = "Touching Sprint",
            StartDate = new DateOnly(2026, 1, 14), // Starts the same day the previous one ends
            EndDate = new DateOnly(2026, 1, 28),
            TeamId = _teamId
        };

        // Act
        var act = () => _service.CreateSprintAsync(dto, _adminId, false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*overlap*");
    }

    [Fact]
    public async Task UpdateSprintAsync_ShouldThrowInvalidOperation_WhenDatesOverlapWithAnotherSprint()
    {
        // Arrange
        var otherSprint = new Sprint
        {
            Id = Guid.NewGuid(),
            Name = "Other Sprint",
            StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 1, 14, 0, 0, 0, DateTimeKind.Utc),
            TeamId = _teamId
        };
        var currentSprint = new Sprint
        {
            Id = Guid.NewGuid(),
            Name = "Current Sprint",
            StartDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 1, 28, 0, 0, 0, DateTimeKind.Utc),
            TeamId = _teamId
        };
        _context.Sprints.AddRange(otherSprint, currentSprint);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateSprintDto
        {
            Name = "Current Sprint Updated",
            StartDate = new DateOnly(2026, 1, 14), // Overlaps with otherSprint
            EndDate = new DateOnly(2026, 1, 28)
        };

        // Act
        var act = () => _service.UpdateSprintAsync(currentSprint.Id, updateDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*overlap*");
    }
}