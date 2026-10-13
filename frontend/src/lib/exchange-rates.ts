import { createCachedRequest } from '@/lib/cached-request'

export interface DailyExchangeRates {
  eur: number
  usd: number
  date: string
}

export interface ExchangeRates extends DailyExchangeRates {
  previous: DailyExchangeRates | null
  // Oldest first, the last day is the current one. Covers the past five years.
  history: DailyExchangeRates[]
}

interface RateResponse {
  base?: string
  quote?: string
  rate?: number
  date?: string
}

interface ExchangeRatesCache {
  response: RateResponse[]
  fetchedAt: number
}

const cacheDuration = 6 * 60 * 60_000
const incompleteCacheDuration = 5 * 60_000
const apiUrl = 'https://api.frankfurter.dev/v2/rates?base=EUR&quotes=HUF,USD&providers=ECB'

function isRate(value: unknown): value is number {
  return typeof value === 'number' && Number.isFinite(value) && value > 0
}

function isDate(value: unknown): value is string {
  return typeof value === 'string'
    && /^\d{4}-\d{2}-\d{2}$/.test(value)
    && Number.isFinite(Date.parse(value))
    && new Date(value).toISOString().slice(0, 10) === value
}

function invalidExchangeRates(): never {
  throw new Error('Invalid exchange rate data.')
}

function parseExchangeRateHistory(response: RateResponse[]): DailyExchangeRates[] {
  if (!Array.isArray(response)) {
    return invalidExchangeRates()
  }

  const days = new Map<string, { huf?: number, usd?: number }>()

  for (const item of response) {
    if (item?.base !== 'EUR' || !isRate(item.rate) || !isDate(item.date)) {
      return invalidExchangeRates()
    }

    const day = days.get(item.date) ?? {}

    if (item.quote === 'HUF') {
      day.huf = item.rate
    } else if (item.quote === 'USD') {
      day.usd = item.rate
    }

    days.set(item.date, day)
  }

  const history: DailyExchangeRates[] = []

  for (const [date, { huf, usd }] of days) {
    // A day missing one of the rates is left out of the history.
    if (huf === undefined || usd === undefined) {
      continue
    }

    const usdInHuf = huf / usd

    if (!isRate(usdInHuf)) {
      return invalidExchangeRates()
    }

    history.push({
      eur: huf,
      usd: usdInHuf,
      date,
    })
  }

  if (history.length === 0) {
    return invalidExchangeRates()
  }

  return history.sort((first, second) => first.date.localeCompare(second.date))
}

function parseExchangeRates(response: RateResponse[]): ExchangeRates {
  const history = parseExchangeRateHistory(response)

  return {
    ...history[history.length - 1],
    previous: history.length > 1 ? history[history.length - 2] : null,
    history,
  }
}

function readCachedExchangeRates(cache: ExchangeRatesCache): ExchangeRates | null {
  if (typeof cache.fetchedAt !== 'number') {
    return null
  }

  const age = Date.now() - cache.fetchedAt
  const rates = parseExchangeRates(cache.response)
  const duration = rates.previous ? cacheDuration : incompleteCacheDuration

  if (!Number.isFinite(age) || age < 0 || age >= duration) {
    return null
  }

  return rates
}

// One request brings the past five years, the header and the currency page both read it.
async function fetchExchangeRates() {
  const from = new Date()

  from.setUTCFullYear(from.getUTCFullYear() - 5)

  const response = await fetch(`${apiUrl}&from=${from.toISOString().slice(0, 10)}`, {
    signal: AbortSignal.timeout(10_000),
    credentials: 'omit',
    referrerPolicy: 'no-referrer',
  })

  if (!response.ok) {
    throw new Error('Exchange rate request failed.')
  }

  const data = await response.json() as RateResponse[]

  return {
    value: parseExchangeRates(data),
    cache: {
      response: data,
      fetchedAt: Date.now(),
    },
  }
}

export const getExchangeRates = createCachedRequest<[], ExchangeRates, ExchangeRatesCache>({
  storageKey: 'hirfolyam:exchange-rates:ecb:huf:v4',
  readCache: readCachedExchangeRates,
  fetchValue: fetchExchangeRates,
})
