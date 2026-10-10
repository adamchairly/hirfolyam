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
          <path d="M10 4.5 11.6 8.9 16 10.5 11.6 12.1 10 16.5 8.4 12.1 4 10.5 8.4 8.9Z" />
          <path d="M17.5 14.5 18.3 16.7 20.5 17.5 18.3 18.3 17.5 20.5 16.7 18.3 14.5 17.5 16.7 16.7Z" />
        </svg>
      </button>
    </div>
  )
}
