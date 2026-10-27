import { useExchangeRates } from '@/hooks/use-exchange-rates'
import { cn } from '@/lib/utils'

const rateFormatter = new Intl.NumberFormat('hu-HU', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const percentageFormatter = new Intl.NumberFormat('hu-HU', {
  style: 'percent',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const dateFormatter = new Intl.DateTimeFormat('hu-HU', {
  timeZone: 'Europe/Budapest',
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
})

function getRateMovement(rate: number | undefined, previous: number | undefined) {
  if (rate === undefined || previous === undefined) {
    return {
      className: 'text-neutral-100',
      symbol: null,
      description: 'A változás nem érhető el.',
    }
  }

  const change = rate - previous

  if (change === 0) {
    return {
      className: 'text-neutral-100',
      symbol: '→',
      description: 'Változatlan.',
    }
  }

  const isRising = change > 0
  const amount = rateFormatter.format(Math.abs(change))
  const percentage = percentageFormatter.format(Math.abs(change) / previous)

  return {
    className: isRising ? 'text-red-400' : 'text-emerald-400',
    symbol: isRising ? '↑' : '↓',
    description: `${isRising ? 'Emelkedés' : 'Csökkenés'}: ${amount} Ft (${percentage}).`,
  }
}

export function ExchangeRatesWidget() {
  const { rates, isLoading, hasError } = useExchangeRates()
  const eur = rates ? rateFormatter.format(rates.eur) : '—'
  const usd = rates ? rateFormatter.format(rates.usd) : '—'
  const eurMovement = getRateMovement(rates?.eur, rates?.previous?.eur)
  const usdMovement = getRateMovement(rates?.usd, rates?.previous?.usd)
  const status = hasError ? 'Az árfolyamok most nem elérhetők.' : 'Árfolyamok betöltése…'
  const comparison = rates?.previous
    ? `Összehasonlítás az előző elérhető adattal (${dateFormatter.format(new Date(rates.previous.date))}). EUR: ${eurMovement.description} USD: ${usdMovement.description}`
    : 'Az előző napi változás nem érhető el.'
  const description = rates
    ? `Napi referenciaárfolyam: 1 EUR = ${eur} Ft, 1 USD = ${usd} Ft. Adat dátuma: ${dateFormatter.format(new Date(rates.date))}`
    : status

  return (
    <a
      href="https://www.ecb.europa.eu/stats/policy_and_exchange_rates/euro_reference_exchange_rates/html/index.en.html"
      target="_blank"
      rel="noopener noreferrer"
      aria-label={`${description} ${rates ? `${comparison} ` : ''}Forrás: ECB, Frankfurter API. A forrás megnyitása új lapon.`}
      aria-busy={isLoading}
      title={description}
      className="group flex min-h-6 shrink-0 items-center gap-3 rounded-sm text-xs"
    >
      <span className="flex items-baseline gap-1.5 whitespace-nowrap" aria-hidden="true">
        <span className="text-neutral-400 transition-colors group-hover:text-neutral-200">EUR</span>
        <span className={cn('inline-flex items-center gap-1 font-semibold tabular-nums', eurMovement.className)}>
          {eur}
          {eurMovement.symbol && <span>{eurMovement.symbol}</span>}
        </span>
        <span className="text-[10px] text-neutral-400 max-sm:hidden">Ft</span>
      </span>

      <span className="flex items-baseline gap-1.5 whitespace-nowrap" aria-hidden="true">
        <span className="text-neutral-400 transition-colors group-hover:text-neutral-200">USD</span>
        <span className={cn('inline-flex items-center gap-1 font-semibold tabular-nums', usdMovement.className)}>
          {usd}
          {usdMovement.symbol && <span>{usdMovement.symbol}</span>}
        </span>
        <span className="text-[10px] text-neutral-400 max-sm:hidden">Ft</span>
      </span>
    </a>
  )
}
