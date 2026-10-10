export function toPlainText(value: string | null): string {
  if (!value) {
    return ''
  }

  const document = new DOMParser().parseFromString(value, 'text/html')

  for (const element of document.querySelectorAll('script, style, iframe, object')) {
    element.remove()
  }

  for (const element of document.querySelectorAll('p, div, br, li')) {
    element.append(document.createTextNode(' '))
  }

  return (document.body.textContent ?? '').replace(/\s+/g, ' ').trim()
}
