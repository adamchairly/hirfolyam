import { useState } from 'react'

import { BookmarkIcon } from '@/components/bookmark-icon'
import { HighlightedText } from '@/components/highlighted-text'
import { useArticleLeadClamp } from '@/hooks/use-article-lead-clamp'
import { useBookmarks } from '@/hooks/use-bookmarks'
import { useGridLeadClamp } from '@/hooks/use-grid-lead-clamp'
import type { ArticleLayout } from '@/lib/article-layout'
import { formatPublishedAt } from '@/lib/date'
import { toPlainText } from '@/lib/text'
import { cn } from '@/lib/utils'
import type { Article } from '@/types/article'

interface ArticleItemProps {
  article: Article
  now: Date
  search?: string
  layout?: ArticleLayout
  showLead?: boolean
}

export function ArticleItem({ article, now, search = '', layout = 'list', showLead = true }: ArticleItemProps) {
  const [imageFailed, setImageFailed] = useState(false)
  const [failedIconUrl, setFailedIconUrl] = useState<string | null>(null)
  const lead = showLead ? toPlainText(article.lead) : ''
  const icon = article.source.iconUrl
  const imageUrl = article.imageUrl?.trim()
  const hasImage = Boolean(imageUrl) && !imageFailed
  const isSearchResult = search.trim().length > 0
  const isGrid = layout === 'grid'
  const { imageRef, titleRef, metadataRef, leadRef, leadLines: listLeadLines } = useArticleLeadClamp(
    hasImage && !isSearchResult && !isGrid,
    lead,
  )
  const { articleRef, leadLines: gridLeadLines } = useGridLeadClamp(isGrid && !hasImage, leadRef)
  const leadLines = isGrid ? gridLeadLines : listLeadLines
  const { bookmarks, toggleBookmark } = useBookmarks()
  const isSaved = bookmarks.includes(article.id)

  return (
    <article
      ref={articleRef}
      className={isGrid
        ? 'flex h-full flex-col gap-3 sm:gap-4'
        : 'flex flex-col gap-5 py-7 sm:flex-row sm:gap-7 sm:py-8'}
    >
      {hasImage && (
        <div
          ref={imageRef}
          className={cn(
            'relative aspect-[3/2] w-full shrink-0 self-start overflow-hidden bg-neutral-900',
            !isGrid && 'sm:w-64 md:w-80',
          )}
        >
          <img
            src={imageUrl}
            alt=""
            width={480}
            height={320}
            loading="lazy"
            decoding="async"
            referrerPolicy="no-referrer"
            className="absolute inset-0 h-full w-full object-cover object-center"
            onError={() => {
              setImageFailed(true)
            }}
          />
        </div>
      )}

      <div className={cn('min-w-0 flex-1', isGrid && 'flex flex-col')}>
        <h2
          ref={titleRef}
          className={isGrid
            ? 'text-base leading-snug font-bold tracking-tight text-neutral-50 sm:text-lg'
            : 'text-xl leading-snug font-bold tracking-tight text-neutral-50 sm:text-2xl lg:text-[1.75rem]'}
        >
          <a
            href={article.url}
            target="_blank"
            rel="noopener noreferrer"
            className="hover:underline decoration-neutral-500 underline-offset-4"
          >
            <HighlightedText text={article.title} search={search} />
          </a>
        </h2>

        {lead && (
          <p
            ref={leadRef}
            className={isGrid
              ? 'mt-2 line-clamp-3 text-sm leading-6 text-neutral-300'
              : 'mt-3 text-base leading-7 text-neutral-300'}
            style={leadLines === null ? undefined : {
              display: leadLines === 0 ? 'none' : '-webkit-box',
              WebkitBoxOrient: 'vertical',
              WebkitLineClamp: leadLines || undefined,
              overflow: 'hidden',
            }}
          >
            <HighlightedText text={lead} search={search} />
          </p>
        )}

        <p
          ref={metadataRef}
          className={cn(
            'flex flex-wrap items-center gap-x-3 gap-y-2 text-xs leading-5 text-neutral-400 sm:gap-x-4',
            isGrid ? 'mt-auto pt-3' : 'mt-3',
          )}
        >
          <span className="inline-flex items-center gap-2">
            <button
              type="button"
              aria-label={isSaved ? 'Eltávolítás a mentett hírek közül' : 'Mentés a mentett hírek közé'}
              aria-pressed={isSaved}
              title={isSaved ? 'Mentve' : 'Mentés'}
              className="-my-1 -ml-1.5 -mr-1 inline-flex size-7 shrink-0 items-center justify-center rounded-md text-neutral-400 transition-colors hover:text-neutral-100 aria-pressed:text-neutral-50"
              onClick={() => {
                toggleBookmark(article.id)
              }}
            >
              <BookmarkIcon filled={isSaved} className="size-4" />
            </button>
            {icon && icon !== failedIconUrl && (
              <img
                src={icon}
                alt={article.source.name}
                width={16}
                height={16}
                loading="lazy"
                decoding="async"
                referrerPolicy="no-referrer"
                className="size-4 shrink-0 rounded-sm object-contain"
                onError={() => {
                  setFailedIconUrl(icon)
                }}
              />
            )}
            <time dateTime={article.publishedAt}>
              {formatPublishedAt(article.publishedAt, now)}
            </time>
          </span>
        </p>
      </div>
    </article>
  )
}
