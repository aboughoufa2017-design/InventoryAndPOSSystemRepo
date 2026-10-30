import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import ar from "./locales/ar.json";
import en from "./locales/en.json";

export const SUPPORTED_LANGS = ["ar", "en"] as const;
export type AppLanguage = (typeof SUPPORTED_LANGS)[number];

const STORAGE_KEY = "warehouse.lang";

export function getStoredLanguage(): AppLanguage {
  const stored = localStorage.getItem(STORAGE_KEY);
  if (stored === "en" || stored === "ar") {
    return stored;
  }
  return "ar";
}

export function applyDocumentDirection(lang: AppLanguage) {
  const rtl = lang === "ar";
  document.documentElement.lang = lang;
  document.documentElement.dir = rtl ? "rtl" : "ltr";
  document.title = lang === "ar" ? "المخازن | Warehouse" : "Warehouse | المخازن";
}

export async function setAppLanguage(lang: AppLanguage) {
  localStorage.setItem(STORAGE_KEY, lang);
  applyDocumentDirection(lang);
  await i18n.changeLanguage(lang);
}

const initial = getStoredLanguage();
applyDocumentDirection(initial);

void i18n.use(initReactI18next).init({
  resources: {
    ar: { translation: ar },
    en: { translation: en },
  },
  lng: initial,
  fallbackLng: "ar",
  interpolation: { escapeValue: false },
});

export default i18n;
