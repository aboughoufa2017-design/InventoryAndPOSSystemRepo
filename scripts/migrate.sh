#!/usr/bin/env bash
# Run EF migrations once against the prod compose DB.
# Usage: ./scripts/migrate.sh
set -euo pipefail

COMPOSE="${COMPOSE:-docker compose -f docker-compose.yml -f docker-compose.prod.yml}"
$COMPOSE --profile migrate run --rm migrate
