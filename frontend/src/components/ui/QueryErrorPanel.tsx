import { Panel } from '@/components/ui/Panel';

interface QueryErrorPanelProps {
  title: string;
  subtitle: string;
  message: string;
  onRetry: () => void;
  retryLabel?: string;
  retrying?: boolean;
  embedded?: boolean;
}

export function QueryErrorPanel({
  title,
  subtitle,
  message,
  onRetry,
  retryLabel = 'Try again',
  retrying = false,
  embedded = false,
}: QueryErrorPanelProps) {
  const content = (
    <div className="stack">
      <div className="banner banner-error">{message}</div>
      <div className="button-row">
        <button
          className="button button-secondary"
          disabled={retrying}
          onClick={onRetry}
          type="button"
        >
          {retrying ? 'Retrying...' : retryLabel}
        </button>
      </div>
    </div>
  );

  if (embedded) {
    return content;
  }

  return (
    <Panel title={title} subtitle={subtitle}>
      {content}
    </Panel>
  );
}
