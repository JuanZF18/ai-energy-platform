import { Check, X } from 'lucide-react'
import { Card, CardHeader } from '@/components/ui/Card'
import { formatPlantDateTime } from '@/lib/format'
import type { Evidence } from '@/lib/types'
import { confirmsIssue } from './eventRelations'

export function EvidenceCards({ evidence }: { evidence: Evidence }) {
  return (
    <div className="grid gap-3 lg:grid-cols-[1.35fr_1fr]">
      <Card>
        <CardHeader title="Evidencia" aside="Hechos medibles que sostienen la conclusión" />
        <ul className="grid gap-2">
          {evidence.facts.map((fact) => {
            const isNegative = fact.startsWith('No hay') || fact.includes('no explica')
            return (
              <li key={fact} className="grid grid-cols-[18px_1fr] gap-2 text-[13px]">
                {isNegative ? <X className="mt-0.5 size-4 text-critical" aria-hidden /> : <Check className="mt-0.5 size-4 text-ok" aria-hidden />}
                <span>{fact}</span>
              </li>
            )
          })}
        </ul>
      </Card>
      <Card>
        <CardHeader title="Eventos relacionados" />
        {evidence.events.length === 0 ? (
          <p className="text-[13px] text-muted">No hay eventos operativos registrados cerca del cambio.</p>
        ) : (
          <ul className="grid gap-3">
            {evidence.events.map((event) => (
              <li key={event.timestamp} className="grid gap-1 text-[13px]">
                <span className="flex items-center gap-2">
                  <span className="rounded bg-dismissed-soft px-1.5 font-mono text-[11px] font-semibold text-dismissed">{event.type}</span>
                  <span className="font-mono text-[11px] text-muted">{formatPlantDateTime(event.timestamp)}</span>
                </span>
                <span>"{event.description}"</span>
                <EventVerdict explainsChange={event.explainsChange} confirmsIssue={confirmsIssue(evidence, event.type)} />
              </li>
            ))}
          </ul>
        )}
      </Card>
    </div>
  )
}

function EventVerdict({ explainsChange, confirmsIssue }: { explainsChange: boolean; confirmsIssue: boolean }) {
  if (confirmsIssue) return <b className="text-ok">Confirma el problema de calidad de datos</b>
  return <b className={explainsChange ? 'text-ok' : 'text-critical'}>{explainsChange ? 'Explica el cambio' : 'No explica el cambio'}</b>
}
