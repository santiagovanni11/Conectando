import { Link } from 'react-router-dom'
import { ROUTES } from '../../constants/routes'

/**
 * Enlaza un nombre de usuario con su perfil.
 *
 * <para>
 * Es la pieza que hace que tocar un nombre lleve al perfil, como en cualquier
 * red social. Existe una sola y en un solo lugar porque el nombre de alguien
 * aparece en muchos lugares —el encabezado de una publicación, un comentario,
 * la lista de amigos, un me gusta— y decidir en cada componente si va con
 * enlace o no lleva a que un día uno de ellos se quede sin él y nadie lo note.
 *
 * <para>
 * Sin hijos muestra el nombre de usuario. Con hijos muestra lo que se le pase:
 * los comentarios y los chats muestran el nombre de pila, no el arroba.
 *
 * <para>
 * `linked={false}` deja el texto sin enlace. Existe para cuando el contenedor ya
 * es un enlace al perfil —una fila de la lista de amigos—: dos enlaces anidados
 * son HTML inválido, y el de adentro además compite con el de afuera por el
 * mismo destino.
 *
 * <para>
 * Una cuenta dada de baja llega con `isDeleted` y sin nombre de usuario: el
 * servidor no manda el identificador que queda guardado. Ahí no hay perfil al
 * que ir, así que se muestra una etiqueta y no un enlace roto.
 * </para>
 */

/**
 * Marca con la que la base nombra a las cuentas dadas de baja.
 *
 * <para>
 * No debería llegar nunca: <c>UserPresentation</c> lo reemplaza antes de que
 * la respuesta salga. Se mira igual porque hay una ventana en la que puede
 * pasar —el navegador con el JavaScript de esta versión contra un servidor
 * que todavía manda el dato crudo— y en ese caso lo que se ve es un
 * identificador de treinta y dos caracteres que parece un error. Preferible
 * mostrar "cuenta eliminada" a mostrar eso.
 * </para>
 */
const MARCA_DE_BAJA = 'eliminado-'

export default function UserHandle({ user, className, children, linked = true }) {
  if (!user) return null

  const nombre = user.userName ?? ''

  // La señal de que la cuenta ya no existe es `isDeleted`, que manda el
  // servidor. La marca del placeholder es la red por si llegara un backend
  // viejo sin el campo: un nombre que empieza así nunca fue elegido por una
  // persona, así que no puede mostrarse.
  const eliminado = Boolean(user.isDeleted) || nombre.startsWith(MARCA_DE_BAJA)

  const texto = children ?? (nombre ? `@${nombre}` : user.displayName)

  if (eliminado) {
    return <span className={className}>Cuenta eliminada</span>
  }

  if (!linked) {
    return <span className={className}>{texto}</span>
  }

  return (
    <Link className={className} to={ROUTES.user(user.id)}>
      {texto}
    </Link>
  )
}