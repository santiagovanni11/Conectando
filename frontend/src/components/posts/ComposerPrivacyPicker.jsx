import { useState, useRef, useEffect } from 'react'
import Icon from '../ui/Icon/Icon'
import { PRIVACY_OPTIONS } from '../../constants/post'

export default function ComposerPrivacyPicker({ value, onChange }) {
  const [isOpen, setIsOpen] = useState(false)
  const wrapperRef = useRef(null)
  const selected = PRIVACY_OPTIONS.find((option) => option.value === value) ?? PRIVACY_OPTIONS[0]

  useEffect(() => {
    if (!isOpen) return undefined

    function handlePointerDown(event) {
      if (!wrapperRef.current?.contains(event.target)) setIsOpen(false)
    }
    function handleKeyDown(event) {
      if (event.key === 'Escape') setIsOpen(false)
    }

    document.addEventListener('pointerdown', handlePointerDown)
    document.addEventListener('keydown', handleKeyDown)
    return () => {
      document.removeEventListener('pointerdown', handlePointerDown)
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [isOpen])

  return (
    <div className="composer-privacy" ref={wrapperRef}>
      <button
        type="button"
        className="composer-privacy__trigger"
        onClick={() => setIsOpen((open) => !open)}
        aria-haspopup="listbox"
        aria-expanded={isOpen}
      >
        <Icon name={selected.icon} size="sm" />
        <span>{selected.label}</span>
      </button>

      {isOpen && (
        <ul className="composer-privacy__list" role="listbox" aria-label="Privacidad de la publicación">
          {PRIVACY_OPTIONS.map((option) => (
            <li key={option.value} role="none">
              <button
                type="button"
                role="option"
                aria-selected={option.value === value}
                className={`composer-privacy__option${option.value === value ? ' is-active' : ''}`}
                onClick={() => {
                  onChange(option.value)
                  setIsOpen(false)
                }}
              >
                <Icon name={option.icon} size="sm" />
                <span>{option.label}</span>
                {option.value === value && <Icon name="check" size="sm" className="composer-privacy__check" />}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}