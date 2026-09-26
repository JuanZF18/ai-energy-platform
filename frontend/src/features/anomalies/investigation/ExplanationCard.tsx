import { Card, CardHeader } from '@/components/ui/Card'
import type { Explanation } from '@/lib/types'

export function ExplanationCard({ explanation }: { explanation: Explanation }) {
  return (
    <Card>
      <CardHeader title="Explicación y próximos pasos" />
      <div className="grid gap-4 md:grid-cols-2">
        <ExplanationList title="Posibles causas" items={explanation.possibleCauses} />
        <ExplanationList title="Pasos siguientes" items={explanation.nextSteps} />
      </div>
    </Card>
  )
}

function ExplanationList({ title, items }: { title: string; items: string[] }) {
  return (
    <div className="grid content-start gap-1.5">
      <h4 className="text-[13px] font-bold">{title}</h4>
      <ul className="grid list-disc gap-1 pl-4 text-[13px] text-muted">
        {items.map((item) => (
          <li key={item}>{item}</li>
        ))}
      </ul>
    </div>
  )
}
