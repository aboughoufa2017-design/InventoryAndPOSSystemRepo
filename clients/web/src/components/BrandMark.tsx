import brandMarkUrl from "../assets/brand-mark.svg";

type BrandMarkProps = {
  size?: "sm" | "md" | "lg";
  showWordmark?: boolean;
  tagline?: string;
  title: string;
};

const sizes = {
  sm: 36,
  md: 48,
  lg: 72,
} as const;

export function BrandMark({ size = "md", showWordmark = true, tagline, title }: BrandMarkProps) {
  const px = sizes[size];

  return (
    <div className={`brand-mark brand-mark-${size}`}>
      <img className="brand-mark-icon" src={brandMarkUrl} alt="" width={px} height={px} />
      {showWordmark ? (
        <div className="brand-mark-text">
          <p className="brand-display">{title}</p>
          {tagline ? <p className="brand-tagline">{tagline}</p> : null}
        </div>
      ) : null}
    </div>
  );
}
