import { Card, CardHeader } from '@/components/ui/Card'
import { formatNumber, formatSignedPercent } from '@/lib/format'
import type { Evidence } from '@/lib/types'

export function ChangedVariablesTable({ evidence }: { evidence: Evidence }) {
  const comparison = evidence.dataQuality ? 'Lecturas normales vs. lecturas sospechosas' : 'Promedio antes vs. durante el cambio'

  return (
    <Card>
      <CardHeader title="Variables que cambiaron" aside={comparison} />
      <ul className="grid sm:hidden">
        {evidence.changedVariables.map((change) => (
          <li key={change.variable} className="grid gap-0.5 border-b border-line-soft py-2 last:border-b-0">
            <span className="flex justify-between gap-3">
              <span className="font-semibold">{change.variable}</span>
              <ChangeValue value={change.changePercent} />
            </span>
            <span className="font-mono text-xs text-muted tabular">
              {formatNumber(change.before, 2)} → {formatNumber(change.after, 2)} {change.unit}
            </span>
          </li>
        ))}
      </ul>
      <div className="hidden overflow-x-auto sm:block">
        <table className="w-full min-w-[480px] text-[13px]">
          <thead>
            <tr className="border-b border-line text-left text-xs font-semibold text-muted">
              <th className="py-2 pr-3">Variable</th>
              <th className="py-2 pr-3 text-right">Antes</th>
              <th className="py-2 pr-3 text-right">Después</th>
              <th className="py-2 text-right">Cambio</th>
            </tr>
          </thead>
          <tbody>
            {evidence.changedVariables.map((change) => (
              <tr key={change.variable} className="border-b border-line-soft last:border-b-0">
                <td className="py-2 pr-3">{change.variable}</td>
                <td className="py-2 pr-3 text-right font-mono tabular">
                  {formatNumber(change.before, 2)} {change.unit}
                </td>
                <td className="py-2 pr-3 text-right font-mono tabular">
                  {formatNumber(change.after, 2)} {change.unit}
                </td>
                <td className="py-2 text-right">
                  <ChangeValue value={change.changePercent} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Card>
  )
}

function ChangeValue({ value }: { value: number }) {
  const color = Math.abs(value) >= 10 ? 'font-semibold text-critical' : 'text-muted'
  return <span className={`font-mono tabular ${color}`}>{formatSignedPercent(value)}</span>
}
