# Authentication

Status: **implemented** (Phase 2). Login, refresh rotation, logout, current user, and device registration are live under `/api/v1/auth` and `/api/v1/devices`.

## Accounts

A user belongs to one tenant in the first release. Passwords are hashed with ASP.NET Core `PasswordHasher` (never plaintext, never logged).

## Tokens

| Token | Lifetime | Storage on server | Storage on web | Storage on mobile |
| --- | --- | --- | --- | --- |
| Access JWT | 15 minutes (configurable) | Not stored | Memory only | Memory only |
| Refresh | 14 days sliding, 90 days absolute (configurable) | **SHA-256 hash only** (`TokenHash`), plus family id | Prefer `HttpOnly` `Secure` cookie (CSRF later) | Platform secure storage |

### `refresh_tokens` table

| Column | Notes |
| --- | --- |
| Id | UUID PK |
| TenantId | Required |
| UserId | Required |
| DeviceId | Nullable |
| FamilyId | Rotation family |
| TokenHash | Unique; raw token never stored |
| CreatedAtUtc | |
| ExpiresAtUtc | |
| RevokedAtUtc | Nullable |
| ReplacedByTokenId | Nullable |

Indexes: unique on `TokenHash`; `(TenantId, UserId)`; `(FamilyId)`.

### Refresh rotation

1. Matching active hash → issue new access + refresh; store new hash; link replaced token.
2. Reuse of an already-rotated token → revoke the family.
3. Logout / password change → revoke the family.

JWT claims include `sub` (user id), `tenant_id`, `email`, optional `device_id`, and repeated `permission` / `role` claims. `MapInboundClaims` is disabled so claim types stay stable.

## Devices and sessions

Devices register a stable DeviceKey (login may register, or `POST /api/v1/devices/register`). Revoked devices fail auth and sync. See [offline-first.md](offline-first.md).

## Offline

Default **MaxOfflineDays = 7** (tenant-configurable via `OfflineSessionPolicy.DefaultMaxOfflineDays`). After expiry the device must reconnect and revalidate before further offline business operations. Security flags apply on next connect.

## Tenant bootstrap

`ITenantProvisioningService` creates a tenant, provisions system roles (`IsSystem = true`), and creates the Owner user. Public self-signup is not exposed.

Platform onboarding is available under `/api/v1/platform/...` (see [saas-platform.md](saas-platform.md)): operators provision tenants (with trial subscription), suspend/resume, and manage settings.

## Creating your first admin (local)

1. Start the API (Development bootstraps `platform@warehouse.local` / `Platform-Pass-123!`).
2. `POST /api/v1/platform/auth/login` with that email/password.
3. `POST /api/v1/platform/tenants` with owner credentials (Bearer token from step 2).
4. Log into the web app with the tenant code + owner email/password.

## Web cookie note

Refresh cookie + CSRF protection for browser clients remains a follow-up. Access token remains Bearer header. Current refresh endpoint accepts a JSON body refresh token (mobile-friendly).
