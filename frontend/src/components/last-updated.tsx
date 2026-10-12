import { Clock } from 'lucide-react'

interface LastUpdatedProps {
  value: string | null
  now: Date
}

const dateTimeFormatter = new Intl.DateTimeFormat('hu-HU', {
  timeZone: 'Europe/Budapest',
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
})

export function LastUpdated({ value, now }: LastUpdatedProps) {
  if (!value) {
    return null
  }

  const updatedAt = new Date(value)

  if (Number.isNaN(updatedAt.getTime())) {
    return null
  }

  const minutesAgo = Math.max(0, Math.floor((now.getTime() - updatedAt.getTime()) / 60_000))
  const elapsed = minutesAgo === 0 ? 'most' : `${minutesAgo} perce`

  return (
    <p className="flex items-center justify-center gap-1.5 whitespace-nowrap text-[10px] leading-4 text-neutral-400 sm:text-[11px]">
      <Clock strokeWidth={1.5} className="size-3 shrink-0" />

      <span>
        Utoljára frissítve:{' '}
        <time
          dateTime={updatedAt.toISOString()}
          title={`A hírletöltés befejezése: ${dateTimeFormatter.format(updatedAt)}`}
          className="tabular-nums text-neutral-300"
        >
          {elapsed}
        </time>
      </span>
    </p>
  )
}
