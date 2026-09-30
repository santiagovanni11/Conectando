import { Link } from 'react-router-dom'
import { ROUTES } from '../../constants/routes'

/**
 * Cómo se muestra el nombre de una cuenta.
 *
 * Existe porque el mismo dato se pinta en muchos lugares —el encabezado de
 * una publicación, un comentario, la lista de amigos, un like— y la decisión
 * de qué poner cuando la cuenta ya no existe tiene que ser la misma en todos.
 * Repartida por los componentes, el día que cambie uno queda mostrando el
 * identificador interno y nadie se da cuenta hasta que se ve en pantalla.
 *
 * Una cuenta dada de baja llega con `isDeleted` y sin nombre de usuario: el
 * servidor no manda el identificador que queda guardado. Acá se decide el
 * texto y si se puede entrar a ese perfil, que ya no existe.
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

export default function UserHandle({ user, className }) {
  if (!user) return null

  const nombre = user.userName ?? ''
  const eliminado = Boolean(user.isDeleted) || nombre === '' || nombre.startsWith(MARCA_DE_BAJA)

  if (eliminado) {
    return <span className={className}>Cuenta eliminada</span>
  }

  return (
    <Link className={className} to={ROUTES.user(user.id)}>
      @{nombre}
    </Link>
  )
}