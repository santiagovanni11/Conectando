/**
 * Datos de contacto del apartado de ayuda.
 *
 * El correo vive acá y no en el componente para que el texto visible, el
 * enlace y el botón no puedan quedar desincronizados entre sí.
 */
export const SUPPORT_EMAIL = 'conectandoredsocial@yahoo.com'

export const SUPPORT_SUBJECT = 'Consulta sobre Conectando'

/** Cuerpo del correo, con el saludo ya hecho para que solo quede escribirlo. */
export const SUPPORT_BODY = `Hola,

Tengo un inconveniente con Conectando:

-


Saludos.`

/** Enlace mailto completo: abre el cliente de correo con todo listo. */
export const SUPPORT_MAILTO = `mailto:${SUPPORT_EMAIL}?subject=${encodeURIComponent(
  SUPPORT_SUBJECT,
)}&body=${encodeURIComponent(SUPPORT_BODY)}`