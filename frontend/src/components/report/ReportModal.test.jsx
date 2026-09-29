import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import ReportModal from './ReportModal'
import { reportService, REPORT_REASONS } from '../../services/reportService'

vi.mock('../../services/reportService', async (importOriginal) => ({
  ...(await importOriginal()),
  reportService: { create: vi.fn(), getOwn: vi.fn() },
}))

const user = { id: 'u2', displayName: 'Ana' }

function renderModal(props = {}) {
  return render(<ReportModal user={user} onClose={vi.fn()} {...props} />)
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('ReportModal', () => {
  it('ofrece varios motivos, como en Facebook', () => {
    renderModal()

    const options = screen.getAllByRole('radio')
    expect(options).toHaveLength(REPORT_REASONS.length)
    expect(options.length).toBeGreaterThan(3)
  })

  it('no se puede enviar sin elegir un motivo', async () => {
    renderModal()

    // El botón arranca deshabilitado: el motivo es obligatorio.
    expect(screen.getByRole('button', { name: /enviar denuncia/i })).toBeDisabled()
  })

  it('envía el motivo elegido y el comentario', async () => {
    const onReported = vi.fn()
    reportService.create.mockResolvedValue({ id: 'r1' })
    const viewer = userEvent.setup()

    renderModal({ onReported })
    await viewer.click(screen.getByLabelText('Me acosa o me amenaza'))
    await viewer.type(screen.getByLabelText('Agregá información (opcional)'), 'me amenaza')
    await viewer.click(screen.getByRole('button', { name: /enviar denuncia/i }))

    expect(reportService.create).toHaveBeenCalledWith(user.id, 'harassment', 'me amenaza')
    expect(onReported).toHaveBeenCalled()
  })

  it('confirma que la denuncia se envió', async () => {
    reportService.create.mockResolvedValue({ id: 'r1' })
    const viewer = userEvent.setup()

    renderModal()
    await viewer.click(screen.getByLabelText('Es spam o publicidad'))
    await viewer.click(screen.getByRole('button', { name: /enviar denuncia/i }))

    expect(await screen.findByText('Denuncia enviada')).toBeInTheDocument()
  })

  it('muestra el error si el servidor rechaza la denuncia', async () => {
    reportService.create.mockRejectedValue(new Error('El motivo de la denuncia no es válido.'))
    const viewer = userEvent.setup()

    renderModal()
    await viewer.click(screen.getByLabelText('Es spam o publicidad'))
    await viewer.click(screen.getByRole('button', { name: /enviar denuncia/i }))

    expect(await screen.findByRole('alert')).toHaveTextContent('no es válido')
  })

  it('el comentario es opcional', async () => {
    reportService.create.mockResolvedValue({ id: 'r1' })
    const viewer = userEvent.setup()

    renderModal()
    await viewer.click(screen.getByLabelText('Otro motivo'))
    await viewer.click(screen.getByRole('button', { name: /enviar denuncia/i }))

    expect(reportService.create).toHaveBeenCalledWith(user.id, 'other', '')
  })
})