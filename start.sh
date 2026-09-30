#!/usr/bin/env bash
set -euo pipefail

localEnvFile=".env.local"
appProject="GreenGenius.App/GreenGenius.App.csproj"

if [[ ! -f "$localEnvFile" ]]; then
    echo "Setting up the development environment"

    adminPassword=$(openssl rand -hex 24)
    appPassword=$(openssl rand -hex 24)

    (
        umask 077
        printf 'POSTGRES_PASSWORD=%s\nAPP_DB_PASSWORD=%s\n' "$adminPassword" "$appPassword" > "$localEnvFile"
    )

    dotnet user-secrets init --project "$appProject"
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
        "Host=localhost;Database=green-genius;Username=greengenius;Password=$appPassword" --project "$appProject"

    echo "Development environment set, passwords stored in $localEnvFile"
fi

chmod 600 "$localEnvFile"

set -a
[[ -f .env ]] && source .env
source "$localEnvFile"
set +a

podman compose up -d
