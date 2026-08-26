import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  retries: 0,
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
  },
  webServer: [
    {
      command: 'dotnet run --no-launch-profile --project src/TodoCharacters.WebApi/TodoCharacters.WebApi.csproj --urls http://localhost:5000',
      url: 'http://localhost:5000/api/tasks',
      reuseExistingServer: !process.env.CI,
      env: { ASPNETCORE_ENVIRONMENT: 'Testing' },
      cwd: '../..',
    },
    {
      command: 'npm run dev',
      url: 'http://localhost:5173',
      reuseExistingServer: !process.env.CI,
    },
  ],
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
});
