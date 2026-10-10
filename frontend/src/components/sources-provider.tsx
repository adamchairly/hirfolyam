import { useEffect, useState, type ReactNode } from 'react'

import { SourcesContext } from '@/hooks/use-source-selection'
import { useSources } from '@/hooks/use-sources'

const storageKey = 'hirfolyam.disabledSources'

function readDisabledSources(): string[] {
  try {
    const stored: unknown = JSON.parse(window.localStorage.getItem(storageKey) ?? '[]')

    if (!Array.isArray(stored)) {
      return []
    }

    return stored.filter((key): key is string => typeof key === 'string')
  } catch {
    return []
  }
}

export function SourcesProvider({ children }: { children: ReactNode }) {
  const sourceList = useSources(60_000)
  const [disabledSourceKeys, setDisabledSourceKeys] = useState(readDisabledSources)
  const sourceFilter = sourceList.isLoading || sourceList.error
    ? null
    : sourceList.sources
        .filter((source) => !disabledSourceKeys.includes(source.key))
        .map((source) => source.key)
        .sort()
        .join(',')

  useEffect(() => {
    try {
      window.localStorage.setItem(storageKey, JSON.stringify(disabledSourceKeys))
    } catch {
      // Keep the selection in memory if storage is not available.
    }
  }, [disabledSourceKeys])

  useEffect(() => {
    function syncSelection(event: StorageEvent) {
      if (event.key === storageKey || event.key === null) {
        setDisabledSourceKeys(readDisabledSources())
      }
    }

    window.addEventListener('storage', syncSelection)

    return () => {
      window.removeEventListener('storage', syncSelection)
    }
  }, [])

  function toggleSource(key: string) {
    setDisabledSourceKeys((previous) => {
      if (previous.includes(key)) {
        return previous.filter((sourceKey) => sourceKey !== key)
      }

      return [...previous, key]
    })
  }

  return (
    <SourcesContext.Provider value={{
      ...sourceList,
      disabledSourceKeys,
      sourceFilter,
      toggleSource,
    }}>
      {children}
    </SourcesContext.Provider>
  )
}
