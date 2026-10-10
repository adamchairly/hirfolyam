import { Link } from 'react-router-dom'

export function DisabledSourcesMessage() {
  return (
    <p role="status" className="py-16 text-center text-neutral-400">
      Minden forrás ki van kapcsolva.{' '}
      <Link to="/sources" className="text-neutral-100 underline underline-offset-4">
        Kapcsolj be legalább egy forrást.
      </Link>
    </p>
  )
}
