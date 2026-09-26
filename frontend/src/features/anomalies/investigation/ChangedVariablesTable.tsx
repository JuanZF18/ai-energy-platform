import { Card, CardHeader } from '@/components/ui/Card'
import { formatNumber, formatSignedPercent } from '@/lib/format'
import type { Evidence } from '@/lib/types'

export function ChangedVariablesTable({ evidence }: { evidence: Evidence }) {
  const comparison = evidence.dataQuality ? 'Lecturas normales vs. lecturas sospechosas' : 'Promedio antes vs. durante el cambio'

  return (
    <Card>
      <CardHeader title="Variables que cambiaron" aside={comparison} />
      <div className="overflow-x-auto">
        <table className="w-full min-w-[480px] text-[13px]">
          <thead>
            <tr className="border-b border-line text-left text-[11px] font-semibold text-muted">
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
                <td className={`py-2 text-right font-mono tabular ${Math.abs(change.changePercent) >= 10 ? 'font-semibold text-critical' : 'text-muted'}`}>
                  {formatSignedPercent(change.changePercent)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Card>
  )
}
