# Mobile client

Scaffold implemented for offline sync against the existing Warehouse API. Posting rules remain server-side (not reimplemented in Dart).

**Was deferred as Phase 9 last** (after Phase 12 production hardening); this directory is now a Flutter project scaffold with Drift (SQLite), secure token storage, an outbox, and push/pull stubs.

## Stack

- Flutter + Provider
- Drift / SQLite (`outbox_ops`, `products_cache`, `balances_cache`)
- `flutter_secure_storage` for access/refresh tokens
- `connectivity_plus` to skip sync when offline
- HTTP stubs for `/api/v1/auth` and `/api/v1/sync`

Read [../../docs/architecture.md](../../docs/architecture.md), [../../docs/offline-first.md](../../docs/offline-first.md), and [../../docs/synchronization.md](../../docs/synchronization.md). The sync protocol matches the web client.

## Run

```bash
cd clients/mobile
flutter pub get
dart run build_runner build --delete-conflicting-outputs
flutter run --dart-define=API_BASE_URL=http://localhost:3000
```

Replace the API base URL with your backend origin (no trailing slash). Default if omitted: `http://localhost:3000`.

## Layout

| Path | Role |
|------|------|
| `lib/auth/auth_repository.dart` | Login / refresh against `/api/v1/auth` |
| `lib/sync/outbox.dart` | Local durable mutation queue |
| `lib/sync/sync_client.dart` | Push/pull stubs for `/api/v1/sync` |
| `lib/db/app_database.dart` | Drift schema (outbox + caches) |
| `lib/screens/` | Login, home (sync), products/barcode, inventory ops, **POS offline checkout** |
