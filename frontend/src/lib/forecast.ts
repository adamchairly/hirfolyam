import { createCachedRequest } from '@/lib/cached-request'

export interface ForecastHour {
  time: string
  temperature: number
  code: number
  isDay: boolean
}

export interface ForecastDay {
  date: string
  minimum: number
  maximum: number
  code: number
}

export interface Forecast {
  temperature: number
  code: number
  isDay: boolean
  hours: ForecastHour[]
  days: ForecastDay[]
}

interface ForecastResponse {
  current?: {
    temperature_2m?: number
    weather_code?: number
    is_day?: number
  }
  hourly?: {
    time?: string[]
    temperature_2m?: number[]
    weather_code?: number[]
    is_day?: number[]
  }
  daily?: {
    time?: string[]
    weather_code?: number[]
    temperature_2m_min?: number[]
    temperature_2m_max?: number[]
  }
}

interface CityForecastsCache {
  responses: ForecastResponse[]
  fetchedAt: number
}

// The three largest Hungarian cities. Budapest comes first, the header shows it.
export const cities = [
  {
    name: 'Budapest',
    latitude: 47.4979,
    longitude: 19.0402,
  },
  {
    name: 'Debrecen',
    latitude: 47.5316,
    longitude: 21.6273,
  },
  {
    name: 'Szeged',
    latitude: 46.253,
    longitude: 20.1414,
  },
]

const cacheDuration = 15 * 60_000

function isTemperature(value: unknown): value is number {
  return typeof value === 'number' && Number.isFinite(value)
}

function isCode(value: unknown): value is number {
  return typeof value === 'number' && Number.isInteger(value)
}

function invalidForecast(): never {
  throw new Error('Invalid forecast data.')
}

function parseHours(hourly: ForecastResponse['hourly']): ForecastHour[] {
  return (hourly?.time ?? []).map((time, index) => {
    const temperature = hourly?.temperature_2m?.[index]
    const code = hourly?.weather_code?.[index]
    const isDay = hourly?.is_day?.[index]

    if (
      typeof time !== 'string'
      || !/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/.test(time)
      || !isTemperature(temperature)
      || !isCode(code)
      || (isDay !== 0 && isDay !== 1)
    ) {
      return invalidForecast()
    }

    return {
      time,
      temperature,
      code,
      isDay: isDay === 1,
    }
  })
}

function parseDays(daily: ForecastResponse['daily']): ForecastDay[] {
  return (daily?.time ?? []).map((date, index) => {
    const minimum = daily?.temperature_2m_min?.[index]
    const maximum = daily?.temperature_2m_max?.[index]
    const code = daily?.weather_code?.[index]

    if (
      typeof date !== 'string'
      || !/^\d{4}-\d{2}-\d{2}$/.test(date)
      || !isTemperature(minimum)
      || !isTemperature(maximum)
      || minimum > maximum
      || !isCode(code)
    ) {
      return invalidForecast()
    }

    return {
      date,
      minimum,
      maximum,
      code,
    }
  })
}

function parseForecast(response: ForecastResponse | null | undefined): Forecast {
  const temperature = response?.current?.temperature_2m
  const code = response?.current?.weather_code
  const isDay = response?.current?.is_day
  const hours = parseHours(response?.hourly)
  const days = parseDays(response?.daily)

  if (
    !isTemperature(temperature)
    || !isCode(code)
    || (isDay !== 0 && isDay !== 1)
    || hours.length === 0
    || days.length === 0
  ) {
    return invalidForecast()
  }

  return {
    temperature,
    code,
    isDay: isDay === 1,
    hours,
    days,
  }
}

function parseCityForecasts(responses: ForecastResponse[]): Forecast[] {
  if (!Array.isArray(responses) || responses.length !== cities.length) {
    return invalidForecast()
  }

  return responses.map(parseForecast)
}

function readCachedCityForecasts(cache: CityForecastsCache): Forecast[] | null {
  if (typeof cache.fetchedAt !== 'number') {
    return null
  }

  const age = Date.now() - cache.fetchedAt

  if (!Number.isFinite(age) || age < 0 || age >= cacheDuration) {
    return null
  }

  return parseCityForecasts(cache.responses)
}

async function requestForecast(location: { latitude: string, longitude: string, timezone: string }): Promise<unknown> {
  const parameters = new URLSearchParams({
    ...location,
    current: 'temperature_2m,weather_code,is_day',
    hourly: 'temperature_2m,weather_code,is_day',
    daily: 'weather_code,temperature_2m_min,temperature_2m_max',
    temperature_unit: 'celsius',
    forecast_hours: '24',
    forecast_days: '7',
  })

  const response = await fetch(`https://api.open-meteo.com/v1/forecast?${parameters}`, {
    signal: AbortSignal.timeout(10_000),
    credentials: 'omit',
    referrerPolicy: 'no-referrer',
  })

  if (!response.ok) {
    throw new Error('Forecast request failed.')
  }

  return response.json()
}

async function fetchCityForecasts() {
  // One request per city: the API works through the locations of a single request one after the other.
  const responses = await Promise.all(cities.map((city) => requestForecast({
    latitude: String(city.latitude),
    longitude: String(city.longitude),
    timezone: 'Europe/Budapest',
  }))) as ForecastResponse[]

  return {
    value: parseCityForecasts(responses),
    cache: {
      responses,
      fetchedAt: Date.now(),
    },
  }
}

export const getCityForecasts = createCachedRequest<[], Forecast[], CityForecastsCache>({
  storageKey: 'hirfolyam:weather:cities:v1',
  readCache: readCachedCityForecasts,
  fetchValue: fetchCityForecasts,
})

// Not cached, so the reader's location is not stored. Two decimals (about a kilometre) are enough.
export async function getLocalForecast(latitude: number, longitude: number): Promise<Forecast> {
  const response = await requestForecast({
    latitude: latitude.toFixed(2),
    longitude: longitude.toFixed(2),
    timezone: 'auto',
  }) as ForecastResponse

  return parseForecast(response)
}
