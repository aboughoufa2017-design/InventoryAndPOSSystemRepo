import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import {
  createUser,
  listPermissions,
  listRoles,
  listUsers,
  setUserActive,
  type Permission,
  type Role,
  type TenantUser,
} from "../api/resources";
import { DataTable, DEFAULT_PAGE_SIZE, paginate } from "../components/DataTable";
import { Modal } from "../components/Modal";

export function AdminPage() {
  const { t } = useTranslation();
  const [roles, setRoles] = useState<Role[]>([]);
  const [permissions, setPermissions] = useState<Permission[]>([]);
  const [users, setUsers] = useState<TenantUser[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [page, setPage] = useState(1);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [form, setForm] = useState({
    email: "",
    password: "",
    displayName: "",
    roleId: "",
  });

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    const [r, p, u] = await Promise.all([listRoles(), listPermissions(), listUsers()]);
    if (r.error || p.error || u.error) {
      setError(t("admin.loadError"));
    } else {
      const roleList = r.data ?? [];
      setRoles(roleList);
      setPermissions(p.data ?? []);
      setUsers(u.data ?? []);
      setForm((prev) => ({
        ...prev,
        roleId: prev.roleId || roleList.find((x) => x.code === "Administrator")?.id || roleList[0]?.id || "",
      }));
    }
    setLoading(false);
  }, [t]);

  useEffect(() => {
    void load();
  }, [load]);

  function openCreate() {
    setForm((prev) => ({
      ...prev,
      email: "",
      password: "",
      displayName: "",
      roleId: prev.roleId || roles.find((x) => x.code === "Administrator")?.id || roles[0]?.id || "",
    }));
    setDialogOpen(true);
  }

  async function onCreate(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError(null);
    setOk(null);
    const result = await createUser({
      email: form.email.trim(),
      password: form.password,
      displayName: form.displayName.trim(),
      roleIds: [form.roleId],
    });
    setBusy(false);
    if (result.error || !result.data) {
      setError(result.error?.message ?? t("admin.createError"));
      return;
    }
    setOk(t("admin.created"));
    setDialogOpen(false);
    setForm((prev) => ({ ...prev, email: "", password: "", displayName: "" }));
    void load();
  }

  async function onToggleActive(user: TenantUser) {
    setBusy(true);
    setError(null);
    setOk(null);
    const result = await setUserActive(user.id, !user.isActive || user.isDisabled, user.rowVersion);
    setBusy(false);
    if (result.error || !result.data) {
      setError(result.error?.message ?? t("admin.actionError"));
      return;
    }
    setOk(user.isActive && !user.isDisabled ? t("admin.disabled") : t("admin.enabled"));
    void load();
  }

  const rows = paginate(users, page, DEFAULT_PAGE_SIZE);

  return (
    <div className="stack">
      <div className="page-header">
        <div>
          <h1>{t("admin.title")}</h1>
          <p>{t("admin.hint")}</p>
        </div>
        <div className="page-actions">
          <button type="button" className="btn secondary" disabled={loading || busy} onClick={() => void load()}>
            {t("products.refresh")}
          </button>
          <button type="button" className="btn" onClick={openCreate}>
            {t("admin.addUser")}
          </button>
        </div>
      </div>
      {loading ? <p className="muted">{t("app.loading")}</p> : null}
      {error ? <p className="error-text">{error}</p> : null}
      {ok ? <p className="ok-text">{ok}</p> : null}

      <div className="panel">
        <h2 style={{ marginTop: 0, fontSize: "1.1rem" }}>{t("admin.users")}</h2>
        <DataTable
          page={page}
          total={users.length}
          onPageChange={setPage}
          empty={<p className="muted">{t("admin.emptyUsers")}</p>}
        >
          <table className="data-table">
            <thead>
              <tr>
                <th>{t("admin.displayName")}</th>
                <th>{t("admin.email")}</th>
                <th>{t("admin.roles")}</th>
                <th>{t("admin.status")}</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {rows.map((user) => (
                <tr key={user.id}>
                  <td>{user.displayName}</td>
                  <td>{user.email}</td>
                  <td>{user.roles.map((r) => r.roleName).join(", ")}</td>
                  <td>{user.isActive && !user.isDisabled ? t("admin.active") : t("admin.inactive")}</td>
                  <td>
                    <button
                      type="button"
                      className="btn secondary"
                      disabled={busy}
                      onClick={() => void onToggleActive(user)}
                    >
                      {user.isActive && !user.isDisabled ? t("admin.disable") : t("admin.enable")}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </DataTable>
      </div>

      <div className="panel">
        <h2 style={{ marginTop: 0, fontSize: "1.1rem" }}>{t("admin.roles")}</h2>
        <ul style={{ margin: 0, paddingInlineStart: "1.2rem" }}>
          {roles.map((role) => (
            <li key={role.id}>
              {role.name} <span className="muted">({role.code})</span>
            </li>
          ))}
        </ul>
      </div>
      <div className="panel">
        <h2 style={{ marginTop: 0, fontSize: "1.1rem" }}>{t("admin.permissions")}</h2>
        <ul style={{ margin: 0, paddingInlineStart: "1.2rem", columns: 2 }}>
          {permissions.map((perm) => (
            <li key={perm.id}>{perm.code}</li>
          ))}
        </ul>
      </div>

      <Modal
        open={dialogOpen}
        title={t("admin.addUser")}
        onClose={() => setDialogOpen(false)}
        footer={
          <>
            <button type="button" className="btn secondary" onClick={() => setDialogOpen(false)}>
              {t("common.cancel")}
            </button>
            <button type="submit" form="admin-user-form" className="btn" disabled={busy || !form.roleId}>
              {busy ? t("common.saving") : t("admin.saveUser")}
            </button>
          </>
        }
      >
        <form id="admin-user-form" className="stack" onSubmit={(e) => void onCreate(e)}>
          <div className="form-grid">
            <div className="field">
              <label htmlFor="u-name">{t("admin.displayName")}</label>
              <input
                id="u-name"
                value={form.displayName}
                onChange={(e) => setForm((f) => ({ ...f, displayName: e.target.value }))}
                required
              />
            </div>
            <div className="field">
              <label htmlFor="u-email">{t("admin.email")}</label>
              <input
                id="u-email"
                type="email"
                value={form.email}
                onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
                required
              />
            </div>
            <div className="field">
              <label htmlFor="u-pass">{t("admin.password")}</label>
              <input
                id="u-pass"
                type="password"
                minLength={8}
                value={form.password}
                onChange={(e) => setForm((f) => ({ ...f, password: e.target.value }))}
                required
              />
            </div>
            <div className="field">
              <label htmlFor="u-role">{t("admin.role")}</label>
              <select
                id="u-role"
                value={form.roleId}
                onChange={(e) => setForm((f) => ({ ...f, roleId: e.target.value }))}
                required
              >
                {roles.map((role) => (
                  <option key={role.id} value={role.id}>
                    {role.name}
                  </option>
                ))}
              </select>
            </div>
          </div>
        </form>
      </Modal>
    </div>
  );
}
