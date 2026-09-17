# GreenGenius

## Requirements

Install dotnet tools:

```shell
dotnet tool restore
```

## How to start

Just run `start.sh`.

## How to

### Migration

Run a migration in `Domain` with:

```shell
dotnet ef migrations add <your-migration-name> \
    -p GreenGenius.Common.Domain/GreenGenius.Common.Domain.csproj \
    -s GreenGenius.App/GreenGenius.App.csproj
```
