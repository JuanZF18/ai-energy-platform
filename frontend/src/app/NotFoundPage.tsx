import { Link } from 'react-router'
import { StateMessage } from '@/components/ui/States'

export function NotFoundPage() {
  return (
    <StateMessage
      eyebrow="No encontrado"
      title="Esta página no existe"
      description="Revisa la dirección o vuelve al dashboard."
      action={
        <Link to="/" className="inline-flex h-9 items-center rounded-lg border border-line px-3.5 text-[13px] font-semibold hover:bg-surface-2">
          Ir al dashboard
        </Link>
      }
    />
  )
}
