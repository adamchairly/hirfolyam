import { useEffect } from 'react'

import { SourceCard } from '@/components/source-card'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useSourceSelection } from '@/hooks/use-source-selection'
import { categories } from '@/lib/categories'
import { cn } from '@/lib/utils'

const nameCollator = new Intl.Collator('hu-HU')

const gridClassName = 'grid grid-cols-2 gap-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5'

export function SourcesPage() {
  const {
    sources,
    isLoading,
    error,
    retry,
    disabledSourceKeys,
    disabledCategorySlugs,
    toggleSource,
    toggleCategory,
  } = useSourceSelection()

  // The sections follow the order of the navigation.
  const sections = categories
    .map((category) => ({
      category,
      isEnabled: !disabledCategorySlugs.includes(category.slug),
      sources: sources
        .filter((source) => source.category === category.slug)
        .sort((first, second) => nameCollator.compare(first.name, second.name)),
    }))
    .filter((section) => section.sources.length > 0)

  useEffect(() => {
    document.title = 'Források | hírfolyam'
  }, [])

  return (
    <div className="space-y-6 pt-6 sm:pt-8">
      <ul aria-label="Kategóriák" className="flex flex-wrap justify-center gap-2">
        {categories.map((category) => {
          const isEnabled = !disabledCategorySlugs.includes(category.slug)

          return (
            <li key={category.slug}>
              <button
                type="button"
                aria-pressed={isEnabled}
                title={isEnabled ? 'Kategória elrejtése' : 'Kategória megjelenítése'}
                onClick={() => {
                  toggleCategory(category.slug)
                }}
                className={cn(
                  'inline-flex min-h-9 items-center rounded-md bg-neutral-100 px-4 text-sm font-semibold text-neutral-950 transition-[background-color,opacity]',
                  isEnabled ? 'hover:bg-neutral-300' : 'opacity-35 hover:opacity-60',
                )}
              >
                {category.name}
              </button>
            </li>
          )
        })}
      </ul>

      {isLoading && (
        <div aria-busy="true">
          <p role="status" className="sr-only">Források betöltése…</p>
          <div aria-hidden="true" className={gridClassName}>
            {Array.from({ length: 8 }, (_, index) => (
              <div key={index} className="flex min-h-14 w-full items-center gap-3 rounded-md border border-neutral-700/70 bg-neutral-900 px-3 py-2">
                <Skeleton className="size-8 shrink-0" />
                <div className="space-y-1.5">
                  <Skeleton className="h-3 w-20" />
                  <Skeleton className="h-3 w-12" />
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {!isLoading && error && (
        <div className="space-y-4 py-12 text-center">
          <p role="alert" className="text-neutral-300">{error}</p>
          <Button variant="outline" onClick={retry}>Újrapróbálom</Button>
        </div>
      )}

      {!isLoading && !error && sources.length === 0 && (
        <p role="status" className="py-16 text-center text-neutral-400">
          Még nincs megjeleníthető hírforrás.
        </p>
      )}

      {!isLoading && !error && sections.map(({ category, isEnabled, sources: sectionSources }) => (
        <section key={category.slug} aria-labelledby={`sources-${category.slug}`}>
          <h2
            id={`sources-${category.slug}`}
            className={cn(
              'mb-3 text-sm font-semibold transition-colors',
              isEnabled ? 'text-neutral-300' : 'text-neutral-600',
            )}
          >
            {category.name}
          </h2>

          <ul className={gridClassName}>
            {sectionSources.map((source) => (
              <li key={source.key}>
                <SourceCard
                  source={source}
                  isEnabled={!disabledSourceKeys.includes(source.key)}
                  onToggle={() => {
                    toggleSource(source.key)
                  }}
                />
              </li>
            ))}
          </ul>
        </section>
      ))}
    </div>
  )
}
