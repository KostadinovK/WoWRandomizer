# WoWRandomizer

[![CI](https://github.com/KostadinovK/WoWRandomizer/actions/workflows/ci.yml/badge.svg)](https://github.com/KostadinovK/WoWRandomizer/actions/workflows/ci.yml)

**Can't decide what to play? Let the dice choose.**

WoWRandomizer is a small tool for **WoW Forever** players who are stuck on the classic question: *which race and class should I roll?* Ask for a character and get a random, **valid** race + class combination, optionally limited to the faction you want to play.

> **Status:** early development. The project skeleton is in place and the first features are being built. See the [issues](https://github.com/KostadinovK/WoWRandomizer/issues) for progress.

## Features

- Get a random valid race + class combo with a single request
- Optionally filter by faction (**Horde** or **Alliance**)
- Every valid combo is equally likely
- Clear error messages for invalid input
- OpenAPI/Swagger documentation for trying the API in the browser

- A Blazor WebAssembly web app (separate from the API): press **Roll** to get a random race, class and faction (Horde in red, Alliance in blue), with a loading state and a friendly error message with retry

### Planned

- Choose a faction in the web UI, plus race/class/faction icons, a dark WoW-inspired design and roll history
- Save and browse character combinations (PostgreSQL)
- Random spec selection

## API

A single endpoint returns the combo:

```http
GET /api/combo
GET /api/combo?faction=horde
GET /api/combo?faction=alliance
```

Example response:

```json
{
  "race": "Orc",
  "class": "Warrior",
  "faction": "Horde"
}
```

An unsupported faction returns `400 Bad Request` with the list of accepted values (`horde`, `alliance`). The `faction` value is case-insensitive.

## Tech stack

- **.NET 10** / C#, ASP.NET Core Web API
- **Blazor WebAssembly** (standalone) for the web UI, with **bUnit** component tests
- **Docker** (Linux containers)
- **PostgreSQL** (planned)

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker (optional, only for running in a container)

### Run locally

The API and the web UI are two separate apps, so start each one in its own terminal.

**1. The API** (`http://localhost:5078`)

```bash
git clone https://github.com/KostadinovK/WoWRandomizer.git
cd WoWRandomizer/WoWRandomizer.API
dotnet run --launch-profile http
```

Once the API is running, open the Swagger UI at `/swagger` on the URL printed in the console (when enabled in the Development environment).

**2. The web UI** (`http://localhost:5180`)

```bash
cd WoWRandomizer/WoWRandomizer.Web
dotnet run --launch-profile http
```

Then open `http://localhost:5180` in your browser and press **Roll**. The API must be running, otherwise the page shows a "couldn't reach the server" message with a **Try again** button.

### Configuration

| Setting | Where | Purpose |
| --- | --- | --- |
| `ApiBaseUrl` | `WoWRandomizer.Web/wwwroot/appsettings.json` | Base URL of the API the web UI calls (default `http://localhost:5078`). |
| `Cors:AllowedOrigins` | `WoWRandomizer.API/appsettings*.json` | Origins allowed to call the API from a browser. Empty by default; `appsettings.Development.json` allows the local web UI (`http://localhost:5180` and `https://localhost:7180`). |

To deploy the web UI somewhere else, point `ApiBaseUrl` at the API and add the web UI's origin (scheme, host and port, no trailing slash) to `Cors:AllowedOrigins`. The API can also read it from an environment variable, e.g. `Cors__AllowedOrigins__0=https://my-web-ui.example`.

When using the `https` launch profile of the API, set `ApiBaseUrl` to its `https://localhost:7097` address; a redirect from `http` to `https` would otherwise break the browser's cross-origin call.

### Run tests

```bash
dotnet test
```

## Project structure

The project follows Clean Architecture: the Domain holds pure business logic with zero dependencies, and the API stays thin (endpoint definitions and DI wiring). Tests live in separate projects.

```
WoWRandomizer/
├── WoWRandomizer.API/                  # Web API (thin: controllers and DI wiring)
├── WoWRandomizer.Domain/               # Pure business logic, no dependencies
├── WoWRandomizer.Infrastructure/       # Data access (combos.json)
├── WoWRandomizer.Shared/               # Contract types shared by the API and the web UI (ComboResponse)
├── WoWRandomizer.Web/                  # Blazor WebAssembly UI and the typed API client
├── WoWRandomizer.UnitTests/            # Unit tests (Domain, services, API client)
├── WoWRandomizer.IntegrationTests/     # API endpoint tests
├── WoWRandomizer.Web.ComponentTests/   # bUnit component tests for the web UI
├── CLAUDE.md                           # Conventions for AI-assisted development
└── README.md
```

## Contributing

Work is tracked as user stories in [GitHub issues](https://github.com/KostadinovK/WoWRandomizer/issues). Each story is developed on its own branch named `feat/us-x` (where `x` is the story number) and merged through a pull request against `master`.

Every pull request to `master` is built and tested by the [CI workflow](.github/workflows/ci.yml) (GitHub Actions, check name `build-and-test`). Please make sure it is green before merging.

## License

Licensed under the [Apache License 2.0](LICENSE).
