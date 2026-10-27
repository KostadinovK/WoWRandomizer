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

### Planned

- Save and browse character combinations (PostgreSQL)
- Simple web UI (Blazor WebAssembly)
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
- **Docker** (Linux containers)
- **PostgreSQL** (planned)
- **Blazor WebAssembly** (planned)

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker (optional, only for running in a container)

### Run locally

```bash
git clone https://github.com/KostadinovK/WoWRandomizer.git
cd WoWRandomizer/WoWRandomizer.API
dotnet run
```

Once the API is running, open the Swagger UI at `/swagger` on the URL printed in the console (when enabled in the Development environment).

### Run tests

```bash
dotnet test
```

## Project structure

The project follows Clean Architecture: the Domain holds pure business logic with zero dependencies, and the API stays thin (endpoint definitions and DI wiring). Tests live in separate projects.

```
WoWRandomizer/
├── WoWRandomizer.API/    # Web API (thin: controllers and DI wiring)
├── ...                   # Domain and test projects are added as stories land
├── CLAUDE.md             # Conventions for AI-assisted development
└── README.md
```

## Contributing

Work is tracked as user stories in [GitHub issues](https://github.com/KostadinovK/WoWRandomizer/issues). Each story is developed on its own branch named `feat/us-x` (where `x` is the story number) and merged through a pull request against `master`.

Every pull request to `master` is built and tested by the [CI workflow](.github/workflows/ci.yml) (GitHub Actions, check name `build-and-test`). Please make sure it is green before merging.

## License

Licensed under the [Apache License 2.0](LICENSE).
