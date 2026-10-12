import { useSyncExternalStore } from 'react'

import type { StoredValue } from '@/lib/stored-value'

export function useStoredValue<T>(storedValue: StoredValue<T>) {
  return useSyncExternalStore(storedValue.subscribe, storedValue.get)
}
