const STATS = [
  { key: 'postsCount', label: 'Publicaciones' },
  { key: 'friendsCount', label: 'Amigos', kind: 'friends' },
  { key: 'followersCount', label: 'Seguidores', kind: 'followers' },
]

/**
 * Muestra publicaciones, amigos y seguidores.
 * Las tres usan el mismo orden: número arriba, etiqueta abajo.
 * Amigos y seguidores abren su lista al hacer click.
 */
export default function ProfileStats({ profile, counts, onOpenConnections }) {
  // Los conteos llegan aparte: dependen de quién mira. Si faltan —por
  // ejemplo en el perfil propio— se cae a los del perfil y luego a cero.
  const source = counts ?? profile

  return (
    <dl className="profile-stats">
      {STATS.map((stat) => {
        const value = source[stat.key] ?? 0

        if (!stat.kind) {
          return (
            <div key={stat.key} className="profile-stats__item">
              <dd className="profile-stats__value">{value}</dd>
              <dt className="profile-stats__label">{stat.label}</dt>
            </div>
          )
        }

        return (
          <button
            key={stat.key}
            type="button"
            className="profile-stats__item profile-stats__item--action"
            onClick={() => onOpenConnections?.(stat.kind)}
            aria-label={`Ver ${stat.label.toLowerCase()} de ${profile.displayName}`}
          >
            <dd className="profile-stats__value">{value}</dd>
            <dt className="profile-stats__label">{stat.label}</dt>
          </button>
        )
      })}
    </dl>
  )
}