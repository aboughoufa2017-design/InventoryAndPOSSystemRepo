# Mobile client

Scaffold implemented for offline sync against the existing Warehouse API. Posting rules remain server-side (not reimplemented in Dart).

**Was deferred as Phase 9 last** (after Phase 12 production hardening); this directory is now a Flutter project scaffold with Drift (SQLite), secure token storage, an outbox, and push/pull stubs.

## Stack

- Flutter + Provider
- Drift / SQLite (`outbox_ops`, `products_cache`, `balances_cache`)
- `flutter_secure_storage` for access/refresh tokens
- `connectivity_plus` to skip sync when offline
- `mobile_scanner` for camera barcode scanning (POS + register product)
- HTTP for `/api/v1/auth`, `/api/v1/sync`, and online product create

Read [../../docs/architecture.md](../../docs/architecture.md), [../../docs/offline-first.md](../../docs/offline-first.md), and [../../docs/synchronization.md](../../docs/synchronization.md). The sync protocol matches the web client.

## Run

```bash
cd clients/mobile
flutter pub get
dart run build_runner build --delete-conflicting-outputs

# Production / physical device (default API is Render)
flutter run

# Local API on Android emulator
flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5011
```

Default `API_BASE_URL`: `https://warehouse-api-0890.onrender.com` (no trailing slash). Tenant login needs **tenant code** (e.g. `123`), email, and password.

Grant **camera** permission when prompted (Android `CAMERA`, iOS `NSCameraUsageDescription`). Typed barcode entry remains available if the camera is denied.

### Barcode flows

- **POS checkout:** Scan button fills barcode/SKU and adds the matching cached product to the cart (sync products first).
- **Register product:** Scan or type a barcode, fill name/prices/unit, create online via `POST /api/v1/products`, then upsert into the local POS cache. Requires network and an existing unit of measure.

## Layout

| Path | Role |
|------|------|
| `lib/auth/auth_repository.dart` | Login / refresh against `/api/v1/auth` |
| `lib/catalog/catalog_api.dart` | Online units list + create product |
| `lib/widgets/barcode_scanner_sheet.dart` | Shared camera barcode scanner |
| `lib/sync/outbox.dart` | Local durable mutation queue |
| `lib/sync/sync_client.dart` | Push/pull stubs for `/api/v1/sync` |
| `lib/db/app_database.dart` | Drift schema (outbox + caches) |
| `lib/screens/` | Login, home, products, **register product**, inventory ops, **POS** |
