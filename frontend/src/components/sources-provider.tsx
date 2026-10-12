import type { ReactNode } from 'react'

import { SourcesContext } from '@/hooks/use-source-selection'
import { useSources } from '@/hooks/use-sources'
import { useStoredValue } from '@/hooks/use-stored-value'
import { createStoredValue } from '@/lib/stored-value'

function createStoredKeys(storageKey: string) {
  return createStoredValue<string[]>(
    storageKey,
    [],
    (stored) => {
      const keys: unknown = JSON.parse(stored)

      if (!Array.isArray(keys)) {
        return []
      }

      return keys.filter((key): key is string => typeof key === 'string')
    },
    JSON.stringify,
  )
}

const disabledSources = createStoredKeys('hirfolyam.disabledSources')
const disabledCategories = createStoredKeys('hirfolyam.disabledCategories')

function toggleKey(keys: string[], key: string) {
  return keys.includes(key)
    ? keys.filter((storedKey) => storedKey !== key)
    : [...keys, key]
}

export function SourcesProvider({ children }: { children: ReactNode }) {
  const sourceList = useSources(60_000)
  const disabledSourceKeys = useStoredValue(disabledSources)
  const disabledCategorySlugs = useStoredValue(disabledCategories)
  const sourceFilter = sourceList.isLoading || sourceList.error
    ? null
    : sourceList.sources
        .filter((source) => !disabledSourceKeys.includes(source.key))
        .map((source) => source.key)
        .sort()
        .join(',')

  function toggleSource(key: string) {
    disabledSources.set(toggleKey(disabledSources.get(), key))
  }

  // The sources of a category are switched off and on together with it.
  function toggleCategory(slug: string) {
    const isDisabling = !disabledCategorySlugs.includes(slug)
    const categorySourceKeys = sourceList.sources
      .filter((source) => source.category === slug)
      .map((source) => source.key)
    const otherKeys = disabledSources.get().filter((key) => !categorySourceKeys.includes(key))

    disabledCategories.set(toggleKey(disabledCategories.get(), slug))
    disabledSources.set(isDisabling ? [...otherKeys, ...categorySourceKeys] : otherKeys)
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
