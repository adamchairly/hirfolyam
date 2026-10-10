import type { ReactNode } from 'react'

import { useArticleLayout } from '@/hooks/use-article-layout'
import type { ArticleLayout } from '@/lib/article-layout'
import { cn } from '@/lib/utils'

const options: { value: ArticleLayout, label: string, icon: ReactNode }[] = [
  {
    value: 'list',
    label: 'Lista nézet',
    icon: (
      <>
        <rect x="4" y="5" width="6" height="5" rx="1" />
        <rect x="4" y="14" width="6" height="5" rx="1" />
        <path d="M14 6.5h6M14 9h4M14 15.5h6M14 18h4" />
      </>
    ),
  },
  {
    value: 'grid',
    label: 'Rács nézet',
    icon: (
      <>
        <rect x="4" y="4" width="6.5" height="6.5" rx="1" />
        <rect x="13.5" y="4" width="6.5" height="6.5" rx="1" />
        <rect x="4" y="13.5" width="6.5" height="6.5" rx="1" />
        <rect x="13.5" y="13.5" width="6.5" height="6.5" rx="1" />
      </>
    ),
  },
]

interface ArticleLayoutToggleProps {
  className?: string
}

export function ArticleLayoutToggle({ className }: ArticleLayoutToggleProps) {
  const { layout, updateLayout } = useArticleLayout()

  return (
    <div
      role="group"
      aria-label="Elrendezés"
      className={cn('inline-flex items-center gap-0.5 rounded-md border border-neutral-700 p-0.5', className)}
    >
      {options.map((option) => (
        <button
          key={option.value}
          type="button"
          aria-label={option.label}
          aria-pressed={layout === option.value}
          title={option.label}
          className="inline-flex size-7 items-center justify-center rounded-[4px] text-neutral-500 outline-none transition-colors hover:text-neutral-200 focus-visible:ring-2 focus-visible:ring-ring/50 aria-pressed:bg-neutral-700 aria-pressed:text-neutral-50"
          onClick={() => {
            updateLayout(option.value)
          }}
        >
          <svg
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth={1.75}
            strokeLinecap="round"
            strokeLinejoin="round"
            aria-hidden="true"
            className="size-4"
          >
            {option.icon}
          </svg>
        </button>
      ))}
    </div>
  )
}
