import { useLayoutEffect, useRef, useState, type RefObject } from 'react'

// Matches the aspect-[3/2] image and the line-clamp-3 lead of grid cards.
const imageHeightRatio = 2 / 3
const baseLeadLines = 3

export function useGridLeadClamp(enabled: boolean, leadRef: RefObject<HTMLParagraphElement | null>) {
  const articleRef = useRef<HTMLElement>(null)
  const [leadLines, setLeadLines] = useState<number | null>(null)

  useLayoutEffect(() => {
    const article = articleRef.current
    const paragraph = leadRef.current

    if (!enabled || !article || !paragraph) {
      setLeadLines(null)

      return
    }

    // Without an image, the lead may fill the space the image and its gap would take.
    function updateLeadLines() {
      if (!article || !paragraph) {
        return
      }

      const imageHeight = article.clientWidth * imageHeightRatio
      const gap = Number.parseFloat(window.getComputedStyle(article).rowGap)
      const lineHeight = Number.parseFloat(window.getComputedStyle(paragraph).lineHeight)

      setLeadLines(baseLeadLines + Math.floor((imageHeight + gap) / lineHeight))
    }

    const observer = new ResizeObserver(updateLeadLines)

    observer.observe(article)
    updateLeadLines()

    return () => {
      observer.disconnect()
    }
  }, [enabled, leadRef])

  return {
    articleRef,
    leadLines,
  }
}
