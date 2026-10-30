# Offline-first

Status: policy locked. **Flutter** client scaffold under `clients/mobile` (Drift SQLite, outbox, auth, sync push/pull stubs). Web IndexedDB outbox remains deferred; web sync UI calls pull directly.

Offline behavior is part of the product. The client uses its local database when Wi-Fi is down, the public internet is down, or the API is down. It does not wait on a request to decide whether a local save succeeded.

## Local data

| Client | Store | Queue |
| --- | --- | --- |
| Web / later Tauri | IndexedDB (Dexie) | Outbox in the same IndexedDB database |
| Flutter | SQLite (Drift) | Outbox in the same SQLite file |

The local database holds the slice the signed-in user is allowed to use: their tenant, assigned warehouses, catalog needed for those warehouses, balances for those warehouses (including in-transit quantities the device may need), and documents they created locally that are not on the server yet. It is not a replica of every warehouse in the tenant.

A warehouse worker assigned to Warehouse A does not download balances for Warehouse B.

## Device id

The first launch creates a UUID device id and keeps it in local storage that survives restarts. The device registers that id with the server after login. The server can revoke it. The client sends the device id with sync. Reinstalling the app creates a new device id unless the platform gives a stable reinstall identity; a new id is acceptable and must be registered again. Two installs must not share an id.

Device persistence (already in the database foundation) tracks:

- DeviceKey
- Status (Active / Revoked)
- ApplicationVersion
- LastSuccessfulSyncAtUtc
- LastSuccessfulOnlineAuthenticationAtUtc

## Local writes

A user action that changes data commits one local transaction:

1. The business rows, using client-generated UUIDs.
2. The outbox row: `operationId`, `tenantId`, `deviceId`, `userId`, `entityType`, `entityId`, `operationType`, `payload`, `createdAt`, `status = pending`, `retryCount = 0`.

The UI then says the work was saved on this device. Status becomes pending sync, not "waiting for server" as a blocking state. The user keeps working.

The outbox row is not written in a second transaction after the business rows. If the process dies, either both are present or neither is.

## Connectivity and sync status

The UI has explicit states: Online, Offline, Synchronizing, Synced, Pending, Sync error.

The client detects connectivity by reachability of the API, not only by the operating system's network flag.

## OfflineSessionPolicy / DeviceSecurityPolicy (locked)

```text
MaxOfflineDays                      default 7 (tenant-configurable)
LastSuccessfulOnlineAuthentication  timestamptz UTC
RequireOnlineRevalidation           bool
DeviceRevoked                       bool
UserDisabled                        bool
TenantSuspended / IsSuspended       bool
```

### Default policy

- **MaxOfflineDays = 7** after the last successful online authentication/revalidation.
- Named constant: `OfflineSessionPolicy.DefaultMaxOfflineDays`. Do not hard-code the literal `7` inside business handlers; read tenant configuration.
- A tenant may change MaxOfflineDays later (including explicitly setting null only if product policy allows disabling the time stop).
- When the window expires: reconnect, revalidate authentication/session, and block normal offline business operations until revalidated.
- `RequireOnlineRevalidation` may also force an online check before new local operations.

### Security events and next connect

| Flag | Effect when the device next connects |
| --- | --- |
| `DeviceRevoked` | Authentication and sync apply are rejected for that device. |
| `UserDisabled` | Authentication and sync apply are rejected for that user. |
| `TenantSuspended` | Authentication and sync apply are rejected for that tenant. |

While the device is still offline, the server cannot push these flags. On the next connection the client updates local policy fields from the server response.

### Physical compromise

Data already on disk remains readable if the device is stolen. Refresh tokens must not live in IndexedDB or the SQLite business file. Revocation takes effect on the next connection.

Device registration fields and tenant offline policy columns already exist on `devices` / `tenants`. The complete offline engine is not implemented in Phase 1.
