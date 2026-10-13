import { LayoutGrid, LayoutList, type LucideIcon } from 'lucide-react'

import { useArticleLayout } from '@/hooks/use-article-layout'
import type { ArticleLayout } from '@/lib/article-layout'
import { cn } from '@/lib/utils'

const options: { value: ArticleLayout, label: string, icon: LucideIcon }[] = [
  {
    value: 'list',
    label: 'Lista nézet',
    icon: LayoutList,
  },
  {
    value: 'grid',
    label: 'Rács nézet',
    icon: LayoutGrid,
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
      className={cn('inline-flex items-center gap-0.5 rounded-md bg-neutral-900/35 p-0.5', className)}
    >
      {options.map((option) => (
        <button
          key={option.value}
          type="button"
          aria-label={option.label}
          aria-pressed={layout === option.value}
          title={option.label}
          className="inline-flex size-7 items-center justify-center rounded-[4px] text-neutral-500 outline-none transition-colors hover:text-neutral-200 focus-visible:ring-2 focus-visible:ring-ring/50 aria-pressed:bg-neutral-900 aria-pressed:text-neutral-50"
          onClick={() => {
            updateLayout(option.value)
          }}
        >
          <option.icon strokeWidth={1.75} className="size-4" />
        </button>
      ))}
    </div>
  )
}
