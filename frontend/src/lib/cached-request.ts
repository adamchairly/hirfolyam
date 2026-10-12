interface CachedRequestOptions<TArgs extends unknown[], TValue, TCache> {
  storageKey: string
  // Returns null, or throws, when the stored answer can not be used anymore.
  readCache: (cache: TCache, ...args: TArgs) => TValue | null
  fetchValue: (...args: TArgs) => Promise<{ value: TValue, cache: TCache }>
}

// Answers from memory or local storage while the stored answer is usable,
// and shares one request between the callers that ask at the same time.
export function createCachedRequest<TArgs extends unknown[], TValue, TCache>({
  storageKey,
  readCache,
  fetchValue,
}: CachedRequestOptions<TArgs, TValue, TCache>) {
  let cache: TCache | null = null
  let pendingRequest: Promise<TValue> | null = null

  function getCachedValue(...args: TArgs): TValue | null {
    try {
      if (!cache) {
        const stored = localStorage.getItem(storageKey)

        if (stored) {
          cache = JSON.parse(stored) as TCache
        }
      }

      return cache ? readCache(cache, ...args) : null
    } catch {
      cache = null

      return null
    }
  }

  async function fetchAndStore(...args: TArgs): Promise<TValue> {
    const result = await fetchValue(...args)

    cache = result.cache

    try {
      localStorage.setItem(storageKey, JSON.stringify(cache))
    } catch {
      // Keep the memory cache when browser storage is not available.
    }

    return result.value
  }

  return async (...args: TArgs): Promise<TValue> => {
    const cachedValue = getCachedValue(...args)

    if (cachedValue !== null) {
      return cachedValue
    }

    if (!pendingRequest) {
      pendingRequest = fetchAndStore(...args).finally(() => {
        pendingRequest = null
      })
    }

    return pendingRequest
  }
}
