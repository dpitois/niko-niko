using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

using NikoNiko.Core.DTOs.Mood;
using NikoNiko.Core.Interfaces;
using NikoNiko.Core.Models;
using NikoNiko.Data;
using NikoNiko.Services;

namespace NikoNiko.Services.UnitTests;

public class MoodServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly MoodService _service;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _teamId = Guid.NewGuid();
    private readonly Guid _sprintId = Guid.NewGuid();

    public MoodServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _notificationServiceMock = new Mock<INotificationService>();
        _service = new MoodService(_context, _notificationServiceMock.Object);

        // Seed data
        var user = new User
        {
            Id = _userId,
            Email = "test@example.com",
            Name = "Test User",
            OAuthId = "oauth|123"
        };
        var team = new Team { Id = _teamId, Name = "Test Team", AdminId = Guid.NewGuid() };
        var teamUser = new TeamUser { TeamId = _teamId, UserId = _userId };
        var sprint = new Sprint
        {
            Id = _sprintId,
            Name = "Test Sprint",
            TeamId = _teamId,
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow.AddDays(7)
        };

        _context.Users.Add(user);
        _context.Teams.Add(team);
        _context.TeamUsers.Add(teamUser);
        _context.Sprints.Add(sprint);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateOrUpdateMoodEntryAsync_ShouldSucceed_WhenValid()
    {
        // Arrange
        var dto = new CreateMoodEntryDto
        {
            UserId = _userId,
            SprintId = _sprintId,
            Mood = MoodType.Happy,
            Date = DateTime.UtcNow,
            TimezoneOffset = 0
        };

        // Act
        var (result, isCreated) = await _service.CreateOrUpdateMoodEntryAsync(dto, _userId);

        // Assert
        isCreated.Should().BeTrue();
        result.Mood.Should().Be(MoodType.Happy);
        _context.MoodEntries.Should().ContainSingle(me => me.UserId == _userId);
        _notificationServiceMock.Verify(n => n.SendMoodNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), _userId.ToString(), _teamId), Times.Once);
    }

    [Fact]
    public async Task CreateOrUpdateMoodEntryAsync_ShouldThrowArgumentException_WhenDateInFuture()
    {
        // Arrange
        var dto = new CreateMoodEntryDto
        {
            UserId = _userId,
            SprintId = _sprintId,
            Mood = MoodType.Happy,
            Date = DateTime.UtcNow.AddDays(2), // Future
            TimezoneOffset = 0
        };

        // Act
        var act = () => _service.CreateOrUpdateMoodEntryAsync(dto, _userId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*cannot be in the future*");
    }

    [Fact]
    public async Task CreateOrUpdateMoodEntryAsync_ShouldThrowArgumentException_WhenDateOutsideSprint()
    {
        // Arrange
        var dto = new CreateMoodEntryDto
        {
            UserId = _userId,
            SprintId = _sprintId,
            Mood = MoodType.Happy,
            Date = DateTime.UtcNow.AddDays(-10), // Before sprint start
            TimezoneOffset = 0
        };

        // Act
        var act = () => _service.CreateOrUpdateMoodEntryAsync(dto, _userId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*cannot be before the sprint start date*");
    }

    [Fact]
    public async Task CreateOrUpdateMoodEntryAsync_ShouldUpdateExisting_WhenEntryAlreadyExistsForSameDate()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var existingEntry = new MoodEntry
        {
            UserId = _userId,
            SprintId = _sprintId,
            Mood = MoodType.Neutral,
            Date = date
        };
        _context.MoodEntries.Add(existingEntry);
        await _context.SaveChangesAsync();

        var dto = new CreateMoodEntryDto
        {
            UserId = _userId,
            SprintId = _sprintId,
            Mood = MoodType.VeryHappy,
            Date = date,
            TimezoneOffset = 0
        };

        // Act
        var (result, isCreated) = await _service.CreateOrUpdateMoodEntryAsync(dto, _userId);

        // Assert
        isCreated.Should().BeFalse();
        result.Mood.Should().Be(MoodType.VeryHappy);
        _context.MoodEntries.Count().Should().Be(1);
    }
}