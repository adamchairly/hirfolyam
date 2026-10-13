import { useEffect } from 'react'

import { ExchangeRateChart, ExchangeRateChartSkeleton } from '@/components/exchange-rate-chart'
import { Button } from '@/components/ui/button'
import { useExchangeRates } from '@/hooks/use-exchange-rates'

export function CurrencyPage() {
  // The header already loads the same rates, so they are usually waiting in the cache.
  const { rates, isLoading, hasError, retry } = useExchangeRates()

  useEffect(() => {
    document.title = 'Árfolyamok | hírfolyam'
  }, [])

  return (
    <>
      <div className="pt-6 sm:pt-8">
        {rates && (
          <div className="grid gap-4 md:grid-cols-2">
            <ExchangeRateChart currency="EUR" points={rates.history.map(({ date, eur }) => ({ date, rate: eur }))} />
            <ExchangeRateChart currency="USD" points={rates.history.map(({ date, usd }) => ({ date, rate: usd }))} />
          </div>
        )}

        {!rates && isLoading && (
          <div role="status" className="grid gap-4 md:grid-cols-2">
            <span className="sr-only">Árfolyamok betöltése…</span>
            <ExchangeRateChartSkeleton />
            <ExchangeRateChartSkeleton />
          </div>
        )}

        {!rates && !isLoading && hasError && (
          <div className="space-y-4 py-12 text-center">
            <p role="alert" className="text-neutral-300">Az árfolyamok most nem elérhetők.</p>
            <Button variant="outline" onClick={retry}>Újrapróbálom</Button>
          </div>
        )}
      </div>

      <p className="mt-6 text-center text-[11px] text-neutral-500">
        <a
          href="https://www.ecb.europa.eu/stats/policy_and_exchange_rates/euro_reference_exchange_rates/html/index.en.html"
          target="_blank"
          rel="noopener noreferrer"
          className="transition-colors hover:text-neutral-300"
        >
          ECB napi referenciaárfolyamok, Frankfurter API
        </a>
      </p>
    </>
  )
}