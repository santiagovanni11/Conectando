const LONG_DATE_OPTIONS = { day: 'numeric', month: 'long', year: 'numeric' }

const JOINED_LABELS = {
  es: (date) => `Se unió el ${date}`,
  en: (date) => `Joined on ${date}`,
  pt: (date) => `Entrou em ${date}`,
  fr: (date) => `A rejoint le ${date}`,
  it: (date) => `Si è unito il ${date}`,
  de: (date) => `Am ${date} beigetreten`,
}

const DEFAULT_JOINED_LABEL = JOINED_LABELS.es

function resolveDate(iso) {
  if (iso == null || iso === '') return null
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return null
  if (date.getTime() < 0) return null
  return date
}

function resolveLocale(locale) {
  if (locale) return locale
  if (typeof navigator !== 'undefined' && navigator.language) return navigator.language
  return undefined
}

export function isValidDate(iso) {
  return resolveDate(iso) !== null
}

export function formatLongDate(iso, locale) {
  const date = resolveDate(iso)
  if (!date) return null

  try {
    return new Intl.DateTimeFormat(resolveLocale(locale), LONG_DATE_OPTIONS).format(date)
  } catch {
    return null
  }
}

export function formatJoinedDate(iso, locale) {
  const date = formatLongDate(iso, locale)
  if (!date) return null

  const resolved = resolveLocale(locale)
  const language = resolved?.split('-')[0]?.toLowerCase() ?? 'es'
  const label = JOINED_LABELS[language] ?? DEFAULT_JOINED_LABEL
  return label(date)
}

export function formatRelativeTime(iso) {
  const date = resolveDate(iso)
  if (!date) return ''

  const diffMs = Date.now() - date.getTime()
  if (diffMs < 60_000) return 'ahora'

  const minutes = Math.floor(diffMs / 60_000)
  if (minutes < 60) return `${minutes} min`

  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours} h`

  const days = Math.floor(hours / 24)
  if (days < 30) return `${days} d`

  return new Intl.DateTimeFormat(resolveLocale(), { day: 'numeric', month: 'short', year: 'numeric' }).format(date)
}