# AI Ads Copilot

CSV-first Google Ads audit engine. This initial scaffold contains no business rules,
CSV processing, or AI integration.

## Requirements

- .NET 10 SDK

## Projects

- `src/AIAdsCopilot.Domain`: independent domain library.
- `src/AIAdsCopilot.Application`: application library; references Domain.
- `src/AIAdsCopilot.Infrastructure`: infrastructure library; references Application.
- `src/AIAdsCopilot.Cli`: console entry point; references Application and Infrastructure.
- `tests/AIAdsCopilot.Tests`: xUnit assembly smoke tests; references all four projects.
- `samples/google-ads` and `samples/leads`: reserved for anonymized CSV samples.

## Validation

```sh
dotnet build
dotnet test
```

## Run

```sh
dotnet run --project src/AIAdsCopilot.Cli
```

The CLI currently prints the application name only.
