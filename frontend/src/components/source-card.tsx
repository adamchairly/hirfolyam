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
        'group flex aspect-square w-full flex-col items-center justify-center gap-2 rounded-md border p-2 text-center transition-colors hover:border-neutral-500 hover:bg-neutral-900/70',
        isEnabled
          ? 'border-neutral-700/70 bg-neutral-900'
          : 'border-dashed border-neutral-600 bg-neutral-800',
      )}
    >
      <span className={cn(
        'flex aspect-square w-[44%] max-w-20 shrink-0 items-center justify-center overflow-hidden',
        !isEnabled && 'opacity-40 grayscale',
      )}>
        {icon && icon !== failedIconUrl ? (
          <img
            src={icon}
            alt=""
            width={80}
            height={80}
            className="aspect-square size-full rounded-sm object-contain"
            onError={() => {
              setFailedIconUrl(icon)
            }}
          />
        ) : (
          <span aria-hidden="true" className="text-2xl font-bold text-neutral-300">
            {source.name.slice(0, 2).toLocaleUpperCase('hu-HU')}
          </span>
        )}
      </span>

      <span className="text-xs font-bold uppercase tracking-wide text-neutral-100">
        {source.name}
      </span>

      <span className="text-xs font-semibold uppercase tabular-nums text-neutral-300 transition-colors group-hover:text-neutral-50">
        {isEnabled ? `${countFormatter.format(source.articleCount)} cikk` : 'Kikapcsolva'}
      </span>
    </button>
  )
}
