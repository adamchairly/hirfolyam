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

const commitDate = readLastCommit('%cd')

export default defineConfig({
  plugins: [react(), tailwindcss()],
  define: {
    __APP_VERSION__: JSON.stringify(commitDate || 'dev'),
    __APP_COMMIT__: JSON.stringify(readLastCommit('%H')),
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
