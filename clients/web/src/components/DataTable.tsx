import type { ReactNode } from "react";
import { Pagination, DEFAULT_PAGE_SIZE, paginate } from "./Pagination";

type DataTableProps = {
  empty?: ReactNode;
  page: number;
  pageSize?: number;
  total: number;
  onPageChange: (page: number) => void;
  children: ReactNode;
};

export function DataTable({
  empty,
  page,
  pageSize = DEFAULT_PAGE_SIZE,
  total,
  onPageChange,
  children,
}: DataTableProps) {
  return (
    <div className="data-table-block">
      {total === 0 ? empty : <div className="table-wrap">{children}</div>}
      <Pagination page={page} pageSize={pageSize} total={total} onChange={onPageChange} />
    </div>
  );
}

export { paginate, DEFAULT_PAGE_SIZE };
