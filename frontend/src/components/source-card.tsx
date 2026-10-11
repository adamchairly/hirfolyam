import { useState } from 'react'

import { cn } from '@/lib/utils'
import type { Source } from '@/types/source'

const countFormatter = new Intl.NumberFormat('hu-HU')

interface SourceCardProps {
  source: Source
  isEnabled: boolean
  onToggle: () => void
}

export function SourceCard({ source, isEnabled, onToggle }: SourceCardProps) {
  const [failedIconUrl, setFailedIconUrl] = useState<string | null>(null)
  const icon = source.iconUrl

  return (
    <button
      type="button"
      aria-label={source.name}
      aria-pressed={isEnabled}
      title={isEnabled ? 'Forrás kikapcsolása' : 'Forrás bekapcsolása'}
      onClick={onToggle}
      className={cn(
        'group flex h-full min-h-14 w-full items-center gap-3 rounded-md border border-neutral-700/70 bg-neutral-900 px-3 py-2 text-left transition-[border-color,background-color,opacity] hover:border-neutral-500',
        !isEnabled && 'opacity-35 hover:opacity-60',
      )}
    >
      <span className={cn(
        'flex size-8 shrink-0 items-center justify-center overflow-hidden',
        !isEnabled && 'grayscale',
      )}>
        {icon && icon !== failedIconUrl ? (
          <img
            src={icon}
            alt=""
            width={32}
            height={32}
            referrerPolicy="no-referrer"
            className="size-full rounded-sm object-contain"
            onError={() => {
              setFailedIconUrl(icon)
            }}
          />
        ) : (
          <span aria-hidden="true" className="text-sm font-bold text-neutral-300">
            {source.name.slice(0, 2).toLocaleUpperCase('hu-HU')}
          </span>
        )}
      </span>

      <span className="flex min-w-0 flex-col gap-0.5">
        <span className="line-clamp-2 text-sm font-semibold leading-5 text-neutral-100">
          {source.name}
        </span>

        <span className="text-xs leading-4 tabular-nums text-neutral-400 transition-colors group-hover:text-neutral-200">
          {countFormatter.format(source.articleCount)} cikk
        </span>
      </span>
    </button>
  )
}
