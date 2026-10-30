# Dump local warehouse Postgres and/or restore a dump into Render Postgres (External URL).
#
# Dump only:
#   .\scripts\restore-local-to-render.ps1 -DumpOnly
#
# Restore an existing dump to Render (paste External Database URL from Render → Postgres → Connect):
#   .\scripts\restore-local-to-render.ps1 -DumpPath .\backups\warehouse-local-….dump -RenderDatabaseUrl "postgresql://USER:PASSWORD@HOST/DB?sslmode=require"
#
# Dump local then restore to Render:
#   .\scripts\restore-local-to-render.ps1 -RenderDatabaseUrl "postgresql://…"
#
# Requires: PostgreSQL client tools (pg_dump / pg_restore) on PATH, or -PgBin "C:\Program Files\PostgreSQL\18\bin"

[CmdletBinding()]
param(
    [string]$RenderDatabaseUrl = $env:RENDER_DATABASE_URL,
    [string]$DumpPath,
    [switch]$DumpOnly,
    [string]$LocalHost = "localhost",
    [int]$LocalPort = 5432,
    [string]$LocalUser = "warehouse",
    [string]$LocalDb = "warehouse",
    [string]$LocalPassword = "change-me",
    [string]$PgBin = "C:\Program Files\PostgreSQL\18\bin",
    [string]$BackupsDir = "backups"
)

$ErrorActionPreference = "Stop"

function Resolve-PgTool([string]$name) {
    $candidate = Join-Path $PgBin "$name.exe"
    if (Test-Path $candidate) { return $candidate }
    $onPath = Get-Command $name -ErrorAction SilentlyContinue
    if ($onPath) { return $onPath.Source }
    throw "Cannot find $name. Install PostgreSQL client tools or pass -PgBin."
}

$pgDump = Resolve-PgTool "pg_dump"
$pgRestore = Resolve-PgTool "pg_restore"

if (-not $DumpPath) {
    New-Item -ItemType Directory -Force -Path $BackupsDir | Out-Null
    $stamp = Get-Date -Format "yyyyMMddTHHmmssZ"
    $DumpPath = Join-Path $BackupsDir "warehouse-local-$stamp.dump"
    Write-Host "Dumping local $LocalDb → $DumpPath"
    $env:PGPASSWORD = $LocalPassword
    & $pgDump -Fc -h $LocalHost -p $LocalPort -U $LocalUser -d $LocalDb -f $DumpPath
    if ($LASTEXITCODE -ne 0) { throw "pg_dump failed with exit $LASTEXITCODE" }
    Get-Item $DumpPath | Format-List FullName, Length
}

if ($DumpOnly) {
    Write-Host "DumpOnly set — skipping Render restore."
    exit 0
}

if ([string]::IsNullOrWhiteSpace($RenderDatabaseUrl)) {
    Write-Host @"
Dump ready: $DumpPath

To restore onto Render:
  1. Render Dashboard → warehouse-db → Connect → External Database URL (sslmode=require)
  2. Re-run:

     `$env:RENDER_DATABASE_URL = 'postgresql://USER:PASSWORD@HOST/DB?sslmode=require'
     .\scripts\restore-local-to-render.ps1 -DumpPath '$DumpPath'

Or pass -RenderDatabaseUrl 'postgresql://…'
"@
    exit 0
}

if (-not (Test-Path $DumpPath)) {
    throw "Dump file not found: $DumpPath"
}

Write-Host "Restoring $DumpPath → Render (this replaces existing objects with --clean --if-exists)…"
# pg_restore accepts a connection URI via -d
& $pgRestore --clean --if-exists --no-owner --no-acl -d $RenderDatabaseUrl $DumpPath
if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne 1) {
    # pg_restore exit 1 often means non-fatal warnings (e.g. missing roles)
    throw "pg_restore failed with exit $LASTEXITCODE"
}
Write-Host "Restore finished (exit $LASTEXITCODE). Restart warehouse-api on Render if it was running."
