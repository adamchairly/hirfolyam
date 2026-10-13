import { Link } from 'react-router-dom'

import { WeatherIcon } from '@/components/weather-icon'
import { useCityForecasts } from '@/hooks/use-city-forecasts'
import { getWeatherCondition } from '@/lib/weather-condition'

function formatTemperature(value: number): string {
  return `${Math.round(value)}°C`
}

export function WeatherWidget() {
  // Loads every city, so the weather page already finds them in the cache.
  const { forecasts, isLoading, hasError } = useCityForecasts()
  const weather = forecasts?.[0] ?? null
  const today = weather?.days[0]
  const condition = weather ? getWeatherCondition(weather.code, weather.isDay) : null
  const temperature = weather ? formatTemperature(weather.temperature) : '-°C'
  const range = today
    ? `${formatTemperature(today.minimum)} / ${formatTemperature(today.maximum)}`
    : '- / -'
  const status = hasError ? 'Az időjárás most nem elérhető.' : 'Időjárás betöltése…'
  const description = today
    ? `Budapest: ${temperature}, ${condition?.label}. Napi minimum: ${formatTemperature(today.minimum)}, maximum: ${formatTemperature(today.maximum)}.`
    : `Budapest: ${status}`

  return (
    <Link
      to="/weather"
      aria-label={`${description} Adatok: Open Meteo, egész fokra kerekítve. Az előrejelzés megnyitása.`}
      aria-busy={isLoading}
      className="group flex min-h-6 shrink-0 items-center gap-2 rounded-sm"
    >
      <WeatherIcon kind={condition?.icon ?? 'unknown'} isDay={weather?.isDay} />

      {/* On mobile the range drops below, so the widget fits beside the two-row date and rates. */}
      <span className="flex flex-col whitespace-nowrap sm:flex-row sm:items-baseline sm:gap-2" aria-hidden="true">
        <span className="flex items-baseline gap-2">
          <span className="text-sm font-bold tabular-nums text-neutral-100">{temperature}</span>
          <span className="text-[11px] font-semibold text-neutral-300 transition-colors group-hover:text-neutral-50">
            Budapest
          </span>
        </span>
        <span className="text-[11px] leading-4 tabular-nums text-neutral-400">
          {!weather && hasError ? 'Nem elérhető' : range}
        </span>
      </span>
    </Link>
  )
}
