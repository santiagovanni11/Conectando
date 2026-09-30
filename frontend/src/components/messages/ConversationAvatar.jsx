import Avatar from '../ui/Avatar'

/**
 * Cómo se ve una conversación: la foto de quien es, o las de los que son.
 *
 * Vive como componente y no en línea dentro de la fila por dos razones. La
 * primera, que la misma decisión aparece en la lista y en el encabezado del
 * chat, y no puede quedar escrita dos veces. La segunda, y la que importa
 * para lo que viene: el modelo ya habla en plural —`peers` es una lista—,
 * así que cuando existan chats de más de dos personas, el recorte a dos
 * avatares superpuestos es lo único que hay que cambiar acá.
 *
 * Sin foto cae a las iniciales, que es lo que hace `Avatar` en el resto de
 * la app. No se inventa un ícono genérico: la inicial dice al menos de
 * quién es el chat.
 *
 * El punto de "en línea" no se dibuja a propósito. El backend tiene el
 * campo pero no lo calcula, así que siempre valdría false y quedaría un
 * círculo apagado para siempre, que es peor que no tener nada. Cuando haya
 * presencia real, se suma acá y no hay que tocar ni la fila ni el
 * encabezado.
 */
export default function ConversationAvatar({ conversation, size = 'md' }) {
  const peers = conversation?.peers ?? []

  if (peers.length === 0) {
    return <Avatar name="" src={null} size={size} />
  }

  // Caso directo, que es el único que hay hoy.
  if (peers.length === 1) {
    return (
      <Avatar
        name={peers[0].displayName}
        src={peers[0].profileImageUrl}
        size={size}
      />
    )
  }

  return (
    <span className={`conversation-avatar conversation-avatar--${size}`}>
      {peers.slice(0, 2).map((peer, index) => (
        <Avatar
          key={peer.id ?? index}
          name={peer.displayName}
          src={peer.profileImageUrl}
          size={size}
          className={`conversation-avatar__face is-${index === 0 ? 'left' : 'right'}`}
        />
      ))}
    </span>
  )
}
