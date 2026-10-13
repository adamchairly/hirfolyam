import { useId } from 'react'
import { Link } from 'react-router-dom'

import { ExchangeRateChart, ExchangeRateChartSkeleton } from '@/components/exchange-rate-chart'
import { Skeleton } from '@/components/ui/skeleton'
import { WeatherIcon } from '@/components/weather-icon'
import { useCityForecasts } from '@/hooks/use-city-forecasts'
import { useExchangeRates } from '@/hooks/use-exchange-rates'
import type { Forecast } from '@/lib/forecast'
import { getWeatherCondition } from '@/lib/weather-condition'

const sectionClassName = 'rounded-md bg-neutral-900 p-4'
const hourListClassName = 'mt-4 grid grid-cols-6'
// As many as fit the narrow column.
const hourCount = 6

function formatTemperature(value: number): string {
  return `${Math.round(value)}°`
}

// The header loads the same data, so both widgets usually start from the cache.
export function InfoWidgets() {
  return (
    <div className="space-y-4">
      <BudapestWeather />
      <EuroRate />
    </div>
  )
}

function BudapestWeather() {
  const { forecasts, isLoading } = useCityForecasts()
  const forecast = forecasts?.[0]

  if (forecast) {
    return <BudapestForecast forecast={forecast} />
  }

  return isLoading ? <BudapestForecastSkeleton /> : null
}

function BudapestForecast({ forecast }: { forecast: Forecast }) {
  const titleId = useId()
  const condition = getWeatherCondition(forecast.code, forecast.isDay)
  const today = forecast.days[0]

  return (
    <section aria-labelledby={titleId} className={sectionClassName}>
      <h2 id={titleId} className="text-sm font-semibold text-neutral-300">
        <Link to="/weather" className="underline-offset-4 transition-colors hover:text-neutral-50 hover:underline">
          Budapest
        </Link>
      </h2>

      <div className="mt-3 flex items-center gap-3">
        <WeatherIcon kind={condition.icon} isDay={forecast.isDay} className="size-9" />

        <p className="text-3xl font-bold tracking-tight tabular-nums">
          {formatTemperature(forecast.temperature)}
        </p>

        <p className="flex min-w-0 flex-col gap-0.5 text-sm">
          <span className="truncate text-neutral-200">{condition.label}</span>
          <span className="text-xs tabular-nums text-neutral-400">
            {formatTemperature(today.minimum)} / {formatTemperature(today.maximum)}
          </span>
        </p>
      </div>

      {/* The first hour is the current one, shown above. */}
      <ol aria-label="Óránként" className={hourListClassName}>
        {forecast.hours.slice(1, hourCount + 1).map((hour) => {
          const hourCondition = getWeatherCondition(hour.code, hour.isDay)

          return (
            <li key={hour.time} className="flex flex-col items-center gap-1.5">
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
    </section>
  )
}

function BudapestForecastSkeleton() {
  return (
    <div aria-hidden="true" className={sectionClassName}>
      <Skeleton className="h-4 w-16" />

      <div className="mt-3 flex items-center gap-3">
        <Skeleton className="size-9" />
        <Skeleton className="h-9 w-14" />
        <div className="space-y-1.5">
          <Skeleton className="h-4 w-24" />
          <Skeleton className="h-3 w-14" />
        </div>
      </div>

      <div className={hourListClassName}>
        {Array.from({ length: hourCount }, (_, index) => (
          <div key={index} className="flex flex-col items-center gap-1.5">
            <Skeleton className="h-3 w-8" />
            <Skeleton className="size-5" />
            <Skeleton className="h-4 w-6" />
          </div>
        ))}
      </div>
    </div>
  )
}

function EuroRate() {
  const { rates, isLoading } = useExchangeRates()

  if (rates) {
    return <ExchangeRateChart currency="EUR" points={rates.history.map(({ date, eur }) => ({ date, rate: eur }))} compact />
  }

  return isLoading ? <ExchangeRateChartSkeleton compact /> : null
}
