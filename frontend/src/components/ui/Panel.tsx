import type { PropsWithChildren, ReactNode } from 'react';

interface PanelProps extends PropsWithChildren {
  title?: ReactNode;
  subtitle?: ReactNode;
  actions?: ReactNode;
  className?: string;
}

export function Panel({ title, subtitle, actions, className, children }: PanelProps) {
  return (
    <section className={`panel ${className ?? ''}`.trim()}>
      {(title || subtitle || actions) && (
        <header className="panel-header">
          <div>
            {title ? <h2 className="panel-title">{title}</h2> : null}
            {subtitle ? <p className="panel-subtitle">{subtitle}</p> : null}
          </div>
          {actions ? <div>{actions}</div> : null}
        </header>
      )}
      {children}
    </section>
  );
}
