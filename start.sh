#!/usr/bin/env bash

localEnvFile=".env.local"

if [[ ! -f "$localEnvFile" ]]; then
    echo "Let's setup this development environment"
    echo "Please provide database password:"
    
    read -r -s PASS
    
    echo "POSTGRES_PASSWORD=$PASS" > "$localEnvFile"
    
    dotnet user-secrets init --project GreenGenius.App/GreenGenius.App.csproj
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
    "Host=localhost;Database=green-genius;Username=postgres;Password=$PASS" --project GreenGenius.App/GreenGenius.App.csproj
    
    echo "Development environment set !"
fi

podman compose up -d
