import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { SourcesProvider } from '@/components/sources-provider'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <SourcesProvider>
      <App />
    </SourcesProvider>
  </StrictMode>,
)
