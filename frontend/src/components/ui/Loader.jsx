import Icon from './Icon/Icon'

const SIZE_ICON = { sm: 'sm', md: 'md', lg: 'lg' }

export default function Loader({ size = 'md', label, className = '' }) {
  return (
    <span className={`loader loader--${size} ${className}`} role={label ? 'status' : undefined} aria-live="polite">
      <Icon name="spinner" size={SIZE_ICON[size] ?? 'md'} className="icon--spin" />
      {label && <span className="loader__label">{label}</span>}
    </span>
  )
}