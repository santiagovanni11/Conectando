import HelpCard from '../components/help/HelpCard'

/** Apartado de ayuda: /help */
export default function HelpPage() {
  return (
    <div className="help-page">
      <header className="help-page__header">
        <h1 className="help-page__title">Ayuda</h1>
        <p className="help-page__subtitle">
          Si algo no funciona como debería, contanos y lo revisamos.
        </p>
      </header>

      <HelpCard />
    </div>
  )
}