import { useCallback, useEffect, useRef, useState } from 'react'
import { userService } from '../services/userService'
import { socialService } from '../services/socialService'
import { useDebouncedValue } from './useDebouncedValue'

const MIN_LENGTH = 2

function toList(response) {
  if (Array.isArray(response)) return response
  return response?.items ?? response?.results ?? []
}

export function useSearch() {
  const [results, setResults] = useState([])
  const [error, setError] = useState('')
  const [query, setQuery] = useState('')
  const [isSearchOpen, setIsSearchOpen] = useState(false)
  const debouncedQuery = useDebouncedValue(query, 300)
  const requestRef = useRef(0)
  const term = debouncedQuery.trim()
  const isTooShort = term.length < MIN_LENGTH
  const [pending, setPending] = useState(false)

  // Busca solo mientras se escribe, sin depender de apretar Enter.
  useEffect(() => {
    if (isTooShort) return undefined

    const requestId = requestRef.current + 1
    requestRef.current = requestId

    userService
      .searchUsers(term)
      .then((response) => {
        // Descarta respuestas viejas si el usuario sigue escribiendo.
        if (requestRef.current !== requestId) return
        setResults(toList(response))
        setError('')
      })
      .catch((err) => {
        if (requestRef.current !== requestId) return
        setResults([])
        setError(err.message || 'Error al buscar usuarios')
      })
      .finally(() => {
        if (requestRef.current === requestId) setPending(false)
      })

    setPending(true)
    return undefined
  }, [term, isTooShort])

  // Con menos de 2 letras no hay resultados: se deriva, no se setea en un efecto.
  const visibleResults = isTooShort ? [] : results
  const visibleError = isTooShort ? '' : error
  const visibleLoading = isTooShort ? false : pending

  const search = useCallback((term) => {
    setQuery(term)
  }, [])

  function clear() {
    requestRef.current += 1
    setResults([])
    setError('')
    setQuery('')
    setPending(false)
  }

  // Permite mandar la solicitud sin salir de la búsqueda.
  async function sendRequest(userId) {
    setResults((prev) => prev.map((r) => (r.id === userId ? { ...r, friendship: 'sent' } : r)))
    try {
      await socialService.sendRequest(userId)
    } catch (err) {
      setError(err.message)
      setResults((prev) => prev.map((r) => (r.id === userId ? { ...r, friendship: 'none' } : r)))
    }
  }

  const hasQuery = query.trim().length >= MIN_LENGTH

  return {
    results: visibleResults,
    loading: visibleLoading,
    error: visibleError,
    query,
    setQuery,
    search,
    hasQuery,
    isSearchOpen,
    setIsSearchOpen,
    clear,
    sendRequest,
  }
}