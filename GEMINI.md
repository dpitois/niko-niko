# Niko Niko Calendar Project

> **Technical Instructions**: For build commands, code style, environment setup, and architecture rules, see `AGENTS.md`.

## Gemini Added Memories
- The user prefers to be given the command to run the development server or start the Azure Function API, instead of being asked for permission to execute it.
- When launching the project with `docker compose up`, the user prefers the `-d` option to run services in detached mode.
- **Material UI Grid Syntax**: When using the Material UI Grid component, the correct syntax is `<Grid size={{ xs: 12, sm: 6 }}>`. The `item` prop is deprecated and should not be used.
- All implementation plans must be written in Markdown and STRICTLY in English. No French or mixed languages in plans.
- **Third-Party Licensing**: Before adding any new package (NPM, NuGet, etc.), ensure it is compatible with the project's **AGPLv3** license (e.g., MIT, Apache 2.0, BSD). If compatible, the package MUST be added to `THIRD-PARTY-NOTICES.md` with its license details.

## Project Overview

The Niko Niko Calendar is a distributed and self-hosted (via Docker) application designed to help Agile teams track their daily morale. Its core objectives are transparency of collective mood, early detection of morale drops, empathy towards team challenges, and motivation through gamification.

**Target Audience**: Agile Teams, Managers, Scrum Masters, and Project Members.

## Key Features & Domain Logic

*   **Team Management**: Team creation (via admin dashboard), member management (admin role), team invitation system (creation, acceptance, soft deletion).
*   **Sprints**: Admin-defined work periods (usually 2 weeks), sprint tracking on the dashboard.
*   **Mood Tracking**:
    *   Daily mood entry (🤩/😊/😐/☹️/😫).
    *   **Rule**: Users can specify a date, but it must be **within the current sprint range** and **cannot be in the future**.
*   **Real-time Notifications**: SignalR integration for real-time notifications on important actions.
*   **Gamification**: Badge attribution system.
*   **Dashboard**: Centralized view of teams, sprints, and calendars.
*   **Internationalization (i18n)**: Support for English and French.

## Main Data Models

*   `User`: Stores user information (OAuth details, associated teams, badges, Super Admin status).
*   `Team`: Represents an Agile team, linking an admin, members, and sprints.
*   `Sprint`: Defines a time-boxed work period with start and end dates.
*   `MoodEntry`: Records a user's mood for a specific date within a sprint.
*   `Badge`: Represents gamification rewards.
*   `TeamInvitation`: Manages invitations to join a team.