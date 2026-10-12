import { useEffect, useState } from 'react'

import { getVersion } from '@/lib/api'

export function useVersion() {
  const [commit, setCommit] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    getVersion(controller.signal)
      .then((response) => setCommit(response.commit))
      .catch(() => {
        // The footer simply shows no commit.
      })

    return () => {
      controller.abort()
    }
  }, [])

  return commit
}
