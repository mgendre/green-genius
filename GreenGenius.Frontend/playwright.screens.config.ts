import {defineConfig, devices} from '@playwright/test';

export default defineConfig({
  testDir: './e2e/screens',
  testMatch: '**/*.shots.ts',
  outputDir: 'test-results/screens-run',
  forbidOnly: true,
  retries: 0,
  reporter: 'line',
  use: {baseURL: 'http://localhost:4301'},
  projects: [{name: 'chromium', use: {...devices['Desktop Chrome']}}],
  webServer: {
    command: 'npx ng serve --port 4301',
    url: 'http://localhost:4301',
    reuseExistingServer: false,
    timeout: 180_000,
  },
});
