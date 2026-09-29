export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

// El hub viaja por el mismo origen (proxy de Vite en desarrollo).
export const HUB_URL = import.meta.env.VITE_HUB_URL ?? API_BASE_URL