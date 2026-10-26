import { Popover } from 'radix-ui'
import { useCallback, useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'

import { useHotTopics } from '@/hooks/use-hot-topics'
import { useHotTopicsVisibility } from '@/hooks/use-hot-topics-visibility'
import { useSourceSelection } from '@/hooks/use-source-selection'
import { cn } from '@/lib/utils'
import type { HotTopic, HotTopicArticle, HotTopicPeriod } from '@/types/hot-topic'

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
  const [period, setPeriod] = useState<HotTopicPeriod>('hour')
  const { topics, isLoading } = useHotTopics(period)
  const { disabledSourceKeys } = useSourceSelection()
  const visibleTopics = topics
    .map((topic) => ({
      topic,
      articles: getSourceArticles(topic, disabledSourceKeys),
    }))
    .filter(({ articles }) => articles.length > 0)
  const emptyMessage = topics.length > 0
    ? 'Az összefoglaló témái kikapcsolt forrásokból származnak.'
    : periods.find((option) => option.value === period)?.emptyMessage

  return (
    <section aria-label="AI-összefoglaló">
      <HotTopicPeriodSwitch period={period} onChange={setPeriod} />

      {/* The previous topics stay dimmed until the selected period arrives. */}
      <div aria-busy={isLoading} className={cn('mt-4 transition-opacity', isLoading && 'opacity-50')}>
        {visibleTopics.length > 0 ? (
          <ul className="space-y-5 sm:space-y-6">
            {visibleTopics.map(({ topic, articles }) => (
              <HotTopicItem key={topic.title} topic={topic} articles={articles} />
            ))}
          </ul>
        ) : !isLoading && (
          <p role="status" className="text-sm text-neutral-400">
            {emptyMessage}
          </p>
        )}
      </div>
    </section>
  )
}

const periods: { value: HotTopicPeriod, label: string, emptyMessage: string }[] = [
  {
    value: 'hour',
    label: 'Elmúlt óra',
    emptyMessage: 'Az elmúlt órából nincs összefoglaló.',
  },
  {
    value: 'today',
    label: 'Ma',
    emptyMessage: 'Ma még nem készült összefoglaló.',
  },
  {
    value: 'yesterday',
    label: 'Tegnap',
    emptyMessage: 'Tegnapról nincs összefoglaló.',
  },
]

interface HotTopicPeriodSwitchProps {
  period: HotTopicPeriod
  onChange: (period: HotTopicPeriod) => void
}

function HotTopicPeriodSwitch({ period, onChange }: HotTopicPeriodSwitchProps) {
  return (
    <div
      role="group"
      aria-label="Összefoglaló időszaka"
      className="inline-flex items-center gap-0.5 rounded-md bg-neutral-900/35 p-0.5"
    >
      {periods.map((option) => (
        <button
          key={option.value}
          type="button"
          aria-pressed={period === option.value}
          className="inline-flex h-7 items-center rounded-[4px] px-2.5 text-xs font-medium text-neutral-500 outline-none transition-colors hover:text-neutral-200 focus-visible:ring-2 focus-visible:ring-ring/50 aria-pressed:bg-neutral-900 aria-pressed:text-neutral-50"
          onClick={() => {
            onChange(option.value)
          }}
        >
          {option.label}
        </button>
      ))}
    </div>
  )
}

interface HotTopicItemProps {
  topic: HotTopic
  articles: HotTopicArticle[]
}

function HotTopicItem({ topic, articles }: HotTopicItemProps) {
  return (
    // The item has the title's font size and line height, so the source button lines up with the title.
    <li className="text-base leading-snug sm:text-lg">
      {/* The margin stays on the title's line when the button wraps to the next one. */}
      <h3 className="mr-2 inline font-bold tracking-tight text-neutral-50">
        <Link
          to={`/topics/${topic.id}`}
          className="underline-offset-4 transition-colors hover:text-white hover:underline"
        >
          {topic.title}
        </Link>
      </h3>

      <TopicSources articles={articles} />

      <p className="mt-1.5 text-sm leading-6 text-neutral-300">
        {topic.summary}
      </p>
    </li>
  )
}

const stackedIconCount = 3

function TopicSources({ articles }: { articles: HotTopicArticle[] }) {
  const [isOpen, setIsOpen] = useState(false)
  const triggerRef = useRef<HTMLButtonElement>(null)
  const contentRef = useRef<HTMLDivElement>(null)
  const isClosedByScrollRef = useRef(false)

  // Following the button through a page scroll makes the popover jerk, so scrolling anything else closes it.
  useEffect(() => {
    if (!isOpen) {
      return
    }

    function handleOutsideScroll(event: Event) {
      if (event.target instanceof Node && !contentRef.current?.contains(event.target)) {
        isClosedByScrollRef.current = true
        setIsOpen(false)
      }
    }

    // Scroll events do not bubble, so the capture phase sees the page and every inner scroll area.
    window.addEventListener('scroll', handleOutsideScroll, { capture: true, passive: true })

    return () => {
      window.removeEventListener('scroll', handleOutsideScroll, { capture: true })
    }
  }, [isOpen])

  // On closing, Radix focuses the button and so scrolls the page back to it; after a scroll the page stays put.
  function handleCloseAutoFocus(event: Event) {
    if (isClosedByScrollRef.current) {
      isClosedByScrollRef.current = false
      event.preventDefault()
      triggerRef.current?.focus({ preventScroll: true })
    }
  }

  return (
    <Popover.Root open={isOpen} onOpenChange={setIsOpen}>
      <Popover.Trigger
        ref={triggerRef}
        className="group inline-flex h-6 items-center gap-1.5 rounded-full bg-neutral-900 pr-2.5 pl-[3px] align-middle text-xs font-medium whitespace-nowrap text-neutral-300 transition-colors hover:bg-neutral-950 hover:text-neutral-50 data-[state=open]:bg-neutral-950 data-[state=open]:text-neutral-50"
      >
        <span className="flex -space-x-1">
          {articles.slice(0, stackedIconCount).map((article) => (
            // The ring takes the button's colour, so the overlapping icons look cut out.
            <SourceIcon
              key={article.id}
              source={article.source}
              className="size-4.5 ring-2 ring-neutral-900 transition-shadow group-hover:ring-neutral-950 group-data-[state=open]:ring-neutral-950"
            />
          ))}
        </span>

        <span className="tabular-nums">
          {articles.length} forrás
        </span>
      </Popover.Trigger>

      <Popover.Portal>
        <Popover.Content
          ref={contentRef}
          align="start"
          sideOffset={8}
          collisionPadding={16}
          aria-label="Források"
          onCloseAutoFocus={handleCloseAutoFocus}
          className="z-50 w-88 max-w-[calc(100vw-2rem)] origin-(--radix-popover-content-transform-origin) overflow-hidden rounded-lg bg-neutral-900 shadow-xl shadow-black/40 duration-100 outline-none data-[side=bottom]:slide-in-from-top-2 data-[side=top]:slide-in-from-bottom-2 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95"
        >
          <SourceList articles={articles} />
        </Popover.Content>
      </Popover.Portal>
    </Popover.Root>
  )
}

// The list takes at most the free space and scrolls beyond it; its fading bottom shows that more sources follow.
function SourceList({ articles }: { articles: HotTopicArticle[] }) {
  const [hasMoreBelow, setHasMoreBelow] = useState(false)

  // The free space is known only once the popover is placed, and it changes with the viewport.
  const observeList = useCallback((list: HTMLDivElement | null) => {
    if (!list) {
      return
    }

    const observer = new ResizeObserver(() => {
      setHasMoreBelow(canScrollDown(list))
    })

    observer.observe(list)

    return () => {
      observer.disconnect()
    }
  }, [])

  return (
    <div
      ref={observeList}
      className={cn(
        // The scroll padding keeps a row reached with the keyboard above the fade.
        'max-h-[min(36rem,var(--radix-popover-content-available-height))] scroll-pb-10 overflow-y-auto overscroll-contain p-1 [scrollbar-width:thin]',
        hasMoreBelow && '[mask-image:linear-gradient(to_bottom,#000_calc(100%_-_2.5rem),transparent)]',
      )}
      onScroll={(event) => {
        setHasMoreBelow(canScrollDown(event.currentTarget))
      }}
    >
      <ul>
        {articles.map((article) => (
          <li key={article.id}>
            <a
              href={article.url}
              target="_blank"
              rel="noopener noreferrer"
              aria-label={`${article.source.name}: ${article.title} (új lapon nyílik meg)`}
              className="group flex items-start gap-3 rounded-md px-2.5 py-2 transition-colors hover:bg-neutral-800 focus-visible:bg-neutral-800 focus-visible:-outline-offset-2"
            >
              <SourceIcon source={article.source} className="size-5" />

              <span className="min-w-0 flex-1">
                <span className="block text-xs leading-4 font-semibold text-neutral-100">
                  {article.source.name}
                </span>
                <span className="mt-0.5 line-clamp-2 text-xs leading-4 text-neutral-400 transition-colors group-hover:text-neutral-300">
                  {article.title}
                </span>
              </span>
            </a>
          </li>
        ))}
      </ul>
    </div>
  )
}

function canScrollDown(element: HTMLElement) {
  return element.scrollHeight - element.scrollTop - element.clientHeight > 1
}

interface SourceIconProps {
  source: HotTopicArticle['source']
  className?: string
}

// A light disc keeps the dark, transparent logos visible and the mixed shapes uniform.
function SourceIcon({ source, className }: SourceIconProps) {
  const [failedIconUrl, setFailedIconUrl] = useState<string | null>(null)
  const icon = source.iconUrl

  return (
    <span className={cn('flex shrink-0 items-center justify-center overflow-hidden rounded-full bg-neutral-50', className)}>
      {icon && icon !== failedIconUrl ? (
        <img
          src={icon}
          alt=""
          width={20}
          height={20}
          loading="lazy"
          decoding="async"
          referrerPolicy="no-referrer"
          className="size-full object-cover"
          onError={() => {
            setFailedIconUrl(icon)
          }}
        />
      ) : (
        <span aria-hidden="true" className="text-[9px] leading-none font-bold text-neutral-900">
          {source.name.slice(0, 1).toLocaleUpperCase('hu-HU')}
        </span>
      )}
    </span>
  )
}
