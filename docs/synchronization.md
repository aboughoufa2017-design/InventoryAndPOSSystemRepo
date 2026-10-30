# Synchronization

Status: **implemented** (Phase 8). Push idempotency existed from foundation; pull `change_log` and sync engines are live.

Historical note: **Do not create this table in Phase 1.** The pull change-log was deferred then and is created by migration `AddSyncChangeLog`.

## Direction

1. The client notices the API is reachable.
2. It authenticates / refreshes and receives current device security flags from sync responses.
3. If `DeviceRevoked`, `UserDisabled`, or `TenantSuspended` is true, the client stops apply and surfaces the block.
4. Otherwise it **pushes** pending outbox operations (idempotent).
5. Then it **pulls** changes after its last applied `ChangeVersion`.
6. Checkpoint advances only over a contiguous applied range.

## Push

`POST /api/v1/sync/push`

Each operation:

| Field | Notes |
| --- | --- |
| OperationId | Client UUID; PK in `processed_sync_operations` |
| EntityType / OperationType | Routed to a registered handler |
| EntityId | Client UUID for the target entity |
| PayloadJson | Handler-specific JSON |
| BaseVersion | Optional optimistic concurrency (e.g. product `rowVersion`) |

Outcomes (never silent overwrite): `Applied`, `AlreadyProcessed`, `Conflict`, `Rejected`, `ValidationFailed`.

Duplicate `OperationId` returns `AlreadyProcessed` with the stored `ProcessedSyncOperation` result payload (no double-apply).

Registered handlers (Phase 8):

| EntityType | OperationType |
| --- | --- |
| Product | Upsert, Deactivate (tombstone) |
| StockAdjustment | Post |
| Transfer | Dispatch, Receive, Cancel |

Security: JWT must include device id. Revoked device / disabled user / suspended tenant → whole push rejected (`FORBIDDEN`).

## Pull

`POST /api/v1/sync/pull` with `{ afterChangeVersion, limit }`.

| Table | Role |
| --- | --- |
| `tenant_change_counters` | Monotonic `last_change_version` per tenant |
| `change_log` | Unique `(tenant_id, change_version)`; Upsert / Deactivate payloads |

Returns changes ordered by `ChangeVersion`, `latestChangeVersion`, `hasMore`, and a security snapshot.

## Conflict and retry

- Master data: `BaseVersion` mismatch → `Conflict` / `SYNC_CONFLICT`
- Inventory: insufficient stock → `Rejected` / `INSUFFICIENT_STOCK`
- Clients retry the same `OperationId`; server is idempotent

## API permission

`inventory.read` on push/pull endpoints (handlers enforce module permissions such as `products.create`).
