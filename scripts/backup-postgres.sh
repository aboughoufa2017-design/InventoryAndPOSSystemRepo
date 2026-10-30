#!/usr/bin/env bash
# Backup PostgreSQL from the compose stack.
# Usage: ./scripts/backup-postgres.sh [output-dir]
set -euo pipefail

OUT_DIR="${1:-./backups}"
mkdir -p "$OUT_DIR"
STAMP="$(date -u +%Y%m%dT%H%M%SZ)"
FILE="${OUT_DIR}/warehouse-${STAMP}.dump"

COMPOSE="${COMPOSE:-docker compose}"
CONTAINER="$($COMPOSE ps -q postgres)"
if [[ -z "$CONTAINER" ]]; then
  echo "postgres container is not running" >&2
  exit 1
fi

USER="${POSTGRES_USER:-warehouse}"
DB="${POSTGRES_DB:-warehouse}"

docker exec -t "$CONTAINER" pg_dump -Fc -U "$USER" "$DB" > "$FILE"
echo "Wrote $FILE"
ls -lh "$FILE"
