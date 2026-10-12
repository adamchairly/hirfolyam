import { useCurrentTime } from '@/hooks/use-current-time'

const repositoryUrl = 'https://github.com/adamchairly/hirfolyam'

export function SiteFooter() {
  const now = useCurrentTime()

  return (
    <footer className="bg-neutral-900 px-4 py-6 text-center text-xs text-neutral-400 sm:px-6">
      © {now.getFullYear()} hírfolyam {__APP_VERSION__}
      {__APP_COMMIT__ && (
        <>
          {' '}
          <a
            href={`${repositoryUrl}/commit/${__APP_COMMIT__}`}
            target="_blank"
            rel="noopener noreferrer"
            className="transition-colors hover:text-neutral-100 hover:underline"
          >
            {__APP_COMMIT__.slice(0, 7)}
          </a>
        </>
      )}
    </footer>
  )
}
