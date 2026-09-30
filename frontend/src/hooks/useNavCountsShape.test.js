import { describe, it, expect } from 'vitest'
import { toCounts } from './useNavCounts'

/**
 * La garantía de que la navegación nunca recibe un valor raro.
 *
 * Va aparte del test del hook porque `toCounts` es una función pura: se
 * prueba sin montar React ni esperar promesas, y así queda obvio que el
 * crash no depende de cuándo llega la respuesta sino de qué trae.
 */
describe('toCounts', () => {
  const completa = {
    unreadMessages: 3,
    pendingFriendRequests: 1,
    unreadNotifications: 7,
  }

  it('pasa los números tal cual cuando el dato es correcto', () => {
    expect(toCounts(completa)).toEqual(completa)
  })

  it('devuelve ceros si no llega nada', () => {
    // El caso que tumbaba la app entera: apiRequest devolvía null ante un
    // 2xx de cuerpo no-JSON —típico del proxy despertando de una
    // suspensión— y AppNavigation leía counts.unreadMessages sin
    // protegerse, desde arriba de todas las páginas.
    expect(toCounts(null)).toEqual({
      unreadMessages: 0,
      pendingFriendRequests: 0,
      unreadNotifications: 0,
    })
  })

  it('devuelve ceros si no llega un objeto', () => {
    for (const raro of [undefined, 0, '', 'hola', true, []]) {
      expect(toCounts(raro).unreadMessages).toBe(0)
    }
  })

  it('rellena los campos que falten en vez de dejarlos sin definir', () => {
    const parcial = toCounts({ unreadMessages: 2 })

    expect(parcial).toEqual({
      unreadMessages: 2,
      pendingFriendRequests: 0,
      unreadNotifications: 0,
    })
    // Lo que de verdad importa: los tres existen y son números.
    Object.values(parcial).forEach((valor) => expect(typeof valor).toBe('number'))
  })

  it('descarta valores que no son números', () => {
    const sucio = toCounts({
      unreadMessages: 'muchos',
      pendingFriendRequests: null,
      unreadNotifications: NaN,
    })

    expect(sucio).toEqual({
      unreadMessages: 0,
      pendingFriendRequests: 0,
      unreadNotifications: 0,
    })
  })

  it('acepta los números que llegan como texto', () => {
    // El backend los manda como enteros, pero un numérico serializado
    // como texto no debería dejar la barra sin número.
    expect(toCounts({ unreadMessages: '4' }).unreadMessages).toBe(4)
  })

  it('nunca devuelve negativos ni infinitos', () => {
    const raro = toCounts({
      unreadMessages: -5,
      pendingFriendRequests: 2.7,
      unreadNotifications: Infinity,
    })

    expect(raro.unreadMessages).toBe(0)
    expect(raro.pendingFriendRequests).toBe(2)
    expect(raro.unreadNotifications).toBe(0)
  })

  it('ignora propiedades que no son del contador', () => {
    const conRuido = toCounts({ ...completa, extra: 'hola', id: 99 })

    expect(Object.keys(conRuido).sort()).toEqual([
      'pendingFriendRequests',
      'unreadMessages',
      'unreadNotifications',
    ])
  })
})
