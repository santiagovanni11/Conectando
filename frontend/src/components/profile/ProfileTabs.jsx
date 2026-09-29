/**
 * Pestañas del perfil propio: publicaciones, guardadas, bloqueados y cuenta.
 *
 * Solo se muestran en el perfil propio. Las tres primeras son privadas: en
 * el perfil de otro no tendrían sentido. "Cuenta" es donde viven el cambio
 * de contraseña y la baja de la cuenta.
 *
 * Las pestañas se describen aca y no en la pagina, para que agregar una
 * sea agregar una linea y no copiar otro boton.
 */
const TABS = [
  { id: 'posts', label: 'Publicaciones' },
  { id: 'saved', label: 'Publicaciones guardadas' },
  { id: 'blocked', label: 'Perfiles bloqueados' },
  { id: 'account', label: 'Cuenta' },
]

export default function ProfileTabs({ active, onChange }) {
  return (
    <div className="profile-tabs" role="tablist" aria-label="Perfil">
      {TABS.map((tab) => (
        <button
          key={tab.id}
          type="button"
          role="tab"
          aria-selected={active === tab.id}
          className={`profile-tabs__tab${active === tab.id ? ' is-active' : ''}`}
          onClick={() => onChange(tab.id)}
        >
          {tab.label}
        </button>
      ))}
    </div>
  )
}
