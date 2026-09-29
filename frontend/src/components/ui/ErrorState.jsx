import Button from './Button'

export default function ErrorState({ title = 'Ocurrió un error', description, onRetry, className = '' }) {
  return (
    <div className={`error-state ${className}`} role="alert">
      <h3 className="error-state__title">{title}</h3>
      {description && <p className="error-state__description">{description}</p>}
      {onRetry && (
        <Button variant="secondary" size="sm" onClick={onRetry}>
          Intentar de nuevo
        </Button>
      )}
    </div>
  )
}