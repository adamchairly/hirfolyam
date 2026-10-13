import { useCallback, useEffect, useState } from 'react'

import { getLocalForecast, type Forecast } from '@/lib/forecast'

export type LocationStatus = 'idle' | 'locating' | 'denied' | 'error'

export function useLocalForecast() {
  const [forecast, setForecast] = useState<Forecast | null>(null)
  const [status, setStatus] = useState<LocationStatus>('idle')

  const locate = useCallback(() => {
    if (!('geolocation' in navigator)) {
      setStatus('denied')

      return
    }

    setStatus('locating')

    navigator.geolocation.getCurrentPosition(
      async ({ coords }) => {
        try {
          setForecast(await getLocalForecast(coords.latitude, coords.longitude))
        } catch {
          setStatus('error')
        }
      },
      (error) => {
        setStatus(error.code === error.PERMISSION_DENIED ? 'denied' : 'error')
      },
      {
        maximumAge: 10 * 60_000,
        timeout: 15_000,
      },
    )
  }, [])

  useEffect(() => {
    let disposed = false

    // A reader who has already allowed the location sees their weather right away.
    async function restore() {
      try {
        const permission = await navigator.permissions.query({ name: 'geolocation' })

        if (disposed) {
          return
        }

        if (permission.state === 'granted') {
          locate()
        } else if (permission.state === 'denied') {
          setStatus('denied')
        }
      } catch {
        // Without the Permissions API the reader starts it with the button.
      }
    }

    void restore()

    return () => {
      disposed = true
    }
  }, [locate])

  return {
    forecast,
    status,
    locate,
  }
}
