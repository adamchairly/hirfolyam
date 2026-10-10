import { useSyncExternalStore } from 'react'

import {
  getHotTopicsVisibility,
  subscribeToHotTopicsVisibility,
  updateHotTopicsVisibility,
} from '@/lib/hot-topics-visibility'

export function useHotTopicsVisibility() {
  const isVisible = useSyncExternalStore(subscribeToHotTopicsVisibility, getHotTopicsVisibility)

  return {
    isVisible,
    updateVisibility: updateHotTopicsVisibility,
  }
}
