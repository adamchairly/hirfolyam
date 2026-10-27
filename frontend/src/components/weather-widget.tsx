import { WeatherIcon } from '@/components/weather-icon'
import { useWeather } from '@/hooks/use-weather'
import { getWeatherCondition } from '@/lib/weather-condition'

function formatTemperature(value: number): string {
  return `${Math.round(value)}°C`
}

export function WeatherWidget() {
  const { weather, isLoading, hasError } = useWeather()
  const condition = weather ? getWeatherCondition(weather.code, weather.isDay) : null
  const temperature = weather ? formatTemperature(weather.temperature) : '—°C'
  const range = weather
    ? `${formatTemperature(weather.minimum)} / ${formatTemperature(weather.maximum)}`
    : '— / —'
  const status = hasError ? 'Az időjárás most nem elérhető.' : 'Időjárás betöltése…'
  const description = weather
    ? `Budapest: ${temperature}, ${condition?.label}. Napi minimum: ${formatTemperature(weather.minimum)}, maximum: ${formatTemperature(weather.maximum)}.`
    : `Budapest: ${status}`

  return (
    <a
      href="https://open-meteo.com/"
      target="_blank"
      rel="noopener noreferrer"
      aria-label={`${description} Adatok: Open Meteo, egész fokra kerekítve. A forrás megnyitása új lapon.`}
      aria-busy={isLoading}
      title={`${description} Forrás: Open Meteo (CC BY 4.0), egész fokra kerekítve.`}
      className="group flex min-h-6 shrink-0 items-center gap-2 rounded-sm"
    >
      <WeatherIcon kind={condition?.icon ?? 'unknown'} isDay={weather?.isDay} />

      {/* On mobile the range drops below, so the widget fits beside the two-row date and rates. */}
      <span className="grid grid-cols-[auto_auto] items-baseline gap-x-2 whitespace-nowrap sm:flex" aria-hidden="true">
        <span className="text-sm font-bold tabular-nums text-neutral-100">{temperature}</span>
        <span className="text-[10px] font-semibold uppercase tracking-wide text-neutral-300 transition-colors group-hover:text-neutral-50">
          Budapest
        </span>
        <span className="col-span-2 text-[11px] leading-4 tabular-nums text-neutral-400">
          {hasError ? 'Nem elérhető' : range}
        </span>
      </span>
    </a>
  )
}
