import { useEffect, useState } from 'react'

import { getSources } from '@/lib/api'
import type { Source } from '@/types/source'

export function useSources(refreshInterval?: number) {
  const [sources, setSources] = useState<Source[]>([])
  const [lastUpdatedAt, setLastUpdatedAt] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    const controller = new AbortController()

    let isPending = false

    async function loadSources() {
      if (isPending || controller.signal.aborted) {
        return
      }

      isPending = true

      try {
        const response = await getSources(controller.signal)

        if (!controller.signal.aborted) {
          setSources(response.items)
          setLastUpdatedAt(response.lastUpdatedAt ?? null)
          setError(null)
        }
      } catch {
        if (!controller.signal.aborted) {
          setError('Nem sikerült betölteni a forrásokat.')
        }
      } finally {
        isPending = false

        if (!controller.signal.aborted) {
          setIsLoading(false)
        }
      }
    }

    void loadSources()

    const timer = refreshInterval
      ? window.setInterval(() => {
          if (document.visibilityState === 'visible') {
            void loadSources()
          }
        }, refreshInterval)
      : undefined

    function refreshOnVisible() {
      if (refreshInterval && document.visibilityState === 'visible') {
        void loadSources()
      }
    }

    document.addEventListener('visibilitychange', refreshOnVisible)

    return () => {
      controller.abort()
      window.clearInterval(timer)
      document.removeEventListener('visibilitychange', refreshOnVisible)
    }
  }, [attempt, refreshInterval])

  function retry() {
    setIsLoading(true)
    setError(null)
    setAttempt((previous) => previous + 1)
  }

  return {
    sources,
    lastUpdatedAt,
    isLoading,
    error,
    retry,
  }
}
