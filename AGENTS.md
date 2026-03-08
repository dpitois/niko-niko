# AGENTS.md - Development Guidelines for Agentic Coding

This file contains build commands, code style guidelines, and development conventions for agentic coding agents working in the Niko Niko Calendar repository.

## Project Overview

Niko Niko Calendar is a distributed, self-hosted Docker application for Agile teams to track daily morale. It consists of:
- **Backend**: .NET 10 Web API with multiple services (main API, notifications, data layers)
- **Frontend**: React 19 + TypeScript + Vite with Material UI v7
- **Database**: PostgreSQL (production) or SQLite (development/portable)
- **Real-time**: SignalR for notifications

## Build Commands

### Backend (.NET)

```bash
# Navigate to API project
cd api/NikoNiko.Api

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the main API
dotnet run

# Format code (required after any .NET changes)
dotnet format

# Run tests
dotnet test

# Run a single test (replace with actual test method name)
dotnet test --filter "TestMethodName"

# Create Entity Framework migration
dotnet ef migrations add MigrationName --project ../NikoNiko.Data --startup-project .

# Note: NEVER run 'dotnet ef database update' - migrations auto-apply on startup
```

### Frontend (React)

```bash
# Navigate to frontend
cd app/frontend

# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Run linting and formatting (MANDATORY ORDER: lint first, then format)
npm run lint -- --fix && npm run format

# Run linting
npm run lint

# Format code
npm run format
```

### Docker Commands

```bash
# Build and run all services (detached mode)
docker compose up -d --build

# Stop all services
docker compose down

# Restart specific service
docker compose restart backend

# View logs
docker compose logs -f [service-name]
```

## Code Style Guidelines

### Backend (.NET)

#### Formatting & Style
- Follow `.editorconfig` rules strictly
- Use `dotnet format` after any changes
- 4-space indentation for C# files
- Use file-scoped namespaces: `namespace NikoNiko.Services;`

#### Naming Conventions
- **Classes/Interfaces**: PascalCase (interfaces prefixed with 'I')
- **Methods/Properties**: PascalCase
- **Local variables/parameters**: camelCase
- **Private fields**: `_camelCase` prefix
- **Private static fields**: `s_camelCase` prefix
- **Constants**: PascalCase

#### Code Organization
- **Skinny Controller Pattern**: Controllers only handle HTTP concerns (routing, binding, status codes)
- **Business Logic**: Must be in Services layer (`NikoNiko.Services`)
- **Interfaces**: Must be defined in `NikoNiko.Core/Interfaces`
- **DTOs**: In `NikoNiko.Core/DTOs`
- **Models**: In `NikoNiko.Core/Models`

#### Import Organization
- System directives first, alphabetically
- Separate import directive groups with blank lines
- Using directives outside namespaces

#### Error Handling
- Use proper HTTP status codes
- Implement validation using FluentValidation
- Handle exceptions appropriately in service layer
- Use `Result<T>` pattern for operation results where applicable

#### Database & Date Handling
- **Calendar Dates**: `DateTime.SpecifyKind(date.Date, DateTimeKind.Utc)` (NOT `.ToUniversalTime()`)
- **Point-in-Time**: `DateTime.UtcNow`
- EF Core migrations auto-apply on startup

### Frontend (React/TypeScript)

#### Formatting & Style
- Use Prettier with config in `.prettierrc`
- 2-space indentation
- Single quotes for strings
- Trailing commas required
- Max line length: 100 characters

#### Import Organization (ESLint simple-import-sort)
```typescript
// 1. Packages (React, MUI, third-party)
import React from 'react';
import { Grid, Button } from '@mui/material';
import dayjs from 'dayjs';

// 2. Local utils/services/hooks/context/models
import { useAuth } from '@/hooks/useAuth';
import { apiService } from '@/services/apiService';

// 3. Relative imports
import { Component } from './Component';

// 4. Styles (CSS modules, etc.)
import './styles.css';
```

#### TypeScript Guidelines
- Strict typing enabled
- Use interfaces for object shapes
- Prefer explicit return types for functions
- Use generic types where appropriate
- No `any` types unless absolutely necessary

#### React Patterns
- Functional components with hooks
- Use `React.memo` for performance optimization
- Custom hooks in `hooks/` directory
- Context providers for global state
- SWR for data fetching and caching

#### Material UI Usage
- Use path imports for bundle optimization: `import { Button } from '@mui/material';`
- Grid syntax: `<Grid size={{ xs: 12, sm: 6 }}>` (no deprecated `item` prop)
- Follow MUI v7 patterns and theming

#### State Management
- `AuthContext` for authentication state
- SWR for server state
- Local state with `useState`/`useReducer`
- No external state management libraries

#### Internationalization
- Use `i18next` with `useTranslation` hook
- Translations in `src/i18n/locales/`
- Always use translation keys, never hardcode text

## Development Conventions

### Architecture Principles
- **Skinny Controllers**: Minimal logic in API controllers
- **Service Layer**: All business logic in services
- **Interface Segregation**: Interfaces in Core project
- **Dependency Injection**: Use .NET DI container

### File Organization
```
api/
├── NikoNiko.Api/          # Main API project
├── NikoNiko.Core/         # DTOs, models, interfaces
├── NikoNiko.Services/     # Business logic implementations
├── NikoNiko.Data/         # EF Core context and configurations
├── NikoNiko.Data.PostgreSql/  # PostgreSQL-specific
├── NikoNiko.Data.Sqlite/  # SQLite-specific
└── NikoNiko.Notifications/ # SignalR notification service

app/frontend/
├── src/
│   ├── components/        # Reusable components
│   ├── pages/            # Page components
│   ├── hooks/            # Custom hooks
│   ├── services/         # API services
│   ├── context/          # React contexts
│   ├── models/           # TypeScript interfaces
│   └── i18n/             # Internationalization
```

### Testing
- **Backend Integration Tests**: xUnit tests in `*IntegrationTests` projects.
    - Use Moq for mocking.
    - Run: `dotnet test` or `dotnet test --filter "TestMethodName"`.
- **E2E Tests (Playwright)**: Full-stack functional tests located in `app/frontend/tests/`.
    - **Run**: `cd app/frontend && npx playwright test` (runs all tests).
    - **Run UI Mode**: `npx playwright test --ui` (interactive debugger).
    - **Architecture**:
        - Tests launch the Backend API on port 7000 and Notification Service on 7001 using `dotnet run`.
        - Tests launch the Frontend on port 5173 (or available port).
        - **Database**: Uses a dedicated isolated SQLite DB `api/NikoNiko.Api/nikoniko.e2e.db`.
        - **Data Reset**: Uses `POST /api/testing/reset` to wipe the DB and seed default users (`admin@test.com`, `member@test.com`) before tests.
    - **Maintenance**:
        - **Auth**: Use `loginAs` helper which uses the Backdoor Login (`POST /api/testing/login`) to bypass OAuth.
        - **SignalR**: `notifications.spec.ts` covers real-time scenarios but is currently **skipped** (`test.skip`) due to environment flakiness. Unskip to debug.
        - **Backend Changes**: If you modify the DB schema, ensuring `dotnet ef database update` isn't needed (auto-applied), but the `TestingController` might need updates if new required fields are added to User/Team.

### Security & Authentication
- OAuth 2.0 (GitHub, Google, Discord)
- JWT tokens with refresh token pattern
- Role-based authorization (Team Admin, Team Member)
- HttpOnly cookies for refresh tokens

### Database Management
- EF Core migrations auto-apply on startup
- Create migrations locally with `dotnet ef migrations add`
- Never manually run database updates
- Support for both PostgreSQL and SQLite

### Licensing
- Project licensed under AGPL-3.0
- All third-party packages must be compatible (MIT, Apache 2.0, BSD)
- Add new packages to `THIRD-PARTY-NOTICES.md`

### Git & Development Workflow
- All implementation plans in Markdown, English only
- Use conventional commit messages
- Format code before committing
- Run linting and type checking before commits

## Environment Configuration

### Authentication Setup (OAuth 2.0)
For development, configure your OAuth providers with these exact callback URLs:
- **GitHub**: `http://localhost:5000/signin-github`
- **Google**: `http://localhost:5000/signin-google`
- **Discord**: `http://localhost:5000/signin-discord`

### Required Environment Variables
```bash
# Authentication
JWT_KEY=your-secret-key
GITHUB_CLIENT_ID=your-github-client-id
GITHUB_CLIENT_SECRET=your-github-client-secret
GOOGLE_CLIENT_ID=your-google-client-id
GOOGLE_CLIENT_SECRET=your-google-client-secret
DISCORD_CLIENT_ID=your-discord-client-id
DISCORD_CLIENT_SECRET=your-discord-client-secret

# URLs
FRONTEND_REDIRECT_URL=http://localhost:3000/auth/callback
VITE_GITHUB_REPO_URL=https://github.com/your-repo

# Admin
SUPER_ADMINS=user@example.com
```

### Database Configuration
- **SQLite** (default): Set `DatabaseProvider=SQLite`
- **PostgreSQL**: Set `DatabaseProvider=PostgreSQL` and uncomment db service in docker-compose.yml

## Common Issues & Solutions

### Backend
- **Migration Issues**: Delete Migrations folder and recreate if switching database providers
- **Formatting**: Always run `dotnet format` after .NET changes
- **Build Errors**: Check for missing using directives and proper namespace declarations

### Frontend
- **Import Errors**: Follow ESLint simple-import-sort rules
- **Type Errors**: Ensure proper TypeScript interfaces and types
- **Build Issues**: Check Vite configuration and dependencies

### Docker
- **Port Conflicts**: Ensure ports 3000, 5000, 8080 are available
- **Database Connection**: Verify connection strings and database service health
- **Environment Variables**: Check `.env` file configuration

## Quick Start for Agents

1. **Backend Development**: `cd api/NikoNiko.Api && dotnet run`
2. **Frontend Development**: `cd app/frontend && npm run dev`
3. **Full Stack**: `docker compose up -d --build`
4. **Code Formatting**: Backend: `dotnet format`, Frontend: `npm run format`
5. **Linting**: Frontend: `npm run lint`
6. **Testing**: `dotnet test` (backend)

Remember to follow the architectural patterns, code style guidelines, and always format code before committing changes.