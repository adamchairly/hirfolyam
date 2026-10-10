import { useEffect } from 'react'

import { SourceCard } from '@/components/source-card'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useSourceSelection } from '@/hooks/use-source-selection'

const gridClassName = 'grid grid-cols-2 gap-3 min-[480px]:grid-cols-3 sm:grid-cols-4 md:grid-cols-5 lg:grid-cols-6'

export function SourcesPage() {
  const {
    sources,
    isLoading,
    error,
    retry,
    disabledSourceKeys,
    toggleSource,
  } = useSourceSelection()

  useEffect(() => {
    document.title = 'Források | hírfolyam'
  }, [])

  return (
    <section aria-label="Hírportálok" aria-busy={isLoading} className="pt-6 sm:pt-8">
      {isLoading && (
        <>
          <p role="status" className="sr-only">Források betöltése…</p>
          <div aria-hidden="true" className={gridClassName}>
            {Array.from({ length: 8 }, (_, index) => (
              <div key={index} className="flex aspect-square w-full flex-col items-center justify-center gap-2 rounded-md border border-neutral-700/70 bg-neutral-900 p-2">
                <Skeleton className="aspect-square w-[44%] max-w-20 shrink-0" />
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-4 w-16" />
              </div>
            ))}
          </div>
        </>
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

      {!isLoading && !error && sources.length > 0 && (
        <ul className={gridClassName}>
          {sources.map((source) => (
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
      )}
    </section>
  )
}
