import { useId } from 'react'
import Field from './Field'

export default function Input({ label, error, hint, className = '', required, ...props }) {
  const autoId = useId()

  if (!label) {
    return (
      <input
        id={autoId}
        className={`input ${error ? 'input--error' : ''} ${className}`}
        aria-invalid={error ? true : undefined}
        {...props}
      />
    )
  }

  return (
    <Field label={label} error={error} hint={hint} required={required} className={className}>
      {(id, describedBy) => (
        <input
          id={id}
          className={`input ${error ? 'input--error' : ''}`}
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy}
          required={required}
          {...props}
        />
      )}
    </Field>
  )
}