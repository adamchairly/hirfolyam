interface BookmarkIconProps {
  filled?: boolean
  className?: string
}

export function BookmarkIcon({ filled = false, className }: BookmarkIconProps) {
  return (
    <svg
      viewBox="0 0 24 24"
      fill={filled ? 'currentColor' : 'none'}
      stroke="currentColor"
      strokeWidth={1.75}
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      className={className}
    >
      <path d="M7 3.5h10a1 1 0 0 1 1 1v16l-6-4.5-6 4.5v-16a1 1 0 0 1 1-1Z" />
    </svg>
  )
}
