import { createContext, useContext } from 'react'

import type { useSources } from '@/hooks/use-sources'

interface SourceSelection extends ReturnType<typeof useSources> {
  disabledSourceKeys: string[]
  sourceFilter: string | null
  toggleSource: (key: string) => void
}

export const SourcesContext = createContext<SourceSelection | null>(null)

export function useSourceSelection() {
  const selection = useContext(SourcesContext)

  if (!selection) {
    throw new Error('SourcesProvider is required.')
  }

  return selection
}
