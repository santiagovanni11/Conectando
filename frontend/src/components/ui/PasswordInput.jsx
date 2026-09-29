import { useState } from 'react'
import Field from './Field'
import IconButton from './IconButton'

/**
 * Campo de contraseña con el botón del ojo para verla.
 *
 * Sirve para todos los formularios que piden una clave, no solo para
 * entrar: cambiar la contraseña o dar de baja la cuenta también necesitan
 * que el usuario pueda comprobar lo que escribió.
 */
export default function PasswordInput({ label, error, hint, className = '', required, ...props }) {
  const [visible, setVisible] = useState(false)

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
          name={visible ? 'eye-off' : 'eye'}
          label={visible ? 'Ocultar contraseña' : 'Mostrar contraseña'}
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