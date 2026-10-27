import { createCachedRequest } from '@/lib/cached-request'

export interface Weather {
  temperature: number
  minimum: number
  maximum: number
  code: number
  isDay: boolean
  date: string
}

interface WeatherResponse {
  current?: {
    time?: string
    temperature_2m?: number
    weather_code?: number
    is_day?: number
  }
  daily?: {
    time?: string[]
    temperature_2m_min?: number[]
    temperature_2m_max?: number[]
  }
}

interface WeatherCache {
  response: WeatherResponse
  fetchedAt: number
}

const cacheDuration = 15 * 60_000
const dateFormatter = new Intl.DateTimeFormat('en-CA', {
  timeZone: 'Europe/Budapest',
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
})

function parseWeather(response: WeatherResponse | null | undefined): Weather {
  const current = response?.current
  const daily = response?.daily
  const temperature = current?.temperature_2m
  const minimum = daily?.temperature_2m_min?.[0]
  const maximum = daily?.temperature_2m_max?.[0]
  const code = current?.weather_code
  const date = dateFormatter.format(new Date())

  if (
    typeof temperature !== 'number'
    || !Number.isFinite(temperature)
    || typeof minimum !== 'number'
    || !Number.isFinite(minimum)
    || typeof maximum !== 'number'
    || !Number.isFinite(maximum)
    || minimum > maximum
    || typeof code !== 'number'
    || !Number.isInteger(code)
    || (current?.is_day !== 0 && current?.is_day !== 1)
    || typeof current?.time !== 'string'
    || !current.time.startsWith(`${date}T`)
    || daily?.time?.[0] !== date
  ) {
    throw new Error('Invalid weather data.')
  }

  return {
    temperature,
    minimum,
    maximum,
    code,
    isDay: current.is_day === 1,
    date,
  }
}

function readCachedWeather(cache: WeatherCache): Weather | null {
  if (typeof cache.fetchedAt !== 'number') {
    return null
  }

  const age = Date.now() - cache.fetchedAt

  if (!Number.isFinite(age) || age < 0 || age >= cacheDuration) {
    return null
  }

  return parseWeather(cache.response)
}

async function fetchWeather() {
  const parameters = new URLSearchParams({
    latitude: '47.4979',
    longitude: '19.0402',
    current: 'temperature_2m,weather_code,is_day',
    daily: 'temperature_2m_min,temperature_2m_max',
    timezone: 'Europe/Budapest',
    temperature_unit: 'celsius',
    forecast_days: '1',
  })

  const response = await fetch(`https://api.open-meteo.com/v1/forecast?${parameters}`, {
    signal: AbortSignal.timeout(10_000),
    credentials: 'omit',
    referrerPolicy: 'no-referrer',
  })

  if (!response.ok) {
    throw new Error('Weather request failed.')
  }

  const data = await response.json() as WeatherResponse

  return {
    value: parseWeather(data),
    cache: {
      response: data,
      fetchedAt: Date.now(),
    },
  }
}

export const getWeather = createCachedRequest<[], Weather, WeatherCache>({
  storageKey: 'hirfolyam:weather:budapest:v1',
  readCache: readCachedWeather,
  fetchValue: fetchWeather,
})
