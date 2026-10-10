import { useLayoutEffect, useRef, useState } from 'react'

export function useArticleLeadClamp(hasImage: boolean, lead: string) {
  const imageRef = useRef<HTMLDivElement>(null)
  const titleRef = useRef<HTMLHeadingElement>(null)
  const metadataRef = useRef<HTMLParagraphElement>(null)
  const leadRef = useRef<HTMLParagraphElement>(null)
  const [leadLines, setLeadLines] = useState<number | null>(null)

  useLayoutEffect(() => {
    const image = imageRef.current
    const title = titleRef.current
    const metadata = metadataRef.current
    const paragraph = leadRef.current

    if (!hasImage || !image || !title || !metadata || !paragraph) {
      setLeadLines(null)

      return
    }

    function updateLeadLines() {
      if (!image || !title || !metadata || !paragraph) {
        return
      }

      const imageBounds = image.getBoundingClientRect()
      const titleBounds = title.getBoundingClientRect()

      if (titleBounds.top >= imageBounds.bottom) {
        setLeadLines(null)

        return
      }

      const paragraphStyle = window.getComputedStyle(paragraph)
      const metadataStyle = window.getComputedStyle(metadata)
      const lineHeight = Number.parseFloat(paragraphStyle.lineHeight)
      const leadMargin = Number.parseFloat(paragraphStyle.marginTop)
      const metadataMargin = Number.parseFloat(metadataStyle.marginTop)
      const availableHeight = imageBounds.bottom
        - titleBounds.bottom
        - metadata.getBoundingClientRect().height
        - metadataMargin
        - leadMargin
      const lines = Math.max(0, Math.floor(availableHeight / lineHeight))

      setLeadLines(lines)
    }

    const observer = new ResizeObserver(updateLeadLines)

    observer.observe(image)
    observer.observe(title)
    observer.observe(metadata)
    updateLeadLines()

    return () => {
      observer.disconnect()
    }
  }, [hasImage, lead])

  return {
    imageRef,
    titleRef,
    metadataRef,
    leadRef,
    leadLines,
  }
}
