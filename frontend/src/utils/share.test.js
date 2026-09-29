import { describe, it, expect } from 'vitest'
import { shareUrl, absoluteUrl } from '../utils/share'

describe('absoluteUrl', () => {
  it('convierte una ruta relativa en absoluta', () => {
    expect(absoluteUrl('/posts/abc')).toBe(`${window.location.origin}/posts/abc`)
  })
})

describe('shareUrl', () => {
  it('usa el diálogo nativo cuando existe', async () => {
    const share = vi.fn().mockResolvedValue(undefined)
    vi.stubGlobal('navigator', { share, clipboard: { writeText: vi.fn() } })

    const result = await shareUrl({ url: 'https://x.test/p/1', title: 't' })

    expect(result).toBe('shared')
    expect(share).toHaveBeenCalledWith({ url: 'https://x.test/p/1', title: 't', text: undefined })
  })

  it('no reporta error si el usuario cancela el diálogo nativo', async () => {
    const abort = Object.assign(new Error('cancelado'), { name: 'AbortError' })
    vi.stubGlobal('navigator', {
      share: vi.fn().mockRejectedValue(abort),
      clipboard: { writeText: vi.fn() },
    })

    await expect(shareUrl({ url: 'https://x.test/p/1' })).resolves.toBe('shared')
  })

  it('copia al portapapeles cuando no hay diálogo nativo', async () => {
    const writeText = vi.fn().mockResolvedValue(undefined)
    vi.stubGlobal('navigator', { share: undefined, clipboard: { writeText } })

    const result = await shareUrl({ url: 'https://x.test/p/1' })

    expect(result).toBe('copied')
    expect(writeText).toHaveBeenCalledWith('https://x.test/p/1')
  })

  it('informa fallo si tampoco se puede copiar', async () => {
    vi.stubGlobal('navigator', {
      share: undefined,
      clipboard: { writeText: vi.fn().mockRejectedValue(new Error('sin permiso')) },
    })

    await expect(shareUrl({ url: 'https://x.test/p/1' })).resolves.toBe('failed')
  })
})