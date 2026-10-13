import { useEffect, useId, useRef, useState, type KeyboardEvent, type PointerEvent } from 'react'

import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'

export interface ExchangeRatePoint {
  date: string
  rate: number
}

type AxisUnit = 'week' | 'month' | 'year'

interface ChartRange {
  label: string
  period: string
  // Missing for the year to date range, which starts on the first of January.
  months?: number
  axisUnit: AxisUnit
}

const ranges: ChartRange[] = [
  { label: '1 hó', period: 'Egy hónap alatt', months: 1, axisUnit: 'week' },
  { label: '3 hó', period: 'Három hónap alatt', months: 3, axisUnit: 'month' },
  { label: '6 hó', period: 'Fél év alatt', months: 6, axisUnit: 'month' },
  { label: 'YTD', period: 'Év eleje óta', axisUnit: 'month' },
  { label: '1 év', period: 'Egy év alatt', months: 12, axisUnit: 'month' },
  { label: '2 év', period: 'Két év alatt', months: 24, axisUnit: 'year' },
  { label: '5 év', period: 'Öt év alatt', months: 60, axisUnit: 'year' },
]

const defaultRange = ranges[4]

const rateFormatter = new Intl.NumberFormat('hu-HU', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const percentageFormatter = new Intl.NumberFormat('hu-HU', {
  style: 'percent',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const dateFormatter = new Intl.DateTimeFormat('hu-HU', {
  timeZone: 'UTC',
  year: 'numeric',
  month: 'short',
  day: 'numeric',
})

const monthFormatter = new Intl.DateTimeFormat('hu-HU', {
  timeZone: 'UTC',
  month: 'short',
})

const dayFormatter = new Intl.DateTimeFormat('hu-HU', {
  timeZone: 'UTC',
  month: 'short',
  day: 'numeric',
})

const sectionClassName = 'rounded-md bg-neutral-900 p-4 sm:p-6'
const chartHeight = 200
const axisWidth = 44
const axisHeight = 20
const plotTop = 8
const minimumAxisLabelGap = 48
const lineColor = '#38bdf8'
const surfaceColor = '#171717'

// The rates are dated by day, so they are read at noon to stay on the same day everywhere.
function toDate(date: string): Date {
  return new Date(`${date}T12:00:00Z`)
}

// Starts from the rate that was in effect on the first day, so a range opening on a weekend still has its starting rate.
function getRangePoints(points: ExchangeRatePoint[], range: ChartRange): ExchangeRatePoint[] {
  const start = toDate(points[points.length - 1].date)

  if (range.months === undefined) {
    start.setUTCMonth(0, 1)
  } else {
    start.setUTCMonth(start.getUTCMonth() - range.months)
  }

  const startDate = start.toISOString().slice(0, 10)
  const startIndex = points.findLastIndex((point) => point.date <= startDate)

  return points.slice(Math.max(startIndex, 0))
}

function getNiceStep(range: number, count: number): number {
  const raw = range / count
  const magnitude = 10 ** Math.floor(Math.log10(raw))
  const normalized = raw / magnitude
  const nice = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 2.5 ? 2.5 : normalized <= 5 ? 5 : 10

  return nice * magnitude
}

function getScale(points: ExchangeRatePoint[]) {
  const rates = points.map((point) => point.rate)
  const minimum = Math.min(...rates)
  const maximum = Math.max(...rates)
  const padding = (maximum - minimum) * 0.08 || maximum * 0.01
  const low = minimum - padding
  const high = maximum + padding
  const step = getNiceStep(high - low, 4)
  const ticks: number[] = []

  for (let tick = Math.ceil(low / step) * step; tick <= high; tick += step) {
    ticks.push(tick)
  }

  return {
    low,
    high,
    ticks,
    tickFormatter: new Intl.NumberFormat('hu-HU', {
      maximumFractionDigits: step >= 1 ? 0 : step >= 0.1 ? 1 : 2,
    }),
  }
}

function getPeriodKey(date: string, unit: AxisUnit): string {
  if (unit === 'year') {
    return date.slice(0, 4)
  }

  if (unit === 'month') {
    return date.slice(0, 7)
  }

  const monday = toDate(date)

  monday.setUTCDate(monday.getUTCDate() - (monday.getUTCDay() + 6) % 7)

  return monday.toISOString().slice(0, 10)
}

function formatAxisLabel(date: string, unit: AxisUnit): string {
  if (unit === 'year') {
    return date.slice(0, 4)
  }

  const formatter = unit === 'month' ? monthFormatter : dayFormatter

  return formatter.format(toDate(date)).replace(/\.$/, '')
}

// The first trading day of each week, month or year, thinned out so the labels do not touch.
function getAxisLabels(points: ExchangeRatePoint[], unit: AxisUnit, getX: (index: number) => number) {
  const labels: { index: number, label: string }[] = []

  points.forEach((point, index) => {
    if (index === 0 || getPeriodKey(point.date, unit) === getPeriodKey(points[index - 1].date, unit)) {
      return
    }

    const previous = labels[labels.length - 1]

    if (!previous || getX(index) - getX(previous.index) >= minimumAxisLabelGap) {
      labels.push({ index, label: formatAxisLabel(point.date, unit) })
    }
  })

  return labels
}

function useElementWidth<T extends HTMLElement>() {
  const ref = useRef<T>(null)
  const [width, setWidth] = useState(0)

  useEffect(() => {
    const element = ref.current

    if (!element) {
      return
    }

    const observer = new ResizeObserver(([entry]) => {
      setWidth(entry.contentRect.width)
    })

    observer.observe(element)

    return () => observer.disconnect()
  }, [])

  return [ref, width] as const
}

function getMovement(current: number, previous: number | undefined) {
  if (previous === undefined) {
    return null
  }

  const change = current - previous

  if (change === 0) {
    return {
      className: 'text-neutral-300',
      text: '→ változatlan',
    }
  }

  const isRising = change > 0

  return {
    className: isRising ? 'text-red-400' : 'text-emerald-400',
    text: `${isRising ? '↑' : '↓'} ${rateFormatter.format(Math.abs(change))} Ft (${percentageFormatter.format(Math.abs(change) / previous)})`,
  }
}

interface ExchangeRateChartProps {
  currency: string
  points: ExchangeRatePoint[]
}

export function ExchangeRateChart({ currency, points: history }: ExchangeRateChartProps) {
  const titleId = useId()
  const [containerRef, width] = useElementWidth<HTMLDivElement>()
  const [range, setRange] = useState(defaultRange)
  const [activeIndex, setActiveIndex] = useState<number | null>(null)
  const points = getRangePoints(history, range)
  const current = points[points.length - 1]
  const first = points[0]
  const dailyMovement = getMovement(current.rate, history[history.length - 2]?.rate)
  const periodMovement = getMovement(current.rate, points.length > 1 ? first.rate : undefined)
  const { low, high, ticks, tickFormatter } = getScale(points)
  const plotWidth = Math.max(width - axisWidth, 0)
  const plotBottom = chartHeight - axisHeight
  const getX = (index: number) => points.length > 1 ? (index / (points.length - 1)) * plotWidth : plotWidth
  const getY = (rate: number) => plotTop + ((high - rate) / (high - low)) * (plotBottom - plotTop)
  const line = points.map((point, index) => `${index === 0 ? 'M' : 'L'}${getX(index)},${getY(point.rate)}`).join('')
  const area = `${line}L${getX(points.length - 1)},${plotBottom}L${getX(0)},${plotBottom}Z`
  const axisLabels = getAxisLabels(points, range.axisUnit, getX)
  const active = activeIndex === null ? null : points[activeIndex] ?? null
  const rates = points.map((point) => point.rate)
  const summary = `${currency} árfolyam forintban, ${dateFormatter.format(toDate(first.date))} és ${dateFormatter.format(toDate(current.date))} között. `
    + `Kezdő érték: ${rateFormatter.format(first.rate)} Ft, legalacsonyabb: ${rateFormatter.format(Math.min(...rates))} Ft, `
    + `legmagasabb: ${rateFormatter.format(Math.max(...rates))} Ft, utolsó: ${rateFormatter.format(current.rate)} Ft.`

  function selectRange(nextRange: ChartRange) {
    setRange(nextRange)
    setActiveIndex(null)
  }

  function handlePointerMove(event: PointerEvent<HTMLDivElement>) {
    const bounds = event.currentTarget.getBoundingClientRect()
    const position = (event.clientX - bounds.left) / Math.max(plotWidth, 1)

    setActiveIndex(Math.min(Math.max(Math.round(position * (points.length - 1)), 0), points.length - 1))
  }

  function handleKeyDown(event: KeyboardEvent<HTMLDivElement>) {
    const lastIndex = points.length - 1
    const index = activeIndex ?? lastIndex
    const nextIndex = {
      ArrowLeft: index - 1,
      ArrowRight: index + 1,
      Home: 0,
      End: lastIndex,
    }[event.key]

    if (nextIndex === undefined) {
      return
    }

    event.preventDefault()
    setActiveIndex(Math.min(Math.max(nextIndex, 0), lastIndex))
  }

  return (
    <section aria-labelledby={titleId} className={sectionClassName}>
      <h2 id={titleId} className="text-sm font-semibold text-neutral-300">
        {currency} / HUF
      </h2>

      <div className="mt-3 flex flex-wrap items-center justify-between gap-x-4 gap-y-2">
        <p className="text-4xl font-bold tracking-tight">
          {rateFormatter.format(current.rate)}
          <span className="ml-1.5 text-base font-semibold text-neutral-400">Ft</span>
        </p>

        <dl className="grid grid-cols-[auto_auto] gap-x-3 gap-y-0.5 text-xs">
          {dailyMovement && (
            <>
              <dt className="text-neutral-400">Előző naphoz</dt>
              <dd className={cn('text-right font-semibold tabular-nums', dailyMovement.className)}>{dailyMovement.text}</dd>
            </>
          )}
          {periodMovement && (
            <>
              <dt className="text-neutral-400">{range.period}</dt>
              <dd className={cn('text-right font-semibold tabular-nums', periodMovement.className)}>{periodMovement.text}</dd>
            </>
          )}
        </dl>
      </div>

      <div
        ref={containerRef}
        role="img"
        aria-label={summary}
        tabIndex={0}
        onKeyDown={handleKeyDown}
        onBlur={() => setActiveIndex(null)}
        className="relative mt-6 rounded-sm outline-none focus-visible:ring-2 focus-visible:ring-neutral-500 focus-visible:ring-offset-4 focus-visible:ring-offset-neutral-900"
        style={{ height: chartHeight }}
      >
        {width > 0 && (
          <svg width={width} height={chartHeight} aria-hidden="true" className="block overflow-visible">
            {ticks.map((tick) => (
              <g key={tick}>
                <line x1={0} x2={plotWidth} y1={getY(tick)} y2={getY(tick)} className="stroke-neutral-800" strokeWidth={1} />
                <text
                  x={plotWidth + 8}
                  y={getY(tick)}
                  dominantBaseline="middle"
                  className="fill-neutral-500 text-[11px] tabular-nums"
                >
                  {tickFormatter.format(tick)}
                </text>
              </g>
            ))}

            {axisLabels.map(({ index, label }) => (
              <text
                key={index}
                x={getX(index)}
                y={chartHeight - 4}
                textAnchor="middle"
                className="fill-neutral-500 text-[11px]"
              >
                {label}
              </text>
            ))}

            <path d={area} fill={lineColor} fillOpacity={0.1} />
            <path d={line} fill="none" stroke={lineColor} strokeWidth={2} strokeLinejoin="round" strokeLinecap="round" />

            {active && activeIndex !== null && (
              <line
                x1={getX(activeIndex)}
                x2={getX(activeIndex)}
                y1={plotTop}
                y2={plotBottom}
                className="stroke-neutral-500"
                strokeWidth={1}
              />
            )}

            <circle
              cx={getX(activeIndex ?? points.length - 1)}
              cy={getY((active ?? current).rate)}
              r={4}
              fill={lineColor}
              stroke={surfaceColor}
              strokeWidth={2}
            />
          </svg>
        )}

        {/* The hover layer covers only the plot, the axis labels stay outside of it. */}
        <div
          className="absolute top-0 left-0 touch-pan-y"
          style={{ width: plotWidth, height: plotBottom }}
          onPointerMove={handlePointerMove}
          onPointerDown={handlePointerMove}
          onPointerLeave={() => setActiveIndex(null)}
        />

        {active && activeIndex !== null && (
          <div
            aria-hidden="true"
            className="pointer-events-none absolute top-0 flex flex-col rounded-md border border-neutral-700 bg-neutral-950 px-2.5 py-1.5 shadow-lg"
            style={{
              left: getX(activeIndex),
              transform: `translateX(${getX(activeIndex) > plotWidth / 2 ? 'calc(-100% - 12px)' : '12px'})`,
            }}
          >
            <span className="text-sm font-semibold tabular-nums text-neutral-50">{rateFormatter.format(active.rate)} Ft</span>
            <span className="whitespace-nowrap text-[11px] text-neutral-400">{dateFormatter.format(toDate(active.date))}</span>
          </div>
        )}
      </div>

      <div role="group" aria-label="Időszak" className="mt-4 flex flex-wrap justify-center gap-1">
        {ranges.map((option) => (
          <button
            key={option.label}
            type="button"
            aria-pressed={option === range}
            onClick={() => selectRange(option)}
            className={cn(
              'rounded px-2 py-1 text-[11px] font-semibold transition-colors',
              option === range
                ? 'bg-neutral-800 text-neutral-50'
                : 'text-neutral-500 hover:text-neutral-200',
            )}
          >
            {option.label}
          </button>
        ))}
      </div>
    </section>
  )
}

export function ExchangeRateChartSkeleton() {
  return (
    <div aria-hidden="true" className={sectionClassName}>
      <Skeleton className="h-4 w-20" />
      <div className="mt-3 flex flex-wrap items-center justify-between gap-x-4 gap-y-2">
        <Skeleton className="h-10 w-36" />
        <div className="space-y-1.5">
          <Skeleton className="h-3 w-48" />
          <Skeleton className="h-3 w-44" />
        </div>
      </div>
      <Skeleton className="mt-6 w-full" style={{ height: chartHeight }} />
      <Skeleton className="mx-auto mt-4 h-6 w-64" />
    </div>
  )
}
