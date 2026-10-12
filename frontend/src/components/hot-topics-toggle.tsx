import { Sparkles } from 'lucide-react'

import { useHotTopicsVisibility } from '@/hooks/use-hot-topics-visibility'
import { cn } from '@/lib/utils'

interface HotTopicsToggleProps {
  className?: string
}

export function HotTopicsToggle({ className }: HotTopicsToggleProps) {
  const { isVisible, updateVisibility } = useHotTopicsVisibility()
  const label = isVisible ? 'AI-összefoglaló elrejtése' : 'AI-összefoglaló megjelenítése'

  return (
    <div className={cn('inline-flex items-center rounded-md border border-neutral-700 p-0.5', className)}>
      <button
        type="button"
        aria-label="AI-összefoglaló"
        aria-pressed={isVisible}
        title={label}
        className="inline-flex size-7 items-center justify-center rounded-[4px] text-neutral-500 outline-none transition-colors hover:text-neutral-200 focus-visible:ring-2 focus-visible:ring-ring/50 aria-pressed:bg-neutral-700 aria-pressed:text-neutral-50"
        onClick={() => {
          updateVisibility(!isVisible)
        }}
      >
        <Sparkles strokeWidth={1.75} className="size-4" />
      </button>
    </div>
  )
}
