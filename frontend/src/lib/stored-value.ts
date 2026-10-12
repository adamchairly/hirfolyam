export interface StoredValue<T> {
  get: () => T
  subscribe: (listener: () => void) => () => void
  set: (value: T) => void
}

// Keeps a value in local storage, in step with the other tabs.
export function createStoredValue<T>(
  storageKey: string,
  fallback: T,
  parse: (stored: string) => T,
  serialize: (value: T) => string = String,
): StoredValue<T> {
  const listeners = new Set<() => void>()

  function read(): T {
    try {
      const stored = window.localStorage.getItem(storageKey)

      return stored === null ? fallback : parse(stored)
    } catch {
      return fallback
    }
  }

  function notifyListeners() {
    for (const listener of listeners) {
      listener()
    }
  }

  let value = read()

  window.addEventListener('storage', (event) => {
    if (event.key !== storageKey && event.key !== null) {
      return
    }

    value = read()
    notifyListeners()
  })

  return {
    get: () => value,
    subscribe(listener) {
      listeners.add(listener)

      return () => {
        listeners.delete(listener)
      }
    },
    set(nextValue) {
      value = nextValue

      try {
        window.localStorage.setItem(storageKey, serialize(nextValue))
      } catch {
        // Keep the value in memory if storage is not available.
      }

      notifyListeners()
    },
  }
}
