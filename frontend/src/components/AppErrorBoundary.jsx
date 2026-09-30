import { Component } from 'react'
import ErrorState from './ui/ErrorState'

/**
 * Evita que un error en cualquier componente deje la app en blanco.
 * Sin esto, React desmonta todo el árbol y el usuario solo ve una pantalla vacía.
 *
 * El mensaje técnico se muestra debajo del aviso. Antes solo quedaba en
 * la consola, y un error de runtime es justo el caso en que hace falta
 * poder leerlo sin herramientas: decir "recargá" cuando recargar no
 * arregla nada es la peor forma de responder. Es una línea de texto, sin
 * datos del usuario, y no tapa el botón de reintentar.
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
    const { error } = this.state

    if (error) {
      return (
        <>
          <ErrorState
            title="Algo salió mal"
            description="Ocurrió un error inesperado. Recargá la página para continuar."
            onRetry={() => window.location.reload()}
          />
          <p className="app-error-boundary__detail">{error.message}</p>
        </>
      )
    }

    return this.props.children
  }
}