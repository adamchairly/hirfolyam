import { useCurrentTime } from '@/hooks/use-current-time'
import { useVersion } from '@/hooks/use-version'

const repositoryUrl = 'https://github.com/adamchairly/hirfolyam'

export function SiteFooter() {
  const now = useCurrentTime()
  const commit = useVersion()

  return (
    <footer className="bg-neutral-900 px-4 py-6 text-center text-xs text-neutral-400 sm:px-6">
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
    </footer>
  )
}
