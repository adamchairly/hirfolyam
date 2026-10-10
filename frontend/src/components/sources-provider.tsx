import { useEffect, useState, type ReactNode } from 'react'

import { SourcesContext } from '@/hooks/use-source-selection'
import { useSources } from '@/hooks/use-sources'

const sourcesStorageKey = 'hirfolyam.disabledSources'
const categoriesStorageKey = 'hirfolyam.disabledCategories'

function readStoredKeys(storageKey: string): string[] {
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

// Keeps a list of keys in local storage, in step with the other tabs.
function useStoredKeys(storageKey: string) {
  const [keys, setKeys] = useState(() => readStoredKeys(storageKey))

  useEffect(() => {
    try {
      window.localStorage.setItem(storageKey, JSON.stringify(keys))
    } catch {
      // Keep the selection in memory if storage is not available.
    }
  }, [keys, storageKey])

  useEffect(() => {
    function syncSelection(event: StorageEvent) {
      if (event.key === storageKey || event.key === null) {
        setKeys(readStoredKeys(storageKey))
      }
    }

    window.addEventListener('storage', syncSelection)

    return () => {
      window.removeEventListener('storage', syncSelection)
    }
  }, [storageKey])

  return [keys, setKeys] as const
}

function toggleKey(keys: string[], key: string) {
  return keys.includes(key)
    ? keys.filter((storedKey) => storedKey !== key)
    : [...keys, key]
}

export function SourcesProvider({ children }: { children: ReactNode }) {
  const sourceList = useSources(60_000)
  const [disabledSourceKeys, setDisabledSourceKeys] = useStoredKeys(sourcesStorageKey)
  const [disabledCategorySlugs, setDisabledCategorySlugs] = useStoredKeys(categoriesStorageKey)
  const sourceFilter = sourceList.isLoading || sourceList.error
    ? null
    : sourceList.sources
        .filter((source) => !disabledSourceKeys.includes(source.key))
        .map((source) => source.key)
        .sort()
        .join(',')

  function toggleSource(key: string) {
    setDisabledSourceKeys((previous) => toggleKey(previous, key))
  }

  // The sources of a category are switched off and on together with it.
  function toggleCategory(slug: string) {
    const isDisabling = !disabledCategorySlugs.includes(slug)
    const categorySourceKeys = sourceList.sources
      .filter((source) => source.category === slug)
      .map((source) => source.key)

    setDisabledCategorySlugs((previous) => toggleKey(previous, slug))
    setDisabledSourceKeys((previous) => {
      const otherKeys = previous.filter((key) => !categorySourceKeys.includes(key))

      return isDisabling ? [...otherKeys, ...categorySourceKeys] : otherKeys
    })
  }

  return (
    <SourcesContext.Provider value={{
      ...sourceList,
      disabledSourceKeys,
      disabledCategorySlugs,
      sourceFilter,
      toggleSource,
      toggleCategory,
    }}>
      {children}
    </SourcesContext.Provider>
  )
}
