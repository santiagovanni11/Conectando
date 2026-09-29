import { useId } from 'react'
import Field from './Field'

export default function Textarea({ label, error, hint, className = '', required, ...props }) {
  const autoId = useId()

  if (!label) {
    return (
      <textarea
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
        <textarea
          id={id}
          className={`input input--textarea ${error ? 'input--error' : ''}`}
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy}
          required={required}
          {...props}
        />
      )}
    </Field>
  )
}