import { Component } from 'react'
import ErrorState from './ui/ErrorState'

/**
 * Evita que un error en cualquier componente deje la app en blanco.
 * Sin esto, React desmonta todo el árbol y el usuario solo ve una pantalla vacía.
 */
export default class AppErrorBoundary extends Component {
  state = { error: null }

  static getDerivedStateFromError(error) {
    return { error }
  }

  componentDidCatch(error, info) {
    console.error('Error capturado por AppErrorBoundary:', error, info)
  }

  render() {
    if (this.state.error) {
      return (
        <ErrorState
          title="Algo salió mal"
          description="Ocurrió un error inesperado. Recargá la página para continuar."
          onRetry={() => window.location.reload()}
        />
      )
    }

    return this.props.children
  }
}