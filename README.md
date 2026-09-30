# GreenGenius

## Requirements

Install dotnet tools:

```shell
dotnet tool restore
```

### Testcontainers and Podman

Special case for podman, you need to configure podman.socket

```
systemctl --user enable --now podman.socket
```

## How to start

Just run `start.sh`.

## How to

### Run EF Migrations

Run a migration in `GreenGenius.Common.Data` with:

```shell
dotnet ef migrations add <your-migration-name> \
    -p GreenGenius.Common.Data/GreenGenius.Common.Data.csproj \
    -s GreenGenius.App/GreenGenius.App.csproj
```
