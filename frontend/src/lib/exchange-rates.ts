import { createCachedRequest } from '@/lib/cached-request'

interface DailyExchangeRates {
  eur: number
  usd: number
  date: string
}

export interface ExchangeRates extends DailyExchangeRates {
  previous: DailyExchangeRates | null
}

interface RateResponse {
  base?: string
  quote?: string
  rate?: number
  date?: string
}

interface ExchangeRatesCache {
  response: RateResponse[]
  previousResponse: RateResponse[] | null
  fetchedAt: number
}

const cacheDuration = 6 * 60 * 60_000
const incompleteCacheDuration = 5 * 60_000
const apiUrl = 'https://api.frankfurter.dev/v2/rates?base=EUR&quotes=HUF,USD&providers=ECB'

function parseExchangeRates(response: RateResponse[]): DailyExchangeRates {
  if (!Array.isArray(response)) {
    throw new Error('Invalid exchange rate data.')
  }

  const huf = response.find((item) => item?.base === 'EUR' && item.quote === 'HUF')
  const usd = response.find((item) => item?.base === 'EUR' && item.quote === 'USD')

  if (
    typeof huf?.rate !== 'number'
    || !Number.isFinite(huf.rate)
    || huf.rate <= 0
    || typeof usd?.rate !== 'number'
    || !Number.isFinite(usd.rate)
    || usd.rate <= 0
    || typeof huf.date !== 'string'
    || !/^\d{4}-\d{2}-\d{2}$/.test(huf.date)
    || !Number.isFinite(Date.parse(huf.date))
    || new Date(huf.date).toISOString().slice(0, 10) !== huf.date
    || huf.date !== usd.date
  ) {
    throw new Error('Invalid exchange rate data.')
  }

  const usdInHuf = huf.rate / usd.rate

  if (!Number.isFinite(usdInHuf) || usdInHuf <= 0) {
    throw new Error('Invalid exchange rate data.')
  }

  return {
    eur: huf.rate,
    usd: usdInHuf,
    date: huf.date,
  }
}

function getPreviousRates(
  response: RateResponse[] | null,
  currentDate: string,
): DailyExchangeRates | null {
  if (!response) {
    return null
  }

  try {
    const previous = parseExchangeRates(response)

    if (previous.date < currentDate) {
      return previous
    }
  } catch {
    // Keep current rates when previous data is invalid.
  }

  return null
}

function readCachedExchangeRates(cache: ExchangeRatesCache): ExchangeRates | null {
  if (typeof cache.fetchedAt !== 'number') {
    return null
  }

  const age = Date.now() - cache.fetchedAt
  const rates = parseExchangeRates(cache.response)
  const previous = getPreviousRates(cache.previousResponse, rates.date)
  const duration = previous ? cacheDuration : incompleteCacheDuration

  if (!Number.isFinite(age) || age < 0 || age >= duration) {
    return null
  }

  return {
    ...rates,
    previous,
  }
}

async function fetchRateResponse(url: string): Promise<RateResponse[]> {
  const response = await fetch(url, {
    signal: AbortSignal.timeout(10_000),
    credentials: 'omit',
    referrerPolicy: 'no-referrer',
  })

  if (!response.ok) {
    throw new Error('Exchange rate request failed.')
  }

  return await response.json() as RateResponse[]
}

async function fetchExchangeRates() {
  const data = await fetchRateResponse(apiUrl)
  const rates = parseExchangeRates(data)
  const previousDate = new Date(rates.date)

  previousDate.setUTCDate(previousDate.getUTCDate() - 1)

  let previousResponse: RateResponse[] | null = null

  try {
    previousResponse = await fetchRateResponse(
      `${apiUrl}&date=${previousDate.toISOString().slice(0, 10)}`,
    )
  } catch {
    // Keep current rates when the previous request fails.
  }

  const previous = getPreviousRates(previousResponse, rates.date)

  return {
    value: {
      ...rates,
      previous,
    },
    cache: {
      response: data,
      previousResponse: previous ? previousResponse : null,
      fetchedAt: Date.now(),
    },
  }
}

export const getExchangeRates = createCachedRequest<[], ExchangeRates, ExchangeRatesCache>({
  storageKey: 'hirfolyam:exchange-rates:ecb:huf:v2',
  readCache: readCachedExchangeRates,
  fetchValue: fetchExchangeRates,
})
