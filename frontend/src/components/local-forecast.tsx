import { LoaderCircle, MapPin, MapPinOff, RotateCw, type LucideIcon } from 'lucide-react'

import { ForecastSection } from '@/components/forecast-section'
import { useLocalForecast, type LocationStatus } from '@/hooks/use-local-forecast'
import type { Forecast } from '@/lib/forecast'
import { cn } from '@/lib/utils'

// Stands behind the blur until the reader shares their location.
const placeholderForecast: Forecast = {
  temperature: 18,
  code: 2,
  isDay: true,
  hours: Array.from({ length: 24 }, (_, index) => ({
    time: `2000-01-01T${String(index).padStart(2, '0')}:00`,
    temperature: 12 + Math.abs(12 - index) % 7,
    code: [0, 2, 3, 61][index % 4],
    isDay: index >= 7 && index < 19,
  })),
  days: Array.from({ length: 7 }, (_, index) => ({
    date: `2000-01-0${index + 3}`,
    minimum: 6 + index % 3,
    maximum: 15 + index % 4,
    code: [2, 0, 3, 61][index % 4],
  })),
}

const buttons: Record<LocationStatus, { icon: LucideIcon, label: string }> = {
  idle: {
    icon: MapPin,
    label: 'Helymeghatározás engedélyezése',
  },
  locating: {
    icon: LoaderCircle,
    label: 'Helymeghatározás engedélyezése',
  },
  denied: {
    icon: MapPinOff,
    label: 'Helymeghatározás letiltva',
  },
  error: {
    icon: RotateCw,
    label: 'Újrapróbálom',
  },
}

const title = (
  <>
    <MapPin strokeWidth={1.75} className="size-4" />
    Saját hely
  </>
)

export function LocalForecast() {
  const { forecast, status, locate } = useLocalForecast()

  if (forecast) {
    return <ForecastSection title={title} forecast={forecast} />
  }

  const { icon: Icon, label } = buttons[status]

  return (
    <div className="relative">
      <div inert className="select-none opacity-60 blur-sm">
        <ForecastSection title={title} forecast={placeholderForecast} />
      </div>

      <div className="absolute inset-0 flex items-center justify-center p-4">
        <button
          type="button"
          disabled={status === 'locating'}
          aria-busy={status === 'locating'}
          onClick={locate}
          className="inline-flex min-h-10 items-center gap-2 rounded-md bg-neutral-100 px-4 text-sm font-semibold text-neutral-950 transition-colors hover:bg-neutral-300 disabled:cursor-wait"
        >
          <Icon strokeWidth={1.75} className={cn('size-4', status === 'locating' && 'animate-spin')} />
          {label}
        </button>
      </div>
    </div>
  )
}
