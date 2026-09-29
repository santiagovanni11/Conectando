import Icon from './Icon/Icon'

export default function IconButton({
  name,
  label,
  variant = 'ghost',
  size = 'md',
  className = '',
  type = 'button',
  ...props
}) {
  return (
    <button
      type={type}
      aria-label={label}
      title={label}
      className={`icon-btn icon-btn--${variant} icon-btn--${size} ${className}`}
      {...props}
    >
      <Icon name={name} size={size === 'sm' ? 'sm' : 'md'} />
    </button>
  )
}