import { Rss } from 'lucide-react'
import { FaGithub } from 'react-icons/fa'

import { useCurrentTime } from '@/hooks/use-current-time'
import { useVersion } from '@/hooks/use-version'

const repositoryUrl = 'https://github.com/adamchairly/hirfolyam'

const iconLinkClassName = 'text-neutral-400 transition-colors hover:text-neutral-100'

export function SiteFooter() {
  const now = useCurrentTime()
  const commit = useVersion()

  return (
    <footer className="flex flex-col items-center gap-3 bg-neutral-900 px-4 py-6 text-center text-xs text-neutral-400 sm:px-6">
      <div className="flex items-center gap-4">
        <a href="/api/rss" target="_blank" aria-label="RSS" title="RSS" className={iconLinkClassName}>
          <Rss className="size-4" />
        </a>
        <a
          href={repositoryUrl}
          target="_blank"
          rel="noopener noreferrer"
          aria-label="GitHub"
          title="GitHub"
          className={iconLinkClassName}
        >
          <FaGithub aria-hidden="true" className="size-4" />
        </a>
      </div>

      <p>
        © {now.getFullYear()} hírfolyam
        {commit && (
          <>
            {' '}
            <a
              href={`${repositoryUrl}/commit/${commit}`}
              target="_blank"
              rel="noopener noreferrer"
              className="transition-colors hover:text-neutral-100 hover:underline"
            >
              {commit.slice(0, 7)}
            </a>
          </>
        )}
      </p>
    </footer>
  )
}
