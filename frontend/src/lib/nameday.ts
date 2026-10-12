import { createCachedRequest } from '@/lib/cached-request'

interface NamedayResponse {
  data?: {
    hu?: string
  }
}

interface NamedayCache {
  date: string
  names: string
}

async function fetchNameday(date: string) {
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

  return {
    value: names,
    cache: { date, names },
  }
}

/** Hungarian name day(s) for a Budapest calendar date in YYYY-MM-DD format. */
export const getNameday = createCachedRequest<[date: string], string, NamedayCache>({
  storageKey: 'hirfolyam:nameday:hu:v1',
  readCache: (cache, date) => cache.date === date && typeof cache.names === 'string' && cache.names
    ? cache.names
    : null,
  fetchValue: fetchNameday,
})
