# Praetura Loan Service

A loan origination service built for the .NET technical task. It accepts loan applications over a REST API and
assesses them in the background against eligibility rules, recording a decision log for every rule check. Decisions
are announced through a transactional outbox (the optional feature I chose).

**Stack:** .NET 8, ASP.NET Core (controllers), EF Core 8 with SQLite, FluentValidation, xUnit and NSubstitute.

## Contents

- [Running the project](#running-the-project)
- [Endpoints](#endpoints)
- [How it works](#how-it-works)
- [Configuration](#configuration)
- [Project structure](#project-structure)
- [Architecture notes](#architecture-notes)

## Running the project

**Prerequisite:** the [.NET 8 SDK](https://dotnet.microsoft.com/download) (it includes the .NET 8 runtime, which the
projects target and need in order to run).

```bash
# from the repository root
dotnet run --project src/Praetura.LoanService.API --launch-profile http
```

- The API listens on `http://localhost:5192`.
- Swagger UI is at `http://localhost:5192/swagger` (Development environment only).
- `src/Praetura.LoanService.API/Praetura.LoanService.API.http` contains ready-made requests (valid, rejected,
  not found, and several invalid ones). The requests that use the id returned by an earlier one rely on
  Visual Studio / Rider response variables; in other clients, paste the id from the `201` response.
- The `https` launch profile (`https://localhost:7063`) and the Visual Studio *IIS Express* profile
  (`https://localhost:44369`) redirect plain HTTP to HTTPS. If you use one of them, change the host variable at the
  top of the `.http` file to the HTTPS address (a trusted local development certificate is needed).
- The SQLite database (`loanservice.db`) is created automatically on startup by applying the EF Core migrations. It
  is created in the directory the app is run from, and is git-ignored.

**Run the tests:**

```bash
dotnet test
```

**Try it:** `POST` an application (see below), then watch the console. The background processor runs once at
startup and then every 60 seconds, so a new application is decided within a minute. The outbox publisher runs every
30 seconds, so the decision message is logged within 30 seconds of the decision. A `GET` of the application then
shows its status and decision log.

## Endpoints

### `POST /loan-applications`

Submits a new loan application. The application is stored with status `Pending`. Eligibility is assessed later by
the background job.

Request:

```json
{
  "name": "Alice Example",
  "email": "alice@example.com",
  "monthlyIncome": 3500,
  "requestedAmount": 8000,
  "termMonths": 36
}
```

Response `201 Created`, with a `Location` header pointing at the new application:

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "status": "Pending",
  "createdAt": "2025-05-27T10:30:00Z"
}
```

| Status | When |
| --- | --- |
| `201 Created` | The application was stored. |
| `400 Bad Request` | The input is missing or invalid. The body is `application/problem+json` with field-level errors. |

Validation (FluentValidation) rejects a missing or empty `name` or `email`, a malformed `email`, a `name` over 200
or an `email` over 320 characters, and a missing, zero or negative `monthlyIncome`, `requestedAmount` or
`termMonths`. Amounts must also fit two decimal places. Validation is deliberately separate from eligibility, so a
well-formed application with a 6-month term is **accepted** here and **rejected** later by the background job.

Example `400` body (for a malformed email and a negative amount):

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["'Email' is not a valid email address."],
    "RequestedAmount": ["'Requested Amount' must be greater than '0'."]
  }
}
```

### `GET /loan-applications/{id}`

Returns the full application record, including its decision log.

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Alice Example",
  "email": "alice@example.com",
  "monthlyIncome": 3500.0,
  "requestedAmount": 8000.0,
  "termMonths": 36,
  "status": "Approved",
  "createdAt": "2025-05-27T10:30:00Z",
  "reviewedAt": "2025-05-27T10:30:42Z",
  "decisionLog": [
    {
      "ruleName": "MinimumIncome",
      "passed": true,
      "message": "Monthly income £3,500.00 meets the minimum of £2,000.00.",
      "evaluatedAt": "2025-05-27T10:30:42Z"
    },
    {
      "ruleName": "MaximumLoanAmount",
      "passed": true,
      "message": "Requested amount £8,000.00 is within the maximum of £14,000.00 (4x monthly income).",
      "evaluatedAt": "2025-05-27T10:30:42Z"
    },
    {
      "ruleName": "TermRange",
      "passed": true,
      "message": "Term of 36 months is within the allowed range of 12-60 months.",
      "evaluatedAt": "2025-05-27T10:30:42Z"
    }
  ]
}
```

| Status | When |
| --- | --- |
| `200 OK` | The application exists. |
| `404 Not Found` | No application has that id, or the id is not a valid GUID. |

While an application is still `Pending`, `reviewedAt` is `null` and `decisionLog` is empty. All entries from one
assessment share the same `evaluatedAt`, so the order of the entries within `decisionLog` is not guaranteed.

Any unexpected error returns a generic `500` problem response. The details are logged and not returned.

## How it works

### Eligibility rules

An application is `Approved` only if **all** rules pass; otherwise it is `Rejected`. Every rule is always evaluated
(no short-circuiting) so the decision log is complete, and each check produces one `DecisionLogEntry`.

| Rule | Passes when |
| --- | --- |
| `MinimumIncome` | monthly income is at least £2,000 |
| `MaximumLoanAmount` | requested amount is at most 4 x monthly income |
| `TermRange` | term is between 12 and 60 months (inclusive) |

The limits live in `EligibilityOptions` and are read from configuration. The rules are in the Domain project and are
pure, so they are easy to unit test.

### Background processing

`LoanApplicationProcessingService` is a `BackgroundService` that runs once at startup and then every 60 seconds.
Each run:

1. Reads up to a batch (100) of `Pending` applications, oldest first.
2. Evaluates each one, builds its decision log entries, and sets `Approved` / `Rejected` and `ReviewedAt`.
3. Saves the **status change, the decision log entries and the outbox message in a single transaction** (one
   `SaveChanges`). If anything fails part-way, nothing is saved. The application stays `Pending` and is picked up
   again on the next run.

Resilience:

- A failure on one application is logged and does not stop the rest of the batch.
- A failure of a whole run (for example the database being unavailable) is logged and retried at the next
  interval. The loop never exits because of an exception.
- The cancellation token is passed through every call, and shutdown is clean.
- The `DbContext` is scoped but the hosted service is a singleton, so each run creates its own scope.

### Outbox (optional feature)

When an application is decided, a message (`LoanApplicationApproved` or `LoanApplicationRejected`) is written to the
`OutboxMessages` table in the **same transaction** as the decision. This avoids the dual-write problem. The
message exists if and only if the decision was saved.

`OutboxPublishingService` runs every 30 seconds, reads unpublished messages oldest first, publishes each one, and
marks it published (`PublishedAt`). Publishing is simulated: `LoggingMessagePublisher` writes the message to the
log behind an `IMessagePublisher` interface, which is where a real broker would plug in. The payload contains only
the application id, the decision and the review time, with no name, email or financial details.

Delivery is **at-least-once**. The message is published and then marked. If the app crashes in between, the message
is published again, so consumers must be idempotent. A failure on one message is logged and leaves it unpublished
for the next run without affecting the rest of the batch.

## Configuration

All settings are in `src/Praetura.LoanService.API/appsettings.json`, and are validated at startup (the app refuses to
start with nonsensical values, for example a minimum term above the maximum).

| Section | Setting | Default |
| --- | --- | --- |
| `ConnectionStrings` | `LoanService` | `Data Source=loanservice.db` |
| `EligibilityOptions` | `MinimumMonthlyIncome` | `2000` |
| | `IncomeMultiple` | `4` |
| | `MinimumTermMonths` / `MaximumTermMonths` | `12` / `60` |
| `ProcessingOptions` | `IntervalSeconds` / `BatchSize` | `60` / `100` |
| `OutboxOptions` | `IntervalSeconds` / `BatchSize` | `30` / `50` |

## Project structure

```
src/
  Praetura.LoanService.Domain          Entities, eligibility rules, repository and publisher interfaces. No dependencies.
  Praetura.LoanService.Infrastructure  EF Core DbContext, configurations, migrations, repositories, logging publisher.
  Praetura.LoanService.API             Controller, features, hosted services, configuration and error handling.
tests/
  Praetura.LoanService.UnitTests       xUnit tests (NSubstitute for the repositories and publisher).
```

The dependencies point inwards: `API -> Domain <- Infrastructure`. The Domain defines what it needs (for example
`ILoanApplicationRepository`) and Infrastructure implements it, so the business rules know nothing about EF Core.

The API project is organised by feature (vertical slices). Each use case keeps its request, response, validator and
handler together:

```
API/Features/
  LoanApplications/
    LoanApplicationsController.cs
    CreateLoanApplication/        request, response, validator, handler
    GetLoanApplication/           response, handler
    ProcessLoanApplications/      background job: handler, hosted service, options
  Outbox/
    PublishOutboxMessages/        background job: handler, hosted service, options
```

## Architecture notes

### What would I change for 5,000,000 applications per day?

That is roughly 58 submissions per second on average, and several hundred per second at peak. Each application also
produces about three decision log rows and an outbox message, so around 15 million log rows and 5 million messages
a day. This design would not cope as it is.

In order of importance:

1. **Replace SQLite with a server database** (PostgreSQL or SQL Server), tuned for write throughput. SQLite allows a
   single writer, and the API, the processor and the publisher all write to it.
2. **Replace polling with a queue.** Instead of a job scanning for `Pending` rows, the API would publish an
   "application submitted" message to a broker (Service Bus, Kafka, SQS, etc), and a pool of consumers would decide
   applications. That gives backpressure, retries and a dead-letter queue, and removes the polling query.
3. **Scale out the workers and make that safe.** Run the API and the workers as separate deployments that scale
   independently. Multiple workers need to claim work without colliding. With a database, `SELECT ... FOR UPDATE
   SKIP LOCKED` (or an optimistic concurrency token on the status), or partition the queue by application id.
4. **Replace the polling outbox publisher** with change data capture, or a relay that streams
   committed outbox rows to the broker. Add a retry limit and a dead-letter state, and purge published rows.
5. **Handle data volume.** Partition the `DecisionLogEntries` and `OutboxMessages` tables by date, archive or expire
   old rows, and keep indexes narrow. Batch inserts.
6. **Protect the front door.** Rate limiting, request size limits, and the idempotency key from the brief's other
   optional feature, so client retries cannot create duplicate applications.
7. **Scale reads.** `GET` served from read replicas or a cache.
8. **Operate it.** Structured logging with correlation ids, metrics (queue depth, time to decision, failure rate),
   distributed tracing, health checks and alerts on the age of the oldest pending application.

### Shortcuts and trade-offs

- **SQLite.** Chosen so the project runs with no setup. It stores decimals as text and does not enforce column
  lengths or precision. Input validation is the real guard here, and the migrations would give the right columns on
  a server database.
- **Migrations run on startup.** Convenient for this task. In production they would run as a separate deployment
  step, because several instances starting at once can race, and the app's database account should not need
  schema-change rights.
- **Single-instance assumption.** Two instances of the app could pick up the same pending application. There is no
  claim or concurrency check yet (see point 3 above).
- **No retry limit or failed state.** An application or outbox message that fails every time is retried forever
  (and logged each time), and always sits near the front of the queue. It does not block the rest of a batch, but
  it should be capped and moved to a failed state.
- **At-least-once outbox with a simulated publisher.** Publishing only logs. Consumers would need to be idempotent.
- **Polling, not events.** Simple and robust for this size, but it adds latency of up to one interval.
- **Entities have public setters.** Pragmatic for the time. With more time I would encapsulate state changes in
  methods such as `Approve()` / `Reject()` and keep the invariants inside the entity.
- **Rules are private methods in one evaluator class.** A rule-per-class design (`IEligibilityRule`) would be
  more extensible but heavier than three rules justify. Limits are configurable, but the rules themselves are not.
- **No authentication or authorisation.** Anyone who can reach the API can submit applications and read any
  application by id, including the applicant's name and email. In production the API would sit behind authentication
  (for example JWT bearer tokens), a caller would only be able to read their own applications, and the
  applicant's details would be treated as personal data. This was out of scope for the task.
- **Tests are unit tests.** They cover the eligibility rules and both background handlers, using substitutes for the
  repositories. With more time I would add integration tests against a real SQLite database, which would also prove
  the transactional guarantees (decision, log entries and outbox message saved together or not at all), and
  API-level tests.
