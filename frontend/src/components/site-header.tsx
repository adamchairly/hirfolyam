import { Bookmark } from 'lucide-react'
import { useState } from 'react'
import { Link, NavLink } from 'react-router-dom'

import { SiteSearch } from '@/components/site-search'
import { useCurrentTime } from '@/hooks/use-current-time'
import { useNameday } from '@/hooks/use-nameday'
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
const isoDateFormatter = new Intl.DateTimeFormat('en-CA', {
  timeZone: 'Europe/Budapest',
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
})

export function SiteHeader() {
  const [searchExpanded, setSearchExpanded] = useState(false)
  const today = useCurrentTime()
  const nameday = useNameday(isoDateFormatter.format(today))
  const { disabledCategorySlugs } = useSourceSelection()
  const visibleCategories = categories.filter((category) => !disabledCategorySlugs.includes(category.slug))

  return (
    <header className="bg-neutral-900 text-neutral-50">
      <div className="overflow-x-auto bg-neutral-950 [scrollbar-width:thin]">
        <div className="mx-auto flex min-h-9 max-w-7xl items-center px-4 py-1.5 text-xs leading-4 sm:px-6">
          <p className="whitespace-nowrap text-neutral-400">
            <time dateTime={today.toISOString()}>
              {dateFormatter.format(today).replaceAll('. ', '.').replace(',', '')}
            </time>
            {nameday && <span className="text-neutral-300"> {nameday}</span>}
          </p>
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
              'inline-flex min-h-10 items-center justify-center rounded-md px-3 text-xs font-semibold transition-colors sm:px-4 sm:text-sm',
              isActive
                ? 'bg-neutral-100 text-neutral-950'
                : 'bg-black text-neutral-100 hover:bg-neutral-800',
            )}
          >
            Források
          </NavLink>
          <NavLink
            to="/saved"
            aria-label="Mentett hírek"
            title="Mentett hírek"
            className={({ isActive }) => cn(
              'inline-flex size-10 items-center justify-center rounded-md transition-colors',
              isActive
                ? 'bg-neutral-100 text-neutral-950'
                : 'bg-black text-neutral-100 hover:bg-neutral-800',
            )}
          >
            <Bookmark strokeWidth={1.75} className="size-5" />
          </NavLink>
        </div>
      </div>

      <nav aria-label="Fő navigáció">
        <div className="overflow-x-auto overscroll-x-contain px-4 [scrollbar-width:thin] sm:px-6">
          <ul className="mx-auto flex w-max min-w-full justify-center gap-6 sm:min-w-0 sm:gap-8">
            <li>
              <NavLink
                to="/fresh"
                className={({ isActive }) => cn(
                  'block whitespace-nowrap border-b-2 py-3 text-sm font-semibold transition-colors',
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
                    'block whitespace-nowrap border-b-2 py-3 text-sm font-semibold transition-colors',
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
