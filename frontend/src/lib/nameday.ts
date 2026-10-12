interface NamedayResponse {
  data?: {
    hu?: string
  }
}

interface NamedayCache {
  date: string
  names: string
}

const cacheKey = 'hirfolyam:nameday:hu:v1'

let cache: NamedayCache | null = null
let pendingRequest: Promise<string> | null = null

function getCachedNameday(date: string): string | null {
  try {
    if (!cache) {
      const stored = localStorage.getItem(cacheKey)

      if (stored) {
        cache = JSON.parse(stored) as NamedayCache
      }
    }

    return cache?.date === date && typeof cache.names === 'string' ? cache.names : null
  } catch {
    cache = null

    return null
  }
}

async function fetchNameday(date: string): Promise<string> {
  const [, month, day] = date.split('-')
  const parameters = new URLSearchParams({
    day: String(Number(day)),
    month: String(Number(month)),
    country: 'hu',
  })

  const response = await fetch(`https://nameday.abalin.net/api/V2/date?${parameters}`, {
    signal: AbortSignal.timeout(10_000),
    credentials: 'omit',
    referrerPolicy: 'no-referrer',
  })

  if (!response.ok) {
    throw new Error('Nameday request failed.')
  }

  const data = await response.json() as NamedayResponse
  const names = data.data?.hu?.trim()

  if (!names || names === 'n/a') {
    throw new Error('Invalid nameday data.')
  }

  cache = { date, names }

  try {
    localStorage.setItem(cacheKey, JSON.stringify(cache))
  } catch {
    // Keep the memory cache when browser storage is not available.
  }

  return names
}

/** Hungarian name day(s) for a Budapest calendar date in YYYY-MM-DD format. */
export async function getNameday(date: string): Promise<string> {
  const cachedNameday = getCachedNameday(date)

  if (cachedNameday) {
    return cachedNameday
  }

  if (!pendingRequest) {
    pendingRequest = fetchNameday(date).finally(() => {
      pendingRequest = null
    })
  }

  return pendingRequest
}
