import { test, expect } from '@playwright/test';

test.describe('Authentication Flow', () => {
  
  test.beforeEach(async ({ request }) => {
    // 1. Reset the database before each test
    const resetResponse = await request.post('http://localhost:7000/api/testing/reset');
    expect(resetResponse.ok()).toBeTruthy();
  });

  test('should login successfully via backdoor', async ({ page, request }) => {
    // 1. Get Token via Backdoor
    const loginResponse = await request.post('http://localhost:7000/api/testing/login', {
      data: { email: 'admin@test.com' },
      headers: { 'Content-Type': 'application/json' }
    });
    expect(loginResponse.ok()).toBeTruthy();
    const { token } = await loginResponse.json();

    // 2. Inject Token into LocalStorage
    await page.goto('/login'); // Go to login page first to set context
    await page.evaluate((t) => {
      localStorage.setItem('jwt_token', t);
    }, token);

    // 3. Reload/Navigate to verify session
    await page.goto('/');
    
    // 4. Verify we are on the dashboard (or at least NOT on login)
    await expect(page).toHaveURL('/');
    
    // Check for a specific element that only exists when logged in (e.g., User Menu or "My Teams")
    // Assuming "My Teams" or similar text exists on the dashboard
    await expect(page.locator('body')).not.toContainText('Sign in with GitHub');
  });

  test('should redirect to login when unauthenticated', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveURL(/\/login/);
  });
});
