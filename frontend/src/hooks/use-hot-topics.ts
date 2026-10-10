import { useEffect, useState } from 'react'

import { getHotTopics } from '@/lib/api'
import type { HotTopic } from '@/types/hot-topic'

// The topics change once an hour; a missed refresh only delays the next summary.
const refreshInterval = 5 * 60_000

export function useHotTopics() {
  const [topics, setTopics] = useState<HotTopic[]>([])
  const [generatedAt, setGeneratedAt] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    let isPending = false

    async function loadHotTopics() {
      if (isPending || controller.signal.aborted) {
        return
      }

      isPending = true

      try {
        const response = await getHotTopics(controller.signal)

        if (!controller.signal.aborted) {
          setTopics(response.items)
          setGeneratedAt(response.generatedAt ?? null)
        }
      } catch {
        // The block is optional, so a failed request keeps the last topics.
      } finally {
        isPending = false
      }
    }

    void loadHotTopics()

    const timer = window.setInterval(() => {
      if (document.visibilityState === 'visible') {
        void loadHotTopics()
      }
    }, refreshInterval)

    function refreshOnVisible() {
      if (document.visibilityState === 'visible') {
        void loadHotTopics()
      }
    }

    document.addEventListener('visibilitychange', refreshOnVisible)

    return () => {
      controller.abort()
      window.clearInterval(timer)
      document.removeEventListener('visibilitychange', refreshOnVisible)
    }
  }, [])

  return {
    topics,
    generatedAt,
  }
}
