import { Skeleton } from '@/components/ui/skeleton'
import { useArticleLayout } from '@/hooks/use-article-layout'

export function ArticleListSkeleton() {
  const { layout } = useArticleLayout()

  return (
    <div role="status">
      <span className="sr-only">Cikkek betöltése…</span>

      {layout === 'grid' ? (
        <div aria-hidden="true" className="grid grid-cols-2 gap-x-4 gap-y-8 py-8 sm:gap-x-6 sm:gap-y-10 lg:grid-cols-3">
          {[0, 1, 2, 3, 4, 5].map((index) => (
            <div key={index} className="space-y-3">
              <Skeleton className="aspect-[3/2] w-full rounded-none" />
              <Skeleton className="mt-4 h-5 w-full" />
              <Skeleton className="h-5 w-3/4" />
              <Skeleton className="mt-4 h-3 w-24" />
            </div>
          ))}
        </div>
      ) : (
        <div aria-hidden="true">
          {[0, 1, 2].map((index) => (
            <div key={index} className="flex flex-col gap-5 py-7 sm:flex-row sm:gap-7 sm:py-8">
              <Skeleton className="aspect-[3/2] w-full shrink-0 self-start rounded-none sm:w-64 md:w-80" />
              <div className="flex-1 space-y-3">
                <Skeleton className="h-7 w-full" />
                <Skeleton className="h-7 w-4/5" />
                <Skeleton className="mt-5 h-4 w-full" />
                <Skeleton className="h-4 w-5/6" />
                <Skeleton className="mt-5 h-3 w-28" />
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
