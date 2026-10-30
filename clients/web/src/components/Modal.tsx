import { useEffect, useId, useRef, type ReactNode } from "react";
import { useTranslation } from "react-i18next";

type ModalProps = {
  open: boolean;
  title: string;
  onClose: () => void;
  children: ReactNode;
  footer?: ReactNode;
  wide?: boolean;
};

export function Modal({ open, title, onClose, children, footer, wide }: ModalProps) {
  const { t } = useTranslation();
  const titleId = useId();
  const bodyRef = useRef<HTMLDivElement>(null);
  const onCloseRef = useRef(onClose);
  onCloseRef.current = onClose;

  useEffect(() => {
    if (!open) return;

    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") onCloseRef.current();
    };
    document.addEventListener("keydown", onKey);
    const prev = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    // Focus first field in the body only when the dialog opens — not on every parent re-render.
    const id = window.requestAnimationFrame(() => {
      bodyRef.current
        ?.querySelector<HTMLElement>("input:not([disabled]),select:not([disabled]),textarea:not([disabled])")
        ?.focus();
    });

    return () => {
      window.cancelAnimationFrame(id);
      document.removeEventListener("keydown", onKey);
      document.body.style.overflow = prev;
    };
  }, [open]);

  if (!open) return null;

  return (
    <div className="modal-overlay" role="presentation" onMouseDown={onClose}>
      <div
        className={`modal-panel${wide ? " wide" : ""}`}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        onMouseDown={(e) => e.stopPropagation()}
      >
        <div className="modal-header">
          <h2 id={titleId}>{title}</h2>
          <button type="button" className="btn secondary modal-close" onClick={onClose} aria-label={t("common.cancel")}>
            ×
          </button>
        </div>
        <div className="modal-body" ref={bodyRef}>
          {children}
        </div>
        {footer ? <div className="modal-footer">{footer}</div> : null}
      </div>
    </div>
  );
}
