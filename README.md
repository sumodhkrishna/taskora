# Taskora

Taskora is a small full-stack task management app focused on the essentials of a production-minded MVP. The feature set stays intentionally focused, while still prioritizing choices that feel practical and maintainable rather than treating the app like a throwaway demo.

The app includes:

- a .NET backend API
- a React frontend
- JWT-based authentication
- refresh token support
- email verification flow
- password reset flow
- todo creation, editing, completion, reopening, and deletion

The project structure keeps responsibilities separated and easy to reason about. The backend is split into API, application, domain, and infrastructure concerns, and the frontend is organized by feature so auth and todo flows stay easy to grow.

## Tech stack

### Backend

- ASP.NET Core
- EF Core
- SQLite
- JWT authentication

### Frontend

- React
- TypeScript
- Vite
- Axios
- React Router

## Project structure

```text
backend/
  Sumodh.Taskora/                API project
  Sumodh.Taskora.Application/    use cases and handlers
  Sumodh.Taskora.Domain/         domain models
  Sumodh.Taskora.Infra/          persistence and auth infrastructure
  Sumodh.Taskora.Test/           unit tests

frontend/
  taskora-web/                   React app
```

## Features implemented

- User registration
- Email verification before first login
- User login
- Access token and refresh token flow
- Forgot password and reset password flow with SendGrid email delivery
- Protected todo endpoints
- Create, update, delete, complete, and reopen todos
- Current-user scoped todo access
- Todo list pagination, status filtering, priority filtering, search, and due-date filtering
- Request validation on write operations and todo list query parameters
- Authenticated user profile endpoints with self-only access by id
- Health check endpoint
- Global exception handling
- Basic rate limiting around auth-sensitive operations

## Setup

### Prerequisites

- .NET 10 SDK
- Node.js and npm
- A trusted local ASP.NET HTTPS dev certificate
- Visual Studio 2026 or newer to open the solution file at [backend/Sumodh.Taskora.slnx](c:/Users/sumod/source/repos/Sumodh.Taskora/backend/Sumodh.Taskora.slnx)

The backend targets `.NET 10` in [backend/Sumodh.Taskora/Sumodh.Taskora.Api.csproj](c:/Users/sumod/source/repos/Sumodh.Taskora/backend/Sumodh.Taskora/Sumodh.Taskora.Api.csproj). The repository uses a `.slnx` solution file, which requires Visual Studio 2026 or newer if the project is being opened in Visual Studio.

If needed:

```powershell
dotnet dev-certs https --trust
```

## Running the app

### Option 1: use the helper script

From the repo root:

```powershell
.\start-taskora.ps1
```

The script will:

- create the frontend `.env` file if it does not already exist
- restore backend NuGet packages
- install frontend packages automatically with `npm ci` when `package-lock.json` is present, otherwise `npm install`
- wait for the backend and frontend to become reachable
- open the frontend in the default browser

This starts:

- backend: `https://localhost:7002`
- frontend: `http://localhost:5173`

### Option 2: start each app manually

Backend:

```powershell
dotnet run --project .\backend\Sumodh.Taskora\Sumodh.Taskora.Api.csproj --launch-profile https
```

Frontend:

```powershell
cd .\frontend\taskora-web
npm install
npm run dev
```

## Configuration

The frontend expects this value in [frontend/taskora-web/.env](c:/Users/sumod/source/repos/Sumodh.Taskora/frontend/taskora-web/.env):

```env
VITE_API_BASE_URL=https://localhost:7002
```

The backend currently uses SQLite with this default connection string in [backend/Sumodh.Taskora/appsettings.json](c:/Users/sumod/source/repos/Sumodh.Taskora/backend/Sumodh.Taskora/appsettings.json):

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=todoapp.db"
}
```

JWT settings are also configured there. The current secret is development-friendly, but a real deployment should move that into environment-specific secrets management immediately.

Password reset email delivery is configured through the `SendGrid` section in [backend/Sumodh.Taskora/appsettings.json](c:/Users/sumod/source/repos/Sumodh.Taskora/backend/Sumodh.Taskora/appsettings.json):

```json
"SendGrid": {
  "ApiKey": "YOUR_SENDGRID_API_KEY",
  "FromEmail": "verified-sender@example.com",
  "FromName": "Taskora",
  "PasswordResetUrl": "http://localhost:5173/reset-password",
  "EmailVerificationUrl": "http://localhost:5173/verify-email"
}
```

Those values can be overridden per environment, and the API key should be supplied through user secrets or environment variables outside local demo use.

In development, password reset and email verification messages are also surfaced directly in the UI as preview cards so the flows can be tested without relying on external delivery.

## API notes

The backend applies EF Core migrations on startup, so the SQLite database file is created automatically when the API starts.

Todo endpoints are authenticated and scoped to the signed-in user. The todo list endpoint supports pagination plus status, priority, search, and due-date filters, and the query contract rejects invalid filter values instead of silently accepting them.

The user profile API exposes both `GET /api/users/me` and `GET /api/users/{id}`. The `id` route is restricted so authenticated users can only fetch their own profile data.

Password reset requests now send a reset email through SendGrid instead of returning the raw token in the API response. The email includes both a clickable reset link and the token for manual entry.

Newly registered users must verify their email before they can sign in. Verification emails and resend flows use the same delivery configuration, and development mode shows the generated email details directly in the frontend.

During development, OpenAPI is enabled, so the API can be explored through Swagger at:

```text
https://localhost:7002/swagger
```

There is also a health endpoint at:

```text
https://localhost:7002/health
```

## Testing

Backend tests are available in [backend/Sumodh.Taskora.Test](c:/Users/sumod/source/repos/Sumodh.Taskora/backend/Sumodh.Taskora.Test).

The unit test suite covers core auth flows, todo command/query handlers, domain behavior, exception handling, todo list query normalization, query-contract validation, and user access-control branches.

To run them:

```powershell
dotnet test .\backend\Sumodh.Taskora.Test\Sumodh.Taskora.Test.csproj
```

## Architecture notes

A layered backend structure keeps the code easier to explain and extend:

- the API layer handles HTTP concerns
- the application layer contains the use cases
- the domain layer contains the core entities
- the infrastructure layer handles persistence and auth implementations

For a small app, this is a bit more structure than strictly necessary, but it keeps business logic from leaking into controllers or framework code.

On the frontend, code is grouped by feature instead of by file type. That approach is easier to maintain as auth and todo flows grow independently.

## Why these decisions

- The backend is split into API, application, domain, and infrastructure layers so HTTP concerns, business rules, and implementation details can evolve independently instead of being mixed inside controllers.
- Controllers are intentionally thin so routing, status codes, and request binding stay in the API layer while use-case logic remains testable outside ASP.NET Core.
- Request and response DTOs are separated from domain entities so the HTTP contract can stay stable even if persistence or domain internals change.
- Command and query handlers make each use case explicit, which keeps auth, todo, and user flows easier to test and reason about than pushing that logic into controllers or repositories.
- Domain entities hold core invariants such as required titles, valid priority values, and state transitions so correctness is enforced where data changes, not only at the API boundary.
- EF Core was chosen to get real relational persistence, migrations, indexing, and query behavior without hand-rolling data access concerns.
- SQLite was chosen as the default database because it keeps local setup simple while still demonstrating a real persisted data model and migration workflow.
- Repositories isolate EF Core query details from the application layer so use cases depend on abstractions rather than persistence technology.
- Migrations are applied on startup to reduce local setup friction and make the project runnable with fewer manual steps.
- JWT access tokens plus refresh tokens were chosen because they fit a separate frontend/backend architecture better than cookie-only sessions and demonstrate more realistic session handling.
- A current-user abstraction is used so handlers do not depend directly on `HttpContext`, which keeps user-scoped behavior easier to test.
- Email verification and password reset are separated behind token generators and email sender interfaces so token creation, transport, and business flow are not tightly coupled.
- Development uses console-backed email previews while production uses SendGrid-backed senders so the same flows are easy to test locally but still map cleanly to a deployed environment.
- Rate limiting is applied around auth-sensitive endpoints because login, verification, password reset, and token refresh flows are the highest-value abuse targets in this app.
- Global exception handling with `ProblemDetails` was added to keep error responses more consistent and to avoid duplicating exception-to-response mapping inside controllers.
- Health checks and OpenAPI were included because even a small app benefits from basic operability and easier inspection during development.
- The frontend is organized by feature so auth and todo behavior can grow independently without scattering related logic across generic folders.
- A centralized Axios client handles bearer-token attachment and refresh-token retry logic in one place so auth behavior stays consistent across the frontend.

## Assumptions

- The product is a personal task manager, so every todo belongs to exactly one authenticated user and there are no shared lists, teams, or delegated ownership flows.
- Email verification is required before first login, but the app assumes users have access to the mailbox they registered with and does not implement manual admin override flows.
- Password reset and email verification depend on valid frontend URLs plus a configured sender. In development, those flows can be previewed without external delivery, but production assumes environment-specific email configuration is already in place.
- The todo workload is expected to stay modest enough for SQLite to be a reasonable default for local and small-scale deployments.
- Todo querying assumes a single-user list view with pagination and basic filters rather than advanced reporting, saved views, or cross-user search.
- Authentication assumes a single frontend client using bearer tokens and refresh tokens, rather than multiple first-party apps with device/session management.

## Trade-offs

- SQLite keeps setup simple while still demonstrating a real persistence layer. For this use case, it stays lightweight while still covering realistic persistence concerns.
- The product scope is centered on authentication and personal task management rather than collaboration features, teams, labels, notifications, or file attachments.
- The current email verification and password reset flows send email through SendGrid, but they still stop short of a fuller production workflow such as delivery retries, queueing, templates, or bounce monitoring.
- Email delivery depends on a configured SendGrid sender and valid frontend verification/reset URLs. In production, those settings should be environment-specific and managed as secrets.
- Authentication uses JWTs plus refresh tokens, which is more realistic than a single token approach, but it also adds extra complexity. That trade-off is reasonable because session handling is usually important in any app that includes login.
- Rate limiting, health checks, exception handling, and request validation are included because they add strong MVP value without much overhead. They are not exhaustive security or operations features, but they move the project in a more production-minded direction.

## Scalability thoughts

If the project needed to grow beyond its current scope, the next areas of focus would be:

- moving secrets and environment settings out of appsettings into proper secret management
- replacing SQLite with a production database such as PostgreSQL or SQL Server or even nosql
- expanding request-level error contracts and adding more standardized API documentation examples
- adding structured logging and monitoring
- introducing integration tests for key API flows
- improving token revocation and session management
- hardening email delivery for password reset and account verification with retries, templates, and delivery monitoring
- expanding sorting and richer task organization for larger todo lists

## What I would build next

The next improvements to prioritize would be:

- richer sorting and task organization beyond the current filter set
- optimistic UI updates on the frontend
- stronger form validation and user feedback states
- richer session and account-security controls around refresh tokens and verification flows
- containerized local setup
- CI for test and lint validation
- role-based or team-based expansion if the product direction called for shared workspaces

## Contact

For questions, feedback, or collaboration:

- Email: `sumodhkrishna@live.com`
- Website: https://www.sumodh.com/

## Final note

Taskora is intended to be more than a simple CRUD app. The project emphasizes separation of concerns, practical MVP choices, and the operational details that make a codebase easier to maintain beyond the initial implementation stage.
