#!/usr/bin/env bash
# Restore a pg_dump -Fc file into the compose postgres service.
# Usage: ./scripts/restore-postgres.sh path/to/warehouse-....dump
set -euo pipefail

DUMP="${1:?dump file required}"
COMPOSE="${COMPOSE:-docker compose}"
CONTAINER="$($COMPOSE ps -q postgres)"
if [[ -z "$CONTAINER" ]]; then
  echo "postgres container is not running" >&2
  exit 1
fi

USER="${POSTGRES_USER:-warehouse}"
DB="${POSTGRES_DB:-warehouse}"

docker cp "$DUMP" "$CONTAINER:/tmp/restore.dump"
docker exec -t "$CONTAINER" pg_restore -U "$USER" -d "$DB" --clean --if-exists /tmp/restore.dump
docker exec -t "$CONTAINER" rm -f /tmp/restore.dump
echo "Restore completed into $DB"
