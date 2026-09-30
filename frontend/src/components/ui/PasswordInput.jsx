import { useState } from 'react'
import Field from './Field'
import IconButton from './IconButton'

/**
 * Aspecto del botón según el estado del campo.
 *
 * El ícono y el texto accesible salen de acá, de la misma fila, y no
 * sueltos en el JSX. Antes cada uno se escribía por su cuenta y
 * quedaron al revés: se veía "ojo tachado" justo cuando la contraseña
 * estaba a la vista. No se notaba porque el texto decía bien la acción y
 * los tests solo miraban el texto.
 *
 * Responde a dos preguntas distintas, y por eso son dos campos: el ícono
 * describe el ESTADO —tachado significa que no se está viendo— y el texto
 * describe la ACCIÓN que hace el botón, que es lo que necesita un lector
 * de pantalla para saber qué va a pasar al tocarlo.
 */
export const ASPECTO_CONTRASENA = {
  oculta: { icono: 'eye-off', accion: 'Mostrar contraseña' },
  visible: { icono: 'eye', accion: 'Ocultar contraseña' },
}

/**
 * Campo de contraseña con el botón del ojo para verla.
 *
 * Sirve para todos los formularios que piden una clave, no solo para
 * entrar: cambiar la contraseña o dar de baja la cuenta también necesitan
 * que el usuario pueda comprobar lo que escribió.
 */
export default function PasswordInput({ label, error, hint, className = '', required, ...props }) {
  const [visible, setVisible] = useState(false)
  const aspecto = ASPECTO_CONTRASENA[visible ? 'visible' : 'oculta']

  function campo(id, describedBy) {
    return (
      <div className="password-input">
        <input
          id={id}
          type={visible ? 'text' : 'password'}
          className={`input password-input__field ${error ? 'input--error' : ''}`}
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy}
          required={required}
          {...props}
        />
        <IconButton
          type="button"
          name={aspecto.icono}
          label={aspecto.accion}
          size="md"
          variant="ghost"
          className="password-input__toggle"
          aria-pressed={visible}
          onClick={() => setVisible((current) => !current)}
        />
      </div>
    )
  }

  // Sin label el campo va suelto: es el caso de un formulario embebido
  // donde el nombre ya está puesto en otro lado.
  if (!label) {
    return campo(undefined, undefined)
  }

  return (
    <Field label={label} error={error} hint={hint} required={required} className={className}>
      {(id, describedBy) => campo(id, describedBy)}
    </Field>
  )
}