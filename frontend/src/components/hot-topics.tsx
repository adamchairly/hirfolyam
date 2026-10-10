import { useId, useRef, useState } from 'react'
import { Popover } from 'radix-ui'

import { useHotTopics } from '@/hooks/use-hot-topics'
import { useHotTopicsVisibility } from '@/hooks/use-hot-topics-visibility'
import { useSourceSelection } from '@/hooks/use-source-selection'
import { cn } from '@/lib/utils'
import type { HotTopic, HotTopicArticle } from '@/types/hot-topic'

const timeFormatter = new Intl.DateTimeFormat('hu-HU', {
  timeZone: 'Europe/Budapest',
  hour: '2-digit',
  minute: '2-digit',
  hourCycle: 'h23',
})

// One link per outlet: the newest of its articles about the topic.
function getSourceArticles(topic: HotTopic, disabledSourceKeys: string[]): HotTopicArticle[] {
  const articles = new Map<string, HotTopicArticle>()

  for (const article of topic.articles) {
    if (!disabledSourceKeys.includes(article.source.key) && !articles.has(article.source.key)) {
      articles.set(article.source.key, article)
    }
  }

  return [...articles.values()]
}

// Hidden summaries are not loaded at all.
export function HotTopics() {
  const { isVisible } = useHotTopicsVisibility()

  return isVisible ? <HotTopicList /> : null
}

function HotTopicList() {
  const { topics, generatedAt } = useHotTopics()
  const { disabledSourceKeys } = useSourceSelection()
  const visibleTopics = topics
    .map((topic) => ({
      topic,
      articles: getSourceArticles(topic, disabledSourceKeys),
    }))
    .filter(({ articles }) => articles.length > 0)

  if (visibleTopics.length === 0) {
    return null
  }

  const updatedAt = generatedAt ? new Date(generatedAt) : null

  return (
    <section aria-label="AI-összefoglaló">
      <p className="flex items-center justify-end gap-1.5 text-[10px] leading-4 text-neutral-400 sm:text-[11px]">
        {updatedAt && !Number.isNaN(updatedAt.getTime()) && (
          <time dateTime={updatedAt.toISOString()} className="tabular-nums text-neutral-300">
            {timeFormatter.format(updatedAt)}
          </time>
        )}
        <AiNotice />
      </p>

      <ul className="mt-4 space-y-5 sm:space-y-6">
        {visibleTopics.map(({ topic, articles }) => (
          <HotTopicItem key={topic.title} topic={topic} articles={articles} />
        ))}
      </ul>
    </section>
  )
}

// Opens on hover for a mouse, and on a tap or a key press for everyone else.
function AiNotice() {
  const [isOpen, setIsOpen] = useState(false)
  const openedByHover = useRef(false)

  return (
    <Popover.Root open={isOpen} onOpenChange={setIsOpen}>
      <Popover.Trigger asChild>
        <button
          type="button"
          aria-label="Mit jelent az AI-összefoglaló?"
          className="inline-flex size-4 items-center justify-center rounded-full border border-neutral-600 text-[10px] leading-none font-bold text-neutral-400 outline-none transition-colors hover:border-neutral-400 hover:text-neutral-100 focus-visible:ring-2 focus-visible:ring-ring/50"
          onPointerEnter={(event) => {
            if (event.pointerType === 'mouse') {
              openedByHover.current = true
              setIsOpen(true)
            }
          }}
          onPointerLeave={(event) => {
            if (event.pointerType === 'mouse' && openedByHover.current) {
              openedByHover.current = false
              setIsOpen(false)
            }
          }}
          onClick={(event) => {
            // A click on a hovered button would close the note it just opened.
            if (openedByHover.current) {
              event.preventDefault()
            }
          }}
        >
          ?
        </button>
      </Popover.Trigger>

      <Popover.Portal>
        <Popover.Content
          side="bottom"
          align="end"
          sideOffset={6}
          collisionPadding={12}
          className="z-50 max-w-72 rounded-md border border-neutral-700 bg-neutral-900 px-3 py-2.5 text-xs leading-5 text-neutral-200 shadow-lg"
          onOpenAutoFocus={(event) => {
            event.preventDefault()
          }}
        >
          AI által generált tartalom. A témákat mesterséges intelligencia válogatja és foglalja össze óránként
          a legfrissebb cikkekből, ezért pontatlan vagy hiányos lehet.
        </Popover.Content>
      </Popover.Portal>
    </Popover.Root>
  )
}

interface HotTopicItemProps {
  topic: HotTopic
  articles: HotTopicArticle[]
}

function HotTopicItem({ topic, articles }: HotTopicItemProps) {
  const [isExpanded, setIsExpanded] = useState(false)
  const detailsId = useId()

  return (
    <li>
      <h3 className="text-base leading-snug font-bold tracking-tight text-neutral-50 sm:inline sm:text-lg">
        {/* On small screens only the titles show until a topic is opened. */}
        <button
          type="button"
          aria-expanded={isExpanded}
          aria-controls={detailsId}
          className="flex w-full items-start justify-between gap-3 text-left sm:hidden"
          onClick={() => {
            setIsExpanded((previous) => !previous)
          }}
        >
          <span>{topic.title}</span>
          <svg
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth={2}
            strokeLinecap="round"
            strokeLinejoin="round"
            aria-hidden="true"
            className={cn('mt-1 size-4 shrink-0 text-neutral-400 transition-transform', isExpanded && 'rotate-180')}
          >
            <path d="m6 9 6 6 6-6" />
          </svg>
        </button>
        <span className="max-sm:hidden">{topic.title}</span>
      </h3>

      {/* The sources follow the title on wide screens and the summary on small ones. */}
      <SourceIcons articles={articles} className="max-sm:hidden" />

      <div id={detailsId} className={cn(!isExpanded && 'max-sm:hidden')}>
        <p className="mt-1.5 text-sm leading-6 text-neutral-300">
          {topic.summary}
          <SourceIcons articles={articles} className="sm:hidden" />
        </p>
      </div>
    </li>
  )
}

function SourceIcons({ articles, className }: { articles: HotTopicArticle[], className?: string }) {
  return (
    <span
      role="group"
      aria-label="Források"
      className={cn('ml-2 inline-flex flex-wrap items-center gap-1.5 align-middle', className)}
    >
      {articles.map((article) => (
        <SourceLink key={article.id} article={article} />
      ))}
    </span>
  )
}

function SourceLink({ article }: { article: HotTopicArticle }) {
  const [iconFailed, setIconFailed] = useState(false)
  const icon = article.source.iconUrl

  return (
    <a
      href={article.url}
      target="_blank"
      rel="noopener noreferrer"
      title={article.title}
      aria-label={`${article.source.name}: ${article.title} (új lapon nyílik meg)`}
      className="inline-flex items-center rounded-sm opacity-80 transition-opacity hover:opacity-100"
    >
      {icon && !iconFailed ? (
        <img
          src={icon}
          alt=""
          width={16}
          height={16}
          loading="lazy"
          decoding="async"
          className="size-4 shrink-0 rounded-sm object-contain"
          onError={() => {
            setIconFailed(true)
          }}
        />
      ) : (
        // Without an icon the name stands in, so the source can still be opened.
        <span className="text-xs text-neutral-400 underline decoration-neutral-600 underline-offset-2">
          {article.source.name}
        </span>
      )}
    </a>
  )
}
