import { useEffect, useState } from 'react'

import { getWeather, type Weather } from '@/lib/weather'

export function useWeather() {
  const [weather, setWeather] = useState<Weather | null>(null)
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
        const currentWeather = await getWeather()

        if (!disposed) {
          setWeather(currentWeather)
          setHasError(false)
        }
      } catch {
        if (!disposed) {
          setWeather(null)
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
    weather,
    isLoading,
    hasError,
  }
}
