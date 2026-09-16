#!/usr/bin/env bash

localEnvFile=".env.local"

if [ ! -f "$localEnvFile" ]; then
    echo "First we need to setup your environment"
    echo "We need your local development password"
    
    read -s PASS
    
    echo "POSTGRES_PASSWORD" > "$localEnvFile"
    
    echo "Development environment set !"
fi

podman compose up -d
