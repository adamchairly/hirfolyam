import type { ReactNode } from 'react'

import { LastUpdated } from '@/components/last-updated'
import { useCurrentTime } from '@/hooks/use-current-time'

interface PageHeaderProps {
  title: string
  lastUpdatedAt: string | null
  action?: ReactNode
}

export function PageHeader({ title, lastUpdatedAt, action }: PageHeaderProps) {
  const now = useCurrentTime()

  return (
    <header className="space-y-2 py-9 text-center sm:py-12">
      {/* The empty side columns keep the title centred, and give way to it instead of letting the action cover it. */}
      <div className="grid grid-cols-[1fr_auto_1fr] items-center gap-x-3">
        <h1 className="col-start-2 text-3xl font-bold tracking-tight sm:text-4xl">
          {title}
        </h1>

        {action && (
          <div className="justify-self-end">
            {action}
          </div>
        )}
      </div>

      <LastUpdated value={lastUpdatedAt} now={now} />
    </header>
  )
}
