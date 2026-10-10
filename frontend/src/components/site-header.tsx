import { useState } from 'react'
import { Link, NavLink } from 'react-router-dom'

import { BookmarkIcon } from '@/components/bookmark-icon'
import { ExchangeRatesWidget } from '@/components/exchange-rates-widget'
import { SiteSearch } from '@/components/site-search'
import { WeatherWidget } from '@/components/weather-widget'
import { useCurrentTime } from '@/hooks/use-current-time'
import { useSourceSelection } from '@/hooks/use-source-selection'
import { categories } from '@/lib/categories'
import { cn } from '@/lib/utils'

const dateFormatter = new Intl.DateTimeFormat('hu-HU', {
  timeZone: 'Europe/Budapest',
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  weekday: 'long',
})

export function SiteHeader() {
  const [searchExpanded, setSearchExpanded] = useState(false)
  const today = useCurrentTime()
  const { disabledCategorySlugs } = useSourceSelection()
  const visibleCategories = categories.filter((category) => !disabledCategorySlugs.includes(category.slug))

  return (
    <header className="bg-neutral-900 text-neutral-50">
      <div className="overflow-x-auto border-b border-neutral-800/70 bg-neutral-950 [scrollbar-width:thin]">
        {/* Mobile: date above the rates on the left, weather centred across both rows on the right. */}
        <div className="mx-auto grid max-w-7xl grid-cols-[auto_auto] items-center justify-between gap-x-3 gap-y-1 px-4 py-1.5 text-xs leading-4 sm:flex sm:min-h-9 sm:min-w-max sm:gap-x-4 sm:px-6">
          <time className="whitespace-nowrap text-neutral-400" dateTime={today.toISOString()}>
            {dateFormatter.format(today).replace('. ', '.').replace(',', '')}
          </time>

          <div className="contents sm:flex sm:shrink-0 sm:items-center sm:gap-5">
            <div className="max-sm:row-start-2">
              <ExchangeRatesWidget />
            </div>

            <div className="max-sm:col-start-2 max-sm:row-span-2 max-sm:row-start-1 max-sm:justify-self-end">
              <WeatherWidget />
            </div>
          </div>
        </div>
      </div>

      <div className="relative mx-auto grid max-w-7xl grid-cols-[minmax(0,1fr)_auto] items-center gap-x-3 gap-y-4 px-4 py-5 sm:flex sm:h-28 sm:justify-center sm:px-6 sm:py-0">
        <Link
          to="/"
          aria-label="hírfolyam főoldal"
          className={cn(
            'col-span-2 justify-self-center text-3xl font-extrabold tracking-[-0.065em] min-[400px]:text-4xl sm:text-6xl',
            searchExpanded && 'sm:hidden md:block',
          )}
        >
          hírfolyam
        </Link>

        <div className="z-10 min-w-0 sm:absolute sm:left-6">
          <SiteSearch expanded={searchExpanded} onExpandedChange={setSearchExpanded} />
        </div>

        <div className="flex shrink-0 items-center gap-2 sm:absolute sm:right-6">
          <NavLink
            to="/sources"
            className={({ isActive }) => cn(
              'inline-flex min-h-10 items-center justify-center rounded-md border px-3 text-xs font-semibold transition-colors sm:px-4 sm:text-sm',
              isActive
                ? 'border-neutral-100 bg-neutral-100 text-neutral-950'
                : 'border-neutral-600 text-neutral-100 hover:border-neutral-300 hover:bg-neutral-800',
            )}
          >
            Források
          </NavLink>
          <NavLink
            to="/saved"
            aria-label="Mentett hírek"
            title="Mentett hírek"
            className={({ isActive }) => cn(
              'inline-flex size-10 items-center justify-center rounded-md border transition-colors',
              isActive
                ? 'border-neutral-100 bg-neutral-100 text-neutral-950'
                : 'border-neutral-600 text-neutral-100 hover:border-neutral-300 hover:bg-neutral-800',
            )}
          >
            <BookmarkIcon className="size-5" />
          </NavLink>
        </div>
      </div>

      <nav aria-label="Fő navigáció" className="border-b border-neutral-700">
        <div className="overflow-x-auto overscroll-x-contain px-4 [scrollbar-width:thin] sm:px-6">
          <ul className="mx-auto flex w-max min-w-full justify-center gap-6 sm:min-w-0 sm:gap-8">
            <li>
              <NavLink
                to="/fresh"
                className={({ isActive }) => cn(
                  'block whitespace-nowrap border-b-2 py-3 text-xs font-semibold uppercase tracking-wide transition-colors',
                  isActive
                    ? 'border-neutral-100 text-neutral-50'
                    : 'border-transparent text-neutral-400 hover:border-neutral-600 hover:text-neutral-100',
                )}
              >
                Friss
              </NavLink>
            </li>

            {visibleCategories.map((category) => (
              <li key={category.slug}>
                <NavLink
                  to={`/${category.slug}`}
                  className={({ isActive }) => cn(
                    'block whitespace-nowrap border-b-2 py-3 text-xs font-semibold uppercase tracking-wide transition-colors',
                    isActive
                      ? 'border-neutral-100 text-neutral-50'
                      : 'border-transparent text-neutral-400 hover:border-neutral-600 hover:text-neutral-100',
                  )}
                >
                  {category.name}
                </NavLink>
              </li>
            ))}

          </ul>
        </div>
      </nav>
    </header>
  )
}
