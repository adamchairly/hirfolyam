import { createContext, useContext } from 'react'

import type { useSources } from '@/hooks/use-sources'

interface SourceSelection extends ReturnType<typeof useSources> {
  disabledSourceKeys: string[]
  // Hidden categories leave the navigation and the mixed feeds.
  disabledCategorySlugs: string[]
  sourceFilter: string | null
  toggleSource: (key: string) => void
  toggleCategory: (slug: string) => void
}

export const SourcesContext = createContext<SourceSelection | null>(null)

export function useSourceSelection() {
  const selection = useContext(SourcesContext)

  if (!selection) {
    throw new Error('SourcesProvider is required.')
  }

  return selection
}
