# Seed dump for first-start restore

Use **plain SQL** (`warehouse.sql`) so Restore works across Postgres client/server versions.
Custom `-Fc` dumps from PostgreSQL 18 (`archive version 1.16`) fail with older `pg_restore` in the API image.

On API start, if this file exists and the DB has **no tenants**, the API runs `psql -f warehouse.sql`, then EF migrations.

Refresh from local data:

```powershell
$env:PGPASSWORD = 'change-me'
& 'C:\Program Files\PostgreSQL\18\bin\pg_dump.exe' -Fp --clean --if-exists --no-owner --no-acl `
  -h localhost -U warehouse -d warehouse -f deploy/seed/warehouse.sql
# Strip PG 18-only lines (also stripped automatically at restore time):
(Get-Content deploy/seed/warehouse.sql) |
  Where-Object {
    $_ -notmatch '^\\restrict ' -and
    $_ -notmatch '^\\unrestrict ' -and
    $_ -notmatch '^SET transaction_timeout'
  } |
  Set-Content deploy/seed/warehouse.sql
```

Render Postgres is **16**; local dump may be from **18** — keep the seed as plain SQL and avoid PG18-only `SET`s.

Force restore over existing data (destructive): set `WAREHOUSE_RESTORE_ON_START=true`, redeploy once, then remove it.
