import { useEffect, type ReactNode } from 'react'
import { useSearchParams } from 'react-router-dom'

import { ArticleLayoutToggle } from '@/components/article-layout-toggle'
import { ArticleList } from '@/components/article-list'
import { ArticleListSkeleton } from '@/components/article-list-skeleton'
import { DisabledSourcesMessage } from '@/components/disabled-sources-message'
import { InfiniteScroll } from '@/components/infinite-scroll'
import { Button } from '@/components/ui/button'
import { useArticles } from '@/hooks/use-articles'
import { useDebouncedValue } from '@/hooks/use-debounced-value'
import { maximumSearchLength, searchDelay } from '@/lib/search'

const countFormatter = new Intl.NumberFormat('hu-HU')

function SearchToolbar({ children }: { children: ReactNode }) {
  return (
    <div className="flex items-center justify-between gap-4 pt-6">
      <p role="status" className="text-sm text-neutral-400">
        {children}
      </p>

      <ArticleLayoutToggle />
    </div>
  )
}

interface SearchResultsProps {
  search: string
}

function SearchResults({ search }: SearchResultsProps) {
  const {
    articles,
    isLoading,
    hasMore,
    hasEnabledSources,
    totalItems,
    error,
    loadMore,
    retry,
  } = useArticles(undefined, search)

  return (
    <section aria-label="Keresési találatok">
      <SearchToolbar>
        {isLoading
          ? 'Találatok betöltése…'
          : !error && `${countFormatter.format(totalItems)} találat erre: „${search}”`}
      </SearchToolbar>

      {isLoading && articles.length === 0 && <ArticleListSkeleton />}

      <ArticleList
        articles={articles}
        label="Találati lista"
        isLoading={isLoading}
        search={search}
      />

      {!isLoading && !error && articles.length === 0 && (
        hasEnabledSources ? (
          <div className="space-y-2 py-16 text-center">
            <h2 className="text-xl font-semibold">Nincs találat.</h2>
            <p className="text-neutral-400">
              Próbálj másik szót vagy rövidebb kifejezést.
            </p>
          </div>
        ) : (
          <DisabledSourcesMessage />
        )
      )}

      {error && (
        <div className="space-y-4 py-12 text-center">
          <p role="alert" className="text-neutral-300">{error}</p>
          <Button variant="outline" onClick={retry}>
            Újrapróbálom
          </Button>
        </div>
      )}

      {articles.length > 0 && !error && (
        <InfiniteScroll
          hasMore={hasMore}
          isLoading={isLoading}
          onLoadMore={loadMore}
          loadingLabel="További találatok betöltése…"
        />
      )}
    </section>
  )
}

export function SearchPage() {
  const [searchParams] = useSearchParams()
  const search = searchParams.get('q') ?? ''
  const query = search.trim()
  const debouncedQuery = useDebouncedValue(query, searchDelay, '')
  const isPending = query !== debouncedQuery
  const isTooLong = search.length > maximumSearchLength

  useEffect(() => {
    document.title = 'Keresési találatok | hírfolyam'
  }, [])

  return (
    <>
      <h1 className="sr-only">
        Keresési találatok
      </h1>

      {isTooLong ? (
        <p role="alert" className="py-12 text-center text-neutral-300">
          Legfeljebb {maximumSearchLength} karakterre kereshetsz.
        </p>
      ) : !query ? (
        <p role="status" className="py-16 text-center text-neutral-400">
          Írj be egy szót vagy kifejezést a kereséshez.
        </p>
      ) : isPending ? (
        <div>
          <SearchToolbar>Keresés…</SearchToolbar>
          <ArticleListSkeleton />
        </div>
      ) : (
        <SearchResults
          key={debouncedQuery}
          search={debouncedQuery}
        />
      )}
    </>
  )
}
