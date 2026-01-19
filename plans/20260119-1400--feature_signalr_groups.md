# Implementation Plan - Real-time Team Notifications via SignalR Groups

## 1. 🔍 Analysis & Context
*   **Objective:** Implement secure, team-isolated real-time notifications using SignalR Groups. Messages should only be broadcast to members of the specific team where the event occurred.
*   **Affected Files:**
    *   `api/NikoNiko.Services/TokenService.cs` (Add team_ids claim)
    *   `api/NikoNiko.Api/Controllers/AuthController.cs` (Load teams for token)
    *   `api/NikoNiko.Notifications/Hubs/NotificationHub.cs` (Join groups on connect)
    *   `api/NikoNiko.Notifications/Controllers/NotificationsController.cs` (Dispatch to groups, manage dynamic membership)
    *   `api/NikoNiko.Services/NotificationService.cs` & `INotificationService.cs` (Update signatures)
    *   `api/NikoNiko.Api/Controllers/MoodEntriesController.cs` (Pass teamId)
    *   `api/NikoNiko.Api/Controllers/TeamInvitationsController.cs` (Trigger group join)
    *   `api/NikoNiko.Api/Controllers/TeamsController.cs` (Trigger group leave)
*   **Key Dependencies:** `Microsoft.AspNetCore.SignalR`, `System.Security.Claims`.
*   **Risks/Unknowns:** Ensuring dynamic group updates work when a user accepts an invitation without requiring a re-login immediately.
*   **Frontend Impact:** Minimal. The frontend already listens for `ReceiveNotification`. The filtering logic is moved entirely to the server (Group vs All). The client does not need to know which group it is in; it simply receives messages sent to it.

## 2. 📋 Checklist
- [x] Step 1: Enrich JWT with Team IDs
- [x] Step 2: Update NotificationHub to Join Groups
- [x] Step 3: Implement Dynamic Group Management (Immediate Join/Leave)
- [x] Step 4: Update Notification Service & Controller for Group Broadcasting
- [x] Step 5: Integrate Mood Entry Notifications
- [x] Step 6: Integrate Team Invitation & Management Updates
- [x] Verification

## 3. 📝 Step-by-Step Implementation Details

### 🚀 Execution Rules
1. **Interactive Flow**: Execute ONLY ONE step at a time.
2. **Atomic Updates**: Before and after each step, you MUST use `replace` to update the checklist in section 2 and the status in section 3.
3. **Status Vocabulary**: Use `[x]` (Done), `[>]` (In Progress), `[-]` (Skipped/N.A.), `[!]` (Failed/Blocked).
4. **User Confirmation**: After updating the file, stop and wait for explicit user confirmation before proceeding to the next step.
5. **No Stealth Actions**: NEVER execute code or modify files without having first updated the plan to reflect that you are about to do so.

### Step 1: Enrich JWT with Team IDs
*   **Goal:** Include the list of Team IDs the user belongs to in the JWT token so the stateless Notification Hub can read them upon connection/reconnection.
*   **Status:** [Done]
*   **Action:**
    *   Modify `api/NikoNiko.Services/TokenService.cs`:
        *   Update `CreateToken` to iterate over `user.TeamUsers` and add a `team_id` claim for each team.
    *   Modify `api/NikoNiko.Api/Controllers/AuthController.cs`:
        *   Update `HandleSignIn` to `.Include(u => u.TeamUsers)` when fetching the user.
*   **Verification:** Run `TokenServiceTests` to verify claims are present in the generated token. (Success: 3 tests passed)

### Step 2: Update NotificationHub to Join Groups
*   **Goal:** Automatically add the user's connection to SignalR groups based on the `team_id` claims in the JWT when they connect.
*   **Status:** [Done]
*   **Action:**
    *   Modify `api/NikoNiko.Notifications/Hubs/NotificationHub.cs`:
        *   In `OnConnectedAsync`:
            *   Retrieve all `team_id` claims from `Context.User.Claims`.
            *   Loop and `await Groups.AddToGroupAsync(Context.ConnectionId, teamId);`.
*   **Verification:** Manual test: Connect with a token containing team claims and verify logs show group addition.

### Step 3: Implement Dynamic Group Management (Immediate Join/Leave)
*   **Goal:** Allow backend services to force a user's *active* connections to join/leave a group immediately (e.g., when accepting an invite), addressing the "User Added" scenario without waiting for a token refresh.
*   **Status:** [Done]
*   **Action:**
    *   Modify `api/NikoNiko.Notifications/Controllers/NotificationsController.cs`:
        *   Add endpoint `POST /manage-groups`. Payload: `{ UserId, TeamId, Action: "Add"|"Remove" }`.
        *   Use `_userConnectionManager.GetConnections(userId)` to find active connections.
        *   Call `_hubContext.Groups.AddToGroupAsync` (or Remove) for these connections.
    *   Modify `api/NikoNiko.Services/INotificationService.cs` & `NotificationService.cs`:
        *   Add `Task UpdateUserGroupAsync(string userId, Guid teamId, bool isJoining);`
        *   Implement logic to call the new endpoint.
*   **Verification:** Unit test `NotificationsController` to ensure `AddToGroupAsync` is called for the user's connections.

### Step 4: Update Notification Service & Controller for Group Broadcasting
*   **Goal:** Change the broadcasting logic to target specific groups instead of `Clients.All`.
*   **Status:** [Done]
*   **Action:**
    *   Modify `api/NikoNiko.Notifications/Controllers/NotificationsController.cs`:
        *   Update `Dispatch` to accept `TeamId` in payload.
        *   Logic: If `TeamId` is present, use `_hubContext.Clients.Group(payload.TeamId.ToString()).SendAsync(...)`.
        *   Fallback: If no `TeamId`, keep `Clients.All` (or restrict/remove depending on security reqs).
    *   Modify `api/NikoNiko.Services/INotificationService.cs` & `NotificationService.cs`:
        *   Update `SendMoodNotificationAsync` to take `Guid teamId`.
        *   Update payload serialization.
*   **Verification:** Check compiler errors (breaking change on interface).

### Step 5: Integrate Mood Entry Notifications
*   **Goal:** Ensure mood entries trigger the group notification.
*   **Status:** [Done]
*   **Action:**
    *   Modify `api/NikoNiko.Api/Controllers/MoodEntriesController.cs`:
        *   In `CreateMoodEntry`, pass the `teamId` (derived from Sprint) to `SendMoodNotificationAsync`.
*   **Verification:** Run `MoodEntriesControllerTests`.

### Step 6: Integrate Team Invitation & Management Updates
*   **Goal:** Ensure joining/leaving teams updates the real-time groups.
*   **Status:** [Done]
*   **Action:**
    *   Modify `api/NikoNiko.Api/Controllers/TeamInvitationsController.cs`:
        *   In `AcceptTeamInvitation`, after DB success, call `_notificationService.UpdateUserGroupAsync(userId, teamId, true)`.
    *   Modify `api/NikoNiko.Api/Controllers/TeamsController.cs`:
        *   In `RemoveUser`, call `_notificationService.UpdateUserGroupAsync(userId, teamId, false)`.
        *   In `UpdateTeam` (rename), ensure `NotifyTeamRenamedAsync` uses the team group (already passed teamId, just ensure internal wiring is correct).
*   **Verification:** Manual walkthrough of Invitation flow.

## 4. 🧪 Testing Strategy
*   **Unit Tests:**
    *   `TokenServiceTests`: Verify `team_id` claims.
    *   `NotificationHubTests`: Mock `HubCallerContext` with claims and verify `Groups.Add` is called.
*   **Integration Tests:**
    *   `MoodEntriesController`: Mock `INotificationService` and verify `SendMoodNotificationAsync` is called with correct `TeamId`.

## 5. ✅ Success Criteria
*   When a user posts a mood, ONLY members of that team receive the notification.
*   A user in Team A and Team B receives notifications for both.
*   A user NOT in Team A does NOT receive Team A notifications.
*   Accepting an invitation immediately enables notifications for that team without page refresh.
