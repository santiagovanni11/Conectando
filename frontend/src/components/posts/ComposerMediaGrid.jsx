import IconButton from '../ui/IconButton'

export default function ComposerMediaGrid({ drafts, uploading, onRemove, onMove }) {
  if (drafts.length === 0 && !uploading) return null

  return (
    <div className="composer-media">
      {drafts.length > 0 && (
        <div className="composer-media__grid">
          {drafts.map((draft, index) => (
            <figure key={draft.id} className="composer-media__item">
              <img src={draft.url} alt="Foto seleccionada" loading="lazy" />

              <div className="composer-media__controls">
                <IconButton
                  name="chevron-left"
                  size="sm"
                  label="Mover antes"
                  disabled={index === 0}
                  onClick={() => onMove(draft.id, -1)}
                />
                <IconButton
                  name="chevron-right"
                  size="sm"
                  label="Mover después"
                  disabled={index === drafts.length - 1}
                  onClick={() => onMove(draft.id, 1)}
                />
              </div>

              <IconButton
                name="close"
                size="sm"
                label="Quitar foto"
                className="composer-media__remove"
                onClick={() => onRemove(draft.id)}
              />
            </figure>
          ))}
        </div>
      )}

      {uploading && <p className="composer-media__uploading" role="status">Subiendo fotos…</p>}
    </div>
  )
}