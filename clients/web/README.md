# Web client

React + TypeScript + Vite tenant warehouse UI.

## Localization

- **Arabic is the default** (`dir=rtl`, `lang=ar`).
- English is first-class LTR.
- Strings live in `src/i18n/locales/ar.json` and `en.json` (i18next).
- Language preference is stored in `localStorage` (`warehouse.lang`).

The API does not localize business data; UI language is client-only.

## Run

```text
cd clients/web
npm install
npm run dev
```

→ http://localhost:5173

Vite proxies `/api` and `/health` to `http://localhost:5011` (API http profile).

```text
dotnet run --project src/Warehouse.Api --launch-profile http
```

Optional: set `VITE_API_BASE_URL` if not using the proxy.

## Screens

Login, dashboard, products, inventory, purchasing, sales, transfers, sync pull status, admin (users / roles / permissions).

SaaS platform operators use **`clients/platform`** (http://localhost:5174), not this app.

Offline IndexedDB outbox is deferred; sync screen calls pull directly for now.
