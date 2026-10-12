import { useStoredValue } from '@/hooks/use-stored-value'
import { hotTopicsVisibility } from '@/lib/hot-topics-visibility'

export function useHotTopicsVisibility() {
  const isVisible = useStoredValue(hotTopicsVisibility)

  return {
    isVisible,
    updateVisibility: hotTopicsVisibility.set,
  }
}
