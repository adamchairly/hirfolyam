import { isAxiosError } from 'axios'
import { useEffect, useState } from 'react'

import { getHotTopic } from '@/lib/api'
import type { HotTopicDetail } from '@/types/hot-topic'

export function useHotTopic(id: number) {
  const [topic, setTopic] = useState<HotTopicDetail | null>(null)
  const [attempt, setAttempt] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [isNotFound, setIsNotFound] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    async function loadTopic() {
      setIsLoading(true)
      setError(null)

      try {
        const response = await getHotTopic(id, controller.signal)

        if (!controller.signal.aborted) {
          setTopic(response)
        }
      } catch (loadError) {
        if (controller.signal.aborted) {
          return
        }

        if (isAxiosError(loadError) && loadError.response?.status === 404) {
          setIsNotFound(true)
        } else {
          setError('Nem sikerült betölteni az összefoglaló cikkeit.')
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false)
        }
      }
    }

    void loadTopic()

    return () => {
      controller.abort()
    }
  }, [id, attempt])

  function retry() {
    setIsLoading(true)
    setError(null)
    setAttempt((previous) => previous + 1)
  }

  return {
    topic,
    isLoading,
    isNotFound,
    error,
    retry,
  }
}
