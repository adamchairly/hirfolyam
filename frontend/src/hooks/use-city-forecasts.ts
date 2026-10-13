import { useEffect, useState } from 'react'

import { getCityForecasts, type Forecast } from '@/lib/forecast'

export function useCityForecasts() {
  const [forecasts, setForecasts] = useState<Forecast[] | null>(null)
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
        const currentForecasts = await getCityForecasts()

        if (!disposed) {
          setForecasts(currentForecasts)
          setHasError(false)
        }
      } catch {
        // The last forecasts stay on screen when a refresh fails.
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
    forecasts,
    isLoading,
    hasError,
    retry,
  }
}
