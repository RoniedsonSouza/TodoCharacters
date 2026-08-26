export type ViewMode = 'grade' | 'lista';

type ViewModeToggleProps = {
  value: ViewMode;
  onChange: (mode: ViewMode) => void;
  'aria-label'?: string;
};

function GridIcon() {
  return (
    <svg
      width="18"
      height="18"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <rect x="3" y="3" width="7" height="7" rx="1" />
      <rect x="14" y="3" width="7" height="7" rx="1" />
      <rect x="3" y="14" width="7" height="7" rx="1" />
      <rect x="14" y="14" width="7" height="7" rx="1" />
    </svg>
  );
}

function ListIcon() {
  return (
    <svg
      width="18"
      height="18"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <line x1="8" y1="6" x2="21" y2="6" />
      <line x1="8" y1="12" x2="21" y2="12" />
      <line x1="8" y1="18" x2="21" y2="18" />
      <line x1="3" y1="6" x2="3.01" y2="6" />
      <line x1="3" y1="12" x2="3.01" y2="12" />
      <line x1="3" y1="18" x2="3.01" y2="18" />
    </svg>
  );
}

export function ViewModeToggle({
  value,
  onChange,
  'aria-label': ariaLabel = 'Modo de exibição',
}: Readonly<ViewModeToggleProps>) {
  return (
    <div className="view-toggle" role="group" aria-label={ariaLabel}>
      <button
        type="button"
        aria-label="Grade"
        aria-pressed={value === 'grade'}
        className={value === 'grade' ? 'filter-active' : undefined}
        onClick={() => onChange('grade')}
      >
        <GridIcon />
      </button>
      <button
        type="button"
        aria-label="Lista detalhada"
        aria-pressed={value === 'lista'}
        className={value === 'lista' ? 'filter-active' : undefined}
        onClick={() => onChange('lista')}
      >
        <ListIcon />
      </button>
    </div>
  );
}
