import { useId } from 'react'

export default function Field({ label, error, hint, required, className = '', children }) {
  const id = useId()
  const describedBy = error ? `${id}-error` : hint ? `${id}-hint` : undefined

  return (
    <div className={`field ${className}`}>
      {label && (
        <label className="field__label" htmlFor={id}>
          {label}
          {required && <span className="field__required"> *</span>}
        </label>
      )}
      {children(id, describedBy)}
      {error && (
        <p className="field__msg field__msg--error" id={`${id}-error`}>
          {error}
        </p>
      )}
      {!error && hint && (
        <p className="field__msg" id={`${id}-hint`}>
          {hint}
        </p>
      )}
    </div>
  )
}