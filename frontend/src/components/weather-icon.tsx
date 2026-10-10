import type { WeatherIconKind } from '@/lib/weather-condition'

export function WeatherIcon({ kind, isDay = true }: { kind: WeatherIconKind, isDay?: boolean }) {
  const hasCloud = ['partlyCloudy', 'cloud', 'rain', 'snow', 'storm'].includes(kind)

  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      className="size-6 shrink-0 text-neutral-300"
    >
      {kind === 'sun' && (
        <>
          <circle cx="12" cy="12" r="4" />
          <path d="M12 2v2m0 16v2M2 12h2m16 0h2M4.9 4.9l1.4 1.4m11.4 11.4 1.4 1.4M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4" />
        </>
      )}
      {kind === 'moon' && <path d="M20.8 13.2A9 9 0 0 1 10.8 3.2a9 9 0 1 0 10 10Z" />}
      {kind === 'partlyCloudy' && (
        isDay ? (
          <path d="M7 2v1M2 7h1m.5-3.5.8.8M10.5 3.5l-.8.8M4 9a3.5 3.5 0 1 1 6.5-2" />
        ) : (
          <path d="M10 3a5 5 0 0 0 3.5 5.8M10 3a5.5 5.5 0 0 0-6.3 8" />
        )
      )}
      {hasCloud && <path d="M7 16h11a4 4 0 1 0-.8-7.9A5.5 5.5 0 0 0 6.5 10 3 3 0 0 0 7 16Z" />}
      {kind === 'rain' && <path d="m8 19-1 2m5-2-1 2m5-2-1 2" />}
      {kind === 'snow' && <path d="M8 19v3m-1.3-.7 2.6-1.6m-2.6 0 2.6 1.6M16 19v3m-1.3-.7 2.6-1.6m-2.6 0 2.6 1.6" />}
      {kind === 'storm' && <path d="m13 17-3 3h4l-3 3" />}
      {kind === 'fog' && <path d="M5 8h14M3 12h15M7 16h14M4 20h13" />}
      {kind === 'unknown' && (
        <>
          <path d="M10 4a2 2 0 0 1 4 0v10.5a4 4 0 1 1-4 0ZM12 8v9" />
          <circle cx="12" cy="18" r="1" />
        </>
      )}
    </svg>
  )
}
