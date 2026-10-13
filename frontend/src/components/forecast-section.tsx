import { useId, type ReactNode } from 'react'

import { Skeleton } from '@/components/ui/skeleton'
import { WeatherIcon } from '@/components/weather-icon'
import type { Forecast } from '@/lib/forecast'
import { getWeatherCondition } from '@/lib/weather-condition'

const weekdayFormatter = new Intl.DateTimeFormat('hu-HU', {
  timeZone: 'UTC',
  weekday: 'short',
})

const sectionClassName = 'rounded-md bg-neutral-900 p-4 sm:p-6'
// Relative, so the screen reader labels of the hidden hours do not widen the page.
const hourListClassName = 'relative mt-6 flex overflow-x-auto pb-2 [scrollbar-width:thin]'
const dayListClassName = 'mt-4 grid grid-cols-7'

function formatTemperature(value: number): string {
  return `${Math.round(value)}°`
}

// The times are already local to the place, so they are read as written.
function formatDay(date: string, index: number): string {
  return index === 0 ? 'Ma' : weekdayFormatter.format(new Date(`${date}T12:00:00Z`))
}

interface ForecastSectionProps {
  title: ReactNode
  forecast: Forecast
}

export function ForecastSection({ title, forecast }: ForecastSectionProps) {
  const titleId = useId()
  const condition = getWeatherCondition(forecast.code, forecast.isDay)
  const today = forecast.days[0]

  return (
    <section aria-labelledby={titleId} className={sectionClassName}>
      <h2 id={titleId} className="flex items-center gap-1.5 text-sm font-semibold text-neutral-300">
        {title}
      </h2>

      <div className="mt-3 flex items-center gap-4">
        <WeatherIcon kind={condition.icon} isDay={forecast.isDay} className="size-12" />

        <p className="text-5xl font-bold tracking-tight tabular-nums">
          {formatTemperature(forecast.temperature)}
        </p>

        <p className="flex flex-col gap-0.5 text-sm">
          <span className="text-neutral-200">{condition.label}</span>
          <span className="text-xs tabular-nums text-neutral-400">
            {formatTemperature(today.minimum)} / {formatTemperature(today.maximum)}
          </span>
        </p>
      </div>

      <ol aria-label="Óránként" className={hourListClassName}>
        {forecast.hours.map((hour) => {
          const hourCondition = getWeatherCondition(hour.code, hour.isDay)

          return (
            <li key={hour.time} className="flex w-12 shrink-0 flex-col items-center gap-1.5">
              <time dateTime={hour.time} className="text-xs tabular-nums text-neutral-400">
                {hour.time.slice(11, 16)}
              </time>
              <WeatherIcon kind={hourCondition.icon} isDay={hour.isDay} className="size-5" />
              <span className="sr-only">{hourCondition.label}</span>
              <span className="text-sm font-semibold tabular-nums">{formatTemperature(hour.temperature)}</span>
            </li>
          )
        })}
      </ol>

      <ol aria-label="Naponként" className={dayListClassName}>
        {forecast.days.map((day, index) => {
          const dayCondition = getWeatherCondition(day.code, true)

          return (
            <li key={day.date} className="flex flex-col items-center gap-1.5">
              <time dateTime={day.date} className="text-xs text-neutral-400">
                {formatDay(day.date, index)}
              </time>
              <WeatherIcon kind={dayCondition.icon} className="size-5" />
              <span className="sr-only">{dayCondition.label}</span>
              <span className="text-sm font-semibold tabular-nums">{formatTemperature(day.maximum)}</span>
              <span className="text-xs tabular-nums text-neutral-400">{formatTemperature(day.minimum)}</span>
            </li>
          )
        })}
      </ol>
    </section>
  )
}

export function ForecastSectionSkeleton() {
  return (
    <div aria-hidden="true" className={sectionClassName}>
      <Skeleton className="h-4 w-20" />

      <div className="mt-3 flex items-center gap-4">
        <Skeleton className="size-12" />
        <Skeleton className="h-12 w-20" />
        <div className="space-y-1.5">
          <Skeleton className="h-4 w-28" />
          <Skeleton className="h-3 w-16" />
        </div>
      </div>

      <div className={hourListClassName}>
        {Array.from({ length: 24 }, (_, index) => (
          <div key={index} className="flex w-12 shrink-0 flex-col items-center gap-1.5">
            <Skeleton className="h-3 w-8" />
            <Skeleton className="size-5" />
            <Skeleton className="h-4 w-6" />
          </div>
        ))}
      </div>

      <div className={dayListClassName}>
        {Array.from({ length: 7 }, (_, index) => (
          <div key={index} className="flex flex-col items-center gap-1.5">
            <Skeleton className="h-3 w-6" />
            <Skeleton className="size-5" />
            <Skeleton className="h-4 w-6" />
            <Skeleton className="h-3 w-5" />
          </div>
        ))}
      </div>
    </div>
  )
}
