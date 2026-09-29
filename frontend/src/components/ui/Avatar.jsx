function getInitials(name) {
  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((word) => word[0].toUpperCase())
    .join('')
}

export default function Avatar({ name, src, alt, size = 'md', ring = false, className = '' }) {
  const initials = name ? getInitials(name) : ''

  if (src) {
    return (
      <span className={`avatar avatar--${size} ${ring ? 'avatar--ring' : ''} ${className}`}>
        <img className="avatar__img" src={src} alt={alt ?? ''} />
      </span>
    )
  }

  return (
    <span
      className={`avatar avatar--${size} ${ring ? 'avatar--ring' : ''} ${className}`}
      aria-hidden={alt ? undefined : true}
    >
      <span className="avatar__initials">{initials}</span>
    </span>
  )
}