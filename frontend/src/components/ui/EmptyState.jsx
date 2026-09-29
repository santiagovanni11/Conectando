export default function EmptyState({ icon, title, description, children, className = '' }) {
  return (
    <div className={`empty-state ${className}`}>
      {icon && <span className="empty-state__icon" aria-hidden="true">{icon}</span>}
      <h3 className="empty-state__title">{title}</h3>
      {description && <p className="empty-state__description">{description}</p>}
      {children}
    </div>
  )
}