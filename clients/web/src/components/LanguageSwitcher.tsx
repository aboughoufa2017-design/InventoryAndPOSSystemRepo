import { useTranslation } from "react-i18next";
import { setAppLanguage, type AppLanguage } from "../i18n";

export function LanguageSwitcher() {
  const { i18n, t } = useTranslation();
  const current = (i18n.language?.startsWith("en") ? "en" : "ar") as AppLanguage;

  return (
    <div className="lang-switch" role="group" aria-label={t("lang.switch")}>
      <button
        type="button"
        className={current === "ar" ? "active" : undefined}
        onClick={() => void setAppLanguage("ar")}
      >
        {t("lang.ar")}
      </button>
      <button
        type="button"
        className={current === "en" ? "active" : undefined}
        onClick={() => void setAppLanguage("en")}
      >
        {t("lang.en")}
      </button>
    </div>
  );
}
