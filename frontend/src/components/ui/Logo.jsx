export default function Logo({ variant = 'full', size = 'md', className = '', to }) {
  const content = (
    <>
      <svg className="logo__mark" viewBox="0 0 48 40" aria-hidden="true">
        <path
          d="M9 10a6 6 0 0 1 6-6h18a6 6 0 0 1 6 6v8a6 6 0 0 1-6 6h-9l-7 7v-7h-2a6 6 0 0 1-6-6Z"
          fill="currentColor"
        />
        <path
          className="logo__mark-outline"
          d="M39 38H22a6 6 0 0 1-6-6V22a6 6 0 0 1 6-6h2l5-5v5h10a6 6 0 0 1 6 6v10a6 6 0 0 1-6 6Z"
          fill="none"
          stroke="currentColor"
          strokeWidth="3.5"
          strokeLinejoin="round"
        />
      </svg>
      {variant === 'full' && <span className="logo__word">Conectando</span>}
    </>
  )

  if (to) {
    return (
      <a href={to} aria-label="Conectando" className={`logo logo--${variant} logo--${size} ${className}`}>
        {content}
      </a>
    )
  }

  return <span className={`logo logo--${variant} logo--${size} ${className}`}>{content}</span>
}