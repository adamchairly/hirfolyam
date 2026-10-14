import { useEffect, useState } from 'react'

import { getHotTopics } from '@/lib/api'
import type { HotTopic, HotTopicPeriod } from '@/types/hot-topic'

// The topics change at most once an hour; a missed refresh only delays the next summary.
const refreshInterval = 5 * 60_000

interface LoadedTopics {
  period: HotTopicPeriod
  topics: HotTopic[]
}

export function useHotTopics(period: HotTopicPeriod) {
  const [loaded, setLoaded] = useState<LoadedTopics | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    let isPending = false

    async function loadHotTopics() {
      if (isPending || controller.signal.aborted) {
        return
      }

      isPending = true

      try {
        const response = await getHotTopics(period, controller.signal)

        if (!controller.signal.aborted) {
          setLoaded({ period, topics: response.items })
        }
      } catch {
        // The block is optional: a failed refresh keeps the last topics, a failed switch shows none.
        if (!controller.signal.aborted) {
          setLoaded((previous) => previous?.period === period ? previous : { period, topics: [] })
        }
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
  }, [period])

  return {
    // Until the selected period arrives, these are the topics of the previous one.
    topics: loaded?.topics ?? [],
    isLoading: loaded?.period !== period,
  }
}
