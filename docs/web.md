# Web client

Status: **implemented**. Location: `clients/web` (tenant warehouse UI). Platform admin is a **separate** app at `clients/platform` (port 5174).

## Languages

| Language | Direction | Default |
| --- | --- | --- |
| Arabic | RTL | **Yes** |
| English | LTR | Switchable |

Resources: `clients/web/src/i18n/locales/ar.json`, `en.json`. Preference key: `warehouse.lang`.

## Run

1. API: `dotnet run --project src/Warehouse.Api --launch-profile http`
2. Tenant web: `cd clients/web && npm install && npm run dev` → http://localhost:5173
3. Platform admin (optional): `cd clients/platform && npm run dev` → http://localhost:5174

Vite proxies `/api` to port 5011. Development CORS allows both Vite origins.

## Covered screens

Authentication, dashboard, products (create), inventory (warehouses + opening balances), purchasing (suppliers + PO confirm/receive), sales (customers + SO confirm/ship), transfers (dispatch/receive), sync pull status, administration (users create/enable-disable, roles, permissions).
