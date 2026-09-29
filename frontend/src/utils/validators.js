const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
const USERNAME_REGEX = /^[A-Za-z0-9_]{3,30}$/

export function isEmailValid(email) {
  return EMAIL_REGEX.test(email)
}

export function isValidUserName(userName) {
  return USERNAME_REGEX.test(userName)
}

export function isValidPassword(password) {
  return password.length >= 8 && /[A-Za-z]/.test(password) && /\d/.test(password)
}

export function isDisplayNameValid(displayName) {
  const trimmed = displayName.trim()
  return trimmed.length >= 2 && trimmed.length <= 50
}

export function isBioValid(bio) {
  return bio.trim().length <= 160
}

export function isImageUrlValid(url) {
  if (!url || !url.trim()) return true

  try {
    const parsed = new URL(url)
    return parsed.protocol === 'http:' || parsed.protocol === 'https:'
  } catch {
    return false
  }
}