import { useTranslation } from "react-i18next";
import { useTheme } from "../theme/ThemeContext";

export function ThemeSwitcher() {
  const { t } = useTranslation();
  const { theme, setTheme } = useTheme();

  return (
    <div className="theme-switch" role="group" aria-label={t("common.theme")}>
      <button
        type="button"
        className={theme === "light" ? "active" : undefined}
        onClick={() => setTheme("light")}
      >
        {t("common.light")}
      </button>
      <button
        type="button"
        className={theme === "dark" ? "active" : undefined}
        onClick={() => setTheme("dark")}
      >
        {t("common.dark")}
      </button>
    </div>
  );
}
