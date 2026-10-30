import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../auth/AuthContext";

const cards = [
  { to: "/products", title: "products", desc: "productsDesc" },
  { to: "/inventory", title: "inventory", desc: "inventoryDesc" },
  { to: "/purchasing", title: "purchasing", desc: "purchasingDesc" },
  { to: "/sales", title: "sales", desc: "salesDesc" },
  { to: "/transfers", title: "transfers", desc: "transfersDesc" },
  { to: "/sync", title: "sync", desc: "syncDesc" },
] as const;

export function DashboardPage() {
  const { t } = useTranslation();
  const { user } = useAuth();

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>{t("dashboard.welcome", { name: user?.displayName || user?.email })}</h1>
          <p>{t("dashboard.subtitle")}</p>
        </div>
      </div>
      <div className="grid-cards">
        {cards.map((card) => (
          <Link key={card.to} to={card.to} className="dash-card">
            <h3>{t(`dashboard.${card.title}`)}</h3>
            <p>{t(`dashboard.${card.desc}`)}</p>
          </Link>
        ))}
      </div>
    </div>
  );
}
