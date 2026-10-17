import {defineConfig, devices} from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  forbidOnly: true,
  retries: 0,
  reporter: 'line',
  use: {
    baseURL: 'http://localhost:4300',
    trace: 'retain-on-failure',
  },
  projects: [{name: 'chromium', use: {...devices['Desktop Chrome']}}],
  webServer: {
    command: 'npx ng serve --port 4300',
    url: 'http://localhost:4300',
    reuseExistingServer: false,
    timeout: 180_000,
  },
});
