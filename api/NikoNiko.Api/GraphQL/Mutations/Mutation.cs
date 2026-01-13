using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using NikoNiko.Core.DTOs.Mood;
using NikoNiko.Core.Models;
using NikoNiko.Data;
using NikoNiko.Services;

namespace NikoNiko.Api.GraphQL.Mutations;

public class Mutation
{
    [Authorize]
    public async Task<MoodEntry> AddMoodEntry(
        CreateMoodEntryDto input,
        ApplicationDbContext context,
        [Service] INotificationService notificationService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            throw new GraphQLException("User not authenticated.");
        }

        var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
        // Fallback for some OAuth scenarios where NameIdentifier might be different or mapped differently
        if (string.IsNullOrEmpty(userIdString))
        {
             // Try to resolve user by email if ID is not available directly
             var email = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity?.Name;
             if (!string.IsNullOrEmpty(email))
             {
                 var dbUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
                 if (dbUser != null)
                 {
                     userIdString = dbUser.Id.ToString();
                 }
             }
        }

        if (!Guid.TryParse(userIdString, out var authenticatedUserId))
        {
             throw new GraphQLException("User ID not found or invalid.");
        }

        // Validate that the user is creating an entry for themselves
        // In the Controller, we check if input.UserId == authenticatedUserId.
        // However, we might want to override input.UserId with authenticatedUserId to ensure security.
        // But let's stick to the controller logic: verify equality.
        if (input.UserId != authenticatedUserId)
        {
             throw new GraphQLException("You can only create mood entries for yourself.");
        }

        var sprint = await context.Sprints.FindAsync(input.SprintId);
        if (sprint == null)
        {
             throw new GraphQLException("Sprint not found.");
        }

        var entryDate = input.Date?.ToUniversalTime().Date ?? DateTime.UtcNow.Date;

        // Calculate the user's local date based on the provided timezone offset.
        var userLocalNow = DateTime.UtcNow.AddMinutes(input.TimezoneOffset);

        if (entryDate > userLocalNow.Date)
        {
             throw new GraphQLException("Mood entry date cannot be in the future (relative to your local time).");
        }

        if (entryDate < sprint.StartDate.Date)
        {
             throw new GraphQLException("Mood entry date cannot be before the sprint start date.");
        }

        if (entryDate > sprint.EndDate.Date)
        {
             throw new GraphQLException("Mood entry date cannot be after the sprint end date.");
        }

        var existingEntry = await context.MoodEntries.FirstOrDefaultAsync(me =>
            me.UserId == input.UserId &&
            me.SprintId == input.SprintId &&
            me.Date.Date == entryDate);

        var dbUserForNotif = await context.Users.FirstOrDefaultAsync(u => u.Id == input.UserId);
        var userEmail = dbUserForNotif?.Email ?? "Unknown User";
        var notificationMessage = $"L'utilisateur {userEmail} vient de renseigner son humeur!";

        if (existingEntry != null)
        {
            existingEntry.Mood = input.Mood;
            context.MoodEntries.Update(existingEntry);
            await context.SaveChangesAsync();

            await notificationService.SendMoodNotificationAsync(userEmail, notificationMessage, input.UserId.ToString());

            return existingEntry;
        }
        else
        {
            var moodEntry = new MoodEntry
            {
                UserId = input.UserId,
                SprintId = input.SprintId,
                Mood = input.Mood,
                Date = entryDate
            };

            context.MoodEntries.Add(moodEntry);
            await context.SaveChangesAsync();

            await notificationService.SendMoodNotificationAsync(userEmail, notificationMessage, input.UserId.ToString());

            return moodEntry;
        }
    }
}
