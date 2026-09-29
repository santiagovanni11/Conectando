export default function Divider({ orientation = 'horizontal', label, className = '' }) {
  if (label) {
    return (
      <div className={`divider divider--label ${className}`} role="separator" aria-orientation="horizontal">
        <span className="divider__label">{label}</span>
      </div>
    )
  }

  return (
    <hr
      className={`divider divider--${orientation} ${className}`}
      role="separator"
      aria-orientation={orientation === 'vertical' ? 'vertical' : 'horizontal'}
    />
  )
}