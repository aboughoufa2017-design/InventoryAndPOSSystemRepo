import { useTranslation } from "react-i18next";

type PaginationProps = {
  page: number;
  pageSize: number;
  total: number;
  onChange: (page: number) => void;
};

export function Pagination({ page, pageSize, total, onChange }: PaginationProps) {
  const { t } = useTranslation();
  const pageCount = Math.max(1, Math.ceil(total / pageSize));
  if (total <= pageSize) {
    return total === 0 ? null : (
      <p className="pagination muted">
        {t("common.showing", { from: total === 0 ? 0 : 1, to: total, total })}
      </p>
    );
  }

  const from = (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, total);

  return (
    <div className="pagination">
      <span className="muted">{t("common.showing", { from, to, total })}</span>
      <div className="pagination-actions">
        <button type="button" className="btn secondary" disabled={page <= 1} onClick={() => onChange(page - 1)}>
          {t("common.prev")}
        </button>
        <span>
          {t("common.pageOf", { page, pages: pageCount })}
        </span>
        <button
          type="button"
          className="btn secondary"
          disabled={page >= pageCount}
          onClick={() => onChange(page + 1)}
        >
          {t("common.next")}
        </button>
      </div>
    </div>
  );
}

export function paginate<T>(items: T[], page: number, pageSize: number): T[] {
  const start = (page - 1) * pageSize;
  return items.slice(start, start + pageSize);
}

export const DEFAULT_PAGE_SIZE = 10;
