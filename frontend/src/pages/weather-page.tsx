import { useEffect } from 'react'

import { ForecastSection, ForecastSectionSkeleton } from '@/components/forecast-section'
import { LocalForecast } from '@/components/local-forecast'
import { Button } from '@/components/ui/button'
import { useCityForecasts } from '@/hooks/use-city-forecasts'
import { cities } from '@/lib/forecast'

export function WeatherPage() {
  const { forecasts, isLoading, hasError, retry } = useCityForecasts()

  useEffect(() => {
    document.title = 'Időjárás | hírfolyam'
  }, [])

  return (
    <>
      <div className="space-y-4 pt-6 sm:pt-8">
        <LocalForecast />

        {forecasts && cities.map((city, index) => (
          <ForecastSection key={city.name} title={city.name} forecast={forecasts[index]} />
        ))}

        {!forecasts && isLoading && (
          <div role="status" className="space-y-4">
            <span className="sr-only">Időjárás betöltése…</span>
            {cities.map((city) => (
              <ForecastSectionSkeleton key={city.name} />
            ))}
          </div>
        )}

        {!forecasts && !isLoading && hasError && (
          <div className="space-y-4 py-12 text-center">
            <p role="alert" className="text-neutral-300">Az időjárás most nem elérhető.</p>
            <Button variant="outline" onClick={retry}>Újrapróbálom</Button>
          </div>
        )}
      </div>

      <p className="mt-6 text-center text-[11px] text-neutral-500">
        <a
          href="https://open-meteo.com/"
          target="_blank"
          rel="noopener noreferrer"
          className="transition-colors hover:text-neutral-300"
        >
          Data by Open-Meteo
        </a>
      </p>
    </>
  )
}
