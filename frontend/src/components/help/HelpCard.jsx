import { SUPPORT_EMAIL, SUPPORT_MAILTO } from '../../constants/support'
import Icon from '../ui/Icon/Icon'

/**
 * Tarjeta de ayuda.
 *
 * Va en su propio componente y no dentro de la página porque el bloque
 * puede reutilizarse —por ejemplo, dentro de un modal— sin arrastrar la
 * estructura de la pantalla.
 */
export default function HelpCard() {
  return (
    <section className="help-card" aria-labelledby="help-card-title">
      <span className="help-card__icon" aria-hidden="true">
        <Icon name="help" size="lg" />
      </span>

      <h2 className="help-card__title" id="help-card-title">
        ¿Tuviste algún inconveniente?
      </h2>

      <p className="help-card__text">
        Escribinos a{' '}
        <a className="help-card__email" href={SUPPORT_MAILTO}>
          {SUPPORT_EMAIL}
        </a>{' '}
        y pronto intentaremos resolver tu problema.
      </p>
    </section>
  )
}