import { getIconPaths } from './icons'

const SIZE_CLASSES = {
  sm: 'icon--sm',
  md: 'icon--md',
  lg: 'icon--lg',
  xl: 'icon--xl',
}

export default function Icon({ name, size = 'md', className = '', title }) {
  const paths = getIconPaths(name)

  return (
    <svg
      className={`icon ${SIZE_CLASSES[size] ?? ''} ${className}`}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden={title ? undefined : true}
      role={title ? 'img' : undefined}
    >
      {title && <title>{title}</title>}
      {paths.map((d) => (
        <path key={d} d={d} />
      ))}
    </svg>
  )
}