#!/usr/bin/env bash
set -euo pipefail

localEnvFile=".env.local"
engine=$(command -v podman >/dev/null && echo podman || echo docker)
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
    printf '{"ConnectionStrings:DefaultConnection":"Host=localhost;Database=green-genius;Username=greengenius;Password=%s"}' "$appPassword" \
        | dotnet user-secrets set --project "$appProject"

    echo "Development environment set, passwords stored in $localEnvFile"
fi

chmod 600 "$localEnvFile"

set -a
[[ -f .env ]] && source .env
source "$localEnvFile"
set +a

"${engine}" compose up -d
