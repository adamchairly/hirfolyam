import { useEffect, useState } from 'react'

import { getExchangeRates, type ExchangeRates } from '@/lib/exchange-rates'

export function useExchangeRates() {
  const [rates, setRates] = useState<ExchangeRates | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [hasError, setHasError] = useState(false)

  useEffect(() => {
    let disposed = false
    let loading = false

    async function refresh() {
      if (document.hidden || loading) {
        return
      }

      loading = true

      try {
        const currentRates = await getExchangeRates()

        if (!disposed) {
          setRates(currentRates)
          setHasError(false)
        }
      } catch {
        if (!disposed) {
          setRates(null)
          setHasError(true)
        }
      } finally {
        loading = false

        if (!disposed) {
          setIsLoading(false)
        }
      }
    }

    void refresh()

    const timer = window.setInterval(() => {
      void refresh()
    }, 60_000)

    document.addEventListener('visibilitychange', refresh)
    window.addEventListener('online', refresh)

    return () => {
      disposed = true
      window.clearInterval(timer)
      document.removeEventListener('visibilitychange', refresh)
      window.removeEventListener('online', refresh)
    }
  }, [])

  return {
    rates,
    isLoading,
    hasError,
  }
}
