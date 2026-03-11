import type {
  InputHTMLAttributes,
  ReactNode,
  SelectHTMLAttributes,
  TextareaHTMLAttributes,
} from 'react';

interface BaseFieldProps {
  label: string;
  error?: string;
  helperText?: string;
}

type InputFieldProps = BaseFieldProps & InputHTMLAttributes<HTMLInputElement>;
type SelectFieldProps = BaseFieldProps &
  SelectHTMLAttributes<HTMLSelectElement> & {
    children?: ReactNode;
  };
type TextAreaFieldProps = BaseFieldProps & TextareaHTMLAttributes<HTMLTextAreaElement>;

export function InputField({ label, error, helperText, ...props }: InputFieldProps) {
  return (
    <label className="field">
      <span className="field-label">{label}</span>
      <input className="field-input" {...props} />
      {error ? <span className="field-error">{error}</span> : null}
      {!error && helperText ? <span className="field-helper">{helperText}</span> : null}
    </label>
  );
}

export function SelectField({ label, error, children, ...props }: SelectFieldProps) {
  return (
    <label className="field">
      <span className="field-label">{label}</span>
      <select className="field-input" {...props}>
        {children}
      </select>
      {error ? <span className="field-error">{error}</span> : null}
    </label>
  );
}

export function TextAreaField({ label, error, helperText, ...props }: TextAreaFieldProps) {
  return (
    <label className="field">
      <span className="field-label">{label}</span>
      <textarea className="field-input field-textarea" {...props} />
      {error ? <span className="field-error">{error}</span> : null}
      {!error && helperText ? <span className="field-helper">{helperText}</span> : null}
    </label>
  );
}
