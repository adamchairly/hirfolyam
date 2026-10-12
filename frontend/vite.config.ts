import { execFileSync } from 'node:child_process'
import { fileURLToPath, URL } from 'node:url'
import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// The version is the date and hash of the last commit
function readLastCommit(format: string): string {
  try {
    return execFileSync('git', ['log', '-1', '--date=format:%Y.%m.%d', `--format=${format}`], { encoding: 'utf8' }).trim()
  } catch {
    return ''
  }
}

// Docker builds have no git, only the SOURCE_COMMIT build arg
const sourceCommit = process.env.SOURCE_COMMIT?.trim() ?? ''
const buildDate = new Date().toISOString().slice(0, 10).replaceAll('-', '.')
const commitHash = readLastCommit('%H') || sourceCommit
const commitDate = readLastCommit('%cd') || (sourceCommit ? buildDate : '')

export default defineConfig({
  plugins: [react(), tailwindcss()],
  define: {
    __APP_VERSION__: JSON.stringify(commitDate || 'dev'),
    __APP_COMMIT__: JSON.stringify(commitHash),
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target: process.env.API_PROXY_TARGET ?? 'http://localhost:5080',
        changeOrigin: true,
      },
    },
  },
})
