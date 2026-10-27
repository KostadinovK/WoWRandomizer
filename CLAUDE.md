# WoWRandomizer

A small tool that helps a player decide which race/class combo to play in WoW Forever. The first feature is a single endpoint that returns a random valid combo, optionally filtered by faction (Horde or Alliance).

## Technologies
- **.NET 10** (LTS) / C#
- **ASP.NET Core Web API** with controllers, Docker support for Linux
- **PostgreSQL** (planned) for saving character combinations
- **Frontend** (planned): Blazor WebAssembly (standalone), calling the API over HTTP
- **Aspire** may be adopted later; not used for now

## Architecture Philosophy
We follow Clean Architecture:
- **Domain** has zero dependencies - pure business logic
- **Api** is thin - just endpoint definitions and DI wiring
- Follow the **SOLID** principles.
- Every service has an **interface and an implementation** (e.g. `IComboService` and `ComboService`). Consumers depend on the interface, and services are registered in DI against their interface.

## Code Style
- Use `var` wherever the type is evident or allowed.
- Prefer LINQ with **fluent (method) syntax** over query syntax and manual loops.
- **No Mediator** (no MediatR or similar). Controllers call services directly.
- **No Mappers** (no AutoMapper or similar). Map between types explicitly in code (constructors, factory methods, or simple projections).
- Keep business logic in services/domain classes, never in controllers.

## Testing
- Tests live in a **separate project**; never mix test code into production projects.
- **Unit tests** project is required and covers Domain and services.
- An **integration tests** project is added when needed (e.g. API endpoints, database access).

## Domain Notes
- Valid race/class combos are maintained manually in a single data file (e.g. `Data/combos.json`); there is no public API for this data.
- Every race belongs to exactly one faction. There are no neutral races for now.
- Random selection is uniform across all valid combos (after applying the optional faction filter).

## Work Tracking
- User stories are tracked as GitHub issues in `KostadinovK/WoWRandomizer`.

## Git Workflow
- Each user story is developed in its own branch named `feat/us-x`, where `x` is the user story number (e.g. `feat/us-1`).
- When the story is done, open a pull request from that branch against `master`.
- Never commit directly to `master`.
- Every pull request is built and tested by GitHub Actions (`.github/workflows/ci.yml`, check `build-and-test`). Do not merge a pull request while that check is failing.
