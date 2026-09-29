const SITE_NAME = 'Conectando'
const DEFAULT_DESCRIPTION = 'La red social donde las personas se encuentran.'

function upsertMeta(attribute, key, content) {
  if (!content) return
  let tag = document.head.querySelector(`meta[${attribute}="${key}"]`)
  if (!tag) {
    tag = document.createElement('meta')
    tag.setAttribute(attribute, key)
    document.head.appendChild(tag)
  }
  tag.setAttribute('content', content)
}

function upsertProperty(property, content) {
  if (!content) return
  let tag = document.head.querySelector(`meta[property="${property}"]`)
  if (!tag) {
    tag = document.createElement('meta')
    tag.setAttribute('property', property)
    document.head.appendChild(tag)
  }
  tag.setAttribute('content', content)
}

function setTitle(title) {
  document.title = title
}

export function setDefaultMeta() {
  setTitle(SITE_NAME)
  upsertMeta('name', 'description', DEFAULT_DESCRIPTION)
  upsertProperty('og:title', SITE_NAME)
  upsertProperty('og:description', DEFAULT_DESCRIPTION)
  upsertProperty('og:site_name', SITE_NAME)
}

export function setPostMeta({ post, url }) {
  const author = post.author?.displayName ?? post.author?.userName ?? ''
  const excerpt = (post.content?.trim() ?? '').slice(0, 160)
  const cover = post.media?.[0]?.url
  const title = author ? `${SITE_NAME} · ${author}` : SITE_NAME

  setTitle(title)
  upsertMeta('name', 'description', excerpt || DEFAULT_DESCRIPTION)
  upsertProperty('og:title', title)
  upsertProperty('og:description', excerpt || DEFAULT_DESCRIPTION)
  upsertProperty('og:url', url)
  upsertProperty('og:type', 'article')
  if (cover) upsertProperty('og:image', new URL(cover, window.location.origin).toString())
}