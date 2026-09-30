#!/usr/bin/env bash
set -euo pipefail

: "${APP_DB_PASSWORD:?APP_DB_PASSWORD is not set, run ./start.sh}"

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres \
    -v app_password="$APP_DB_PASSWORD" <<'SQL'
CREATE ROLE greengenius LOGIN PASSWORD :'app_password';

CREATE DATABASE "green-genius"
  WITH OWNER greengenius
  TEMPLATE template0
  ENCODING 'UTF8'
  LOCALE_PROVIDER icu
  ICU_LOCALE 'und'
  LOCALE 'en_US.utf8';
SQL
