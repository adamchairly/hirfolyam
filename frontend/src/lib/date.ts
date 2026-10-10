const dateFormatter = new Intl.DateTimeFormat('hu-HU', {
  timeZone: 'Europe/Budapest',
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
})

const timeFormatter = new Intl.DateTimeFormat('hu-HU', {
  timeZone: 'Europe/Budapest',
  hour: '2-digit',
  minute: '2-digit',
  hourCycle: 'h23',
})

function getCalendarDay(date: Date): number {
  const parts = dateFormatter.formatToParts(date)
  const year = Number(parts.find((part) => part.type === 'year')?.value)
  const month = Number(parts.find((part) => part.type === 'month')?.value)
  const day = Number(parts.find((part) => part.type === 'day')?.value)

  return Date.UTC(year, month - 1, day) / 86_400_000
}

export function formatPublishedAt(value: string, now: Date): string {
  const publishedAt = new Date(value)
  const daysAgo = getCalendarDay(now) - getCalendarDay(publishedAt)
  const time = timeFormatter.format(publishedAt)

  if (daysAgo === 0) {
    return `ma ${time}`
  }

  if (daysAgo === 1) {
    return `tegnap ${time}`
  }

  return `${dateFormatter.format(publishedAt)} ${time}`
}
