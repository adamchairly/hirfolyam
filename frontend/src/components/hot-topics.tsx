import { useState } from 'react'
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
    <li>
      <h3 className="text-base leading-snug font-bold tracking-tight text-neutral-50 sm:inline sm:text-lg">
        <Link
          to={`/topics/${topic.id}`}
          className="underline-offset-4 transition-colors hover:text-white hover:underline"
        >
          {topic.title}
        </Link>
      </h3>

      {/* The sources follow the title on wide screens and the summary on small ones. */}
      <SourceIcons articles={articles} className="max-sm:hidden" />

      <p className="mt-1.5 text-sm leading-6 text-neutral-300">
        {topic.summary}
        <SourceIcons articles={articles} className="sm:hidden" />
      </p>
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
          referrerPolicy="no-referrer"
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
