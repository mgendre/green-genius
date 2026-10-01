# GreenGenius

All commands are run from the repository root.

## Requirements

- .NET 10 SDK
- Podman or Docker (with Podman, run `systemctl --user enable --now podman.socket`)
- .Net tools:

```shell
dotnet tool restore
```

## Start the local stack

```shell
./start.sh
```

Support both Docker and Podman.
On the first run, it generates random passwords into `.env.local`.

## Build and test

```shell
dotnet build GreenGenius.slnx
dotnet test GreenGenius.slnx
```

## EF migrations

Add a migration in `GreenGenius.Common.Data` with:

```shell
dotnet ef migrations add <your-migration-name> \
    -p GreenGenius.Common.Data/GreenGenius.Common.Data.csproj \
    -s GreenGenius.App/GreenGenius.App.csproj
```

## CI

GitHub workflow is in `.github/workflows/ci.yaml`.
