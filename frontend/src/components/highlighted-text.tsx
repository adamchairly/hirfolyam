interface HighlightedTextProps {
  text: string
  search: string
}

export function HighlightedText({ text, search }: HighlightedTextProps) {
  const query = search.trim()

  if (!query) {
    return text
  }

  const escapedQuery = query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
  const parts = text.split(new RegExp(`(${escapedQuery})`, 'giu'))

  return parts.map((part, index) => {
    if (index % 2 === 0) {
      return part
    }

    return (
      <mark key={index} className="rounded-sm bg-amber-300 text-neutral-950">
        {part}
      </mark>
    )
  })
}
