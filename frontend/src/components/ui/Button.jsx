import Spinner from './Loader'

export default function Button({
  variant = 'primary',
  size = 'md',
  loading = false,
  icon,
  children,
  className = '',
  disabled,
  type = 'button',
  // `block` se saca de las props para que no llegue al <button> como
  // atributo desconocido. Antes se filtraba solo y el ancho completo nunca
  // se aplicó.
  block = false,
  ...props
}) {
  return (
    <button
      type={type}
      className={`btn btn--${variant} btn--${size}${block ? ' btn--block' : ''} ${className}`}
      disabled={disabled || loading}
      aria-busy={loading || undefined}
      {...props}
    >
      {loading && <Spinner size="sm" className="btn__spinner" />}
      {!loading && icon}
      <span>{children}</span>
    </button>
  )
}