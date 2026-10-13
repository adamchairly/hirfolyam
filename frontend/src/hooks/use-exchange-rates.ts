import { useEffect, useState } from 'react'

import { getExchangeRates, type ExchangeRates } from '@/lib/exchange-rates'

export function useExchangeRates() {
  const [rates, setRates] = useState<ExchangeRates | null>(null)
  const [attempt, setAttempt] = useState(0)
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
        // The last rates stay on screen when a refresh fails.
        if (!disposed) {
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
  }, [attempt])

  function retry() {
    setIsLoading(true)
    setHasError(false)
    setAttempt((previous) => previous + 1)
  }

  return {
    rates,
    isLoading,
    hasError,
    retry,
  }
}
