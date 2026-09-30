import Button from '../ui/Button'
import Input from '../ui/Input'

/**
 * Paso 1: el correo.
 *
 * Dice explícitamente que el código va a llegar "si hay una cuenta con ese
 * correo". Es la forma honesta de explicar por qué alguien que se equivocó
 * de casilla no ve nada, sin confirmarle si existe o no: el servidor responde
 * siempre igual, y esta frase es la que evita que el usuario piense que la
 * app lo está chamuyando.
 */
export default function RequestCodeStep({ busy, error, onSubmit, onBack }) {
  return (
    <form
      className="password-reset__form"
      noValidate
      onSubmit={(event) => {
        event.preventDefault()
        const data = new FormData(event.currentTarget)
        onSubmit(data.get('email').trim())
      }}
    >
      <p className="password-reset__lead">
        Escribí tu email y te mandamos un código para poner una contraseña
        nueva. Si tenés una cuenta con ese correo, te va a llegar.
      </p>

      <Input
        name="email"
        label="Email"
        type="email"
        autoComplete="email"
        placeholder="tunombre@email.com"
        required
        autoFocus
      />

      {error && (
        <p className="form-alert" role="alert">
          {error}
        </p>
      )}

      <Button type="submit" block size="lg" loading={busy}>
        {busy ? 'Enviando…' : 'Enviar código'}
      </Button>

      <Button type="button" variant="ghost" block onClick={onBack}>
        Volver al inicio de sesión
      </Button>
    </form>
  )
}
