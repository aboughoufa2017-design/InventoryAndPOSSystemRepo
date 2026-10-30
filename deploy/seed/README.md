# Seed dump for first-start restore

Place `warehouse.dump` here (custom format from `pg_dump -Fc`).

On API start, if the Postgres `public` schema is empty and this file exists, the API runs `pg_restore` once, then applies EF migrations.

Refresh the dump from local data:

```powershell
$env:PGPASSWORD = 'change-me'
& 'C:\Program Files\PostgreSQL\18\bin\pg_dump.exe' -Fc -h localhost -U warehouse -d warehouse -f deploy/seed/warehouse.dump
```

Force restore over existing data (destructive): set `WAREHOUSE_RESTORE_ON_START=true` on the API service, redeploy once, then remove that env var.
