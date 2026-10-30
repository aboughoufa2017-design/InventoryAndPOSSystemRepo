# Platform admin client

Dedicated SaaS Platform Admin app (separate from the tenant warehouse web at `clients/web`). React + TypeScript + Vite.

## Localization

- **Arabic is the default** (`dir=rtl`, `lang=ar`).
- English is first-class LTR.
- Strings live in `src/i18n/locales/ar.json` and `en.json` (i18next).
- Language preference is stored in `localStorage` (`warehouse.lang`).

## Run

```text
cd clients/platform
npm install
npm run dev
```

Dev server: **http://localhost:5174**

Vite proxies `/api` and `/health` to `http://localhost:5011` (API http profile).

```text
dotnet run --project src/Warehouse.Api --launch-profile http
```

Optional: set `VITE_API_BASE_URL` if not using the proxy.

## Credentials (Development)

Bootstrap platform operator (from API `Platform:BootstrapEmail` / `BootstrapPassword`):

- Email: `platform@warehouse.local`
- Password: `Platform-Pass-123!`

Sign in at http://localhost:5174/login

## Screens

Operator login, tenant list, provision tenant, tenant detail (suspend/resume, usage, readiness, subscription).
