import { useEffect, useRef } from 'react'

/**
 * Pestañas del perfil propio: publicaciones, guardadas, bloqueados y cuenta.
 *
 * Solo se muestran en el perfil propio. Las tres primeras son privadas: en
 * el perfil de otro no tendrían sentido. "Cuenta" es donde viven el cambio
 * de contraseña y la baja de la cuenta.
 *
 * Las pestañas se describen acá y no en la página, para que agregar una
 * sea agregar una línea y no copiar otro botón.
 */
const TABS = [
  { id: 'posts', label: 'Publicaciones' },
  { id: 'saved', label: 'Publicaciones guardadas' },
  { id: 'blocked', label: 'Perfiles bloqueados' },
  { id: 'account', label: 'Cuenta' },
]

export default function ProfileTabs({ active, onChange }) {
  const activeRef = useRef(null)

  /**
   * Trae la pestaña activa a la vista cuando cambia.
   *
   * Las cuatro pestañas no entran en un teléfono —juntas rondan los 600 px
   * contra los ~340 útiles de un iPhone—, así que la fila scrollea en
   * horizontal. Sin esto, llegar a "Cuenta", que es la última y la que más
   * lejos cae, deja la pantalla mostrando pestañas que no son la activa y
   * el usuario no tiene forma de saber dónde está.
   *
   * `block: 'nearest'` evita que la página salte también en vertical: acá
   * solo interesa el eje horizontal.
   *
   * El `?.` del método va aparte del de la referencia porque son dos
   * riesgos distintos: la ref puede no estar, y el método puede no existir,
   * como pasa en jsdom y en navegadores viejos. Sin el segundo, un efecto
   * que solo debería mover la vista termina tirando abajo la pantalla.
   */
  useEffect(() => {
    activeRef.current?.scrollIntoView?.({ block: 'nearest', inline: 'center' })
  }, [active])

  return (
    <div className="profile-tabs" role="tablist" aria-label="Perfil">
      {TABS.map((tab) => (
        <button
          key={tab.id}
          // La referencia va solo en la activa: es la única que hay que
          // traer a la vista cuando la pestaña cambia.
          ref={tab.id === active ? activeRef : null}
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
