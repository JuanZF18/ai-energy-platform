import { Card, CardHeader } from '@/components/ui/Card'
import type { Explanation } from '@/lib/types'

export function ExplanationCard({ explanation }: { explanation: Explanation }) {
  return (
    <Card>
      <CardHeader title="Causas probables y pasos a seguir" />
      <div className="grid gap-4 md:grid-cols-2">
        <div className="grid content-start gap-1.5">
          <h4 className="text-[13px] font-bold">Causas probables</h4>
          <ul className="grid list-disc gap-1 pl-4 text-[13px] text-muted">
            {explanation.possibleCauses.map((cause) => (
              <li key={cause}>{cause}</li>
            ))}
          </ul>
        </div>
        <div className="grid content-start gap-1.5">
          <h4 className="text-[13px] font-bold">Pasos a seguir, en orden</h4>
          <ol className="grid list-decimal gap-1 pl-4 text-[13px] text-ink marker:font-semibold marker:text-accent">
            {explanation.nextSteps.map((step) => (
              <li key={step}>{step}</li>
            ))}
          </ol>
        </div>
      </div>
    </Card>
  )
}
